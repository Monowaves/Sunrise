using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using MonoWaves.QoL;

public class PaletteGeneratorWindow : EditorWindow
{
    private const string ASSEMBLY_DEFENITION = "UnityEditor.{0},UnityEditor";

    private List<string> _loaded = new();
    private string _loadedPalleteName;

    [MenuItem("Window/Monowaves/QoL/Palette Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<PaletteGeneratorWindow>("Palette Generator");
        window.maxSize = new Vector2(300, 0);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(12);

        if (GUILayout.Button("Load Palette"))
        {
            LoadPallete();
        }

        EditorGUILayout.Space(3);

        if (_loaded.Count > 0)
        {
            GUIStyle textStyle = new(EditorStyles.label);
            textStyle.normal.textColor = Color.green;

            EditorGUILayout.LabelField($"Pallete {_loadedPalleteName} loaded!", textStyle);
        }
        else
            EditorGUILayout.LabelField("No pallete loaded.");

        EditorGUILayout.Space(12);

        if (_loaded.Count > 0)
        {
            if (GUILayout.Button("Generate Library"))
            {
                GenerateLibrary();
            }
        }
    }

    private void LoadPallete()
    {
        string palettePath = EditorUtility.OpenFilePanel("Select palette", "", "gpl");

        if (!string.IsNullOrEmpty(palettePath))
        {
            _loaded = File.ReadLines(palettePath).ToList();
            _loadedPalleteName = _loaded.Find(line => line.StartsWith("#Palette Name:")).Replace("#Palette Name: ", "");
        }
    }

    private void GenerateLibrary()
    {
        List<Color> colors = new();

        foreach (var line in _loaded)
        {
            if (!line.StartsWith('#') && line != _loaded.First())
            {
                string[] rgbhex = line.Split();
                Color color = $"#{rgbhex.Last()}".HexToColor();

                Debug.Log(rgbhex.Last());

                colors.Add(color);
            }
        }

        colors.Add(Color.white);

        CreateNewLibrary(_loadedPalleteName, colors);
    }

    private void CreateNewLibrary(string name, List<Color> colors)
    {
        // The ScriptableSingleton class is public, but because PresetLibraryManager isn't
        // we still need to make a generic type and then use reflection to get the static instance property.
        // This is assuming that we need the singleton instance for library registration purposes -
        // it might not be necessary.
        Type managerType = Type.GetType(string.Format(ASSEMBLY_DEFENITION, "PresetLibraryManager"));
        Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(managerType);
        PropertyInfo instancePropertyInfo = singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        var managerInstance = instancePropertyInfo.GetValue(null, null);

        // We create an instance of the save/load helper and then pass it and the path to the CreateLibrary method.
        // "colors" is the file extension we want for the library asset without the '.'
        Type libraryType = Type.GetType(string.Format(ASSEMBLY_DEFENITION, "ColorPresetLibrary"));
        Type helperType = Type.GetType(string.Format(ASSEMBLY_DEFENITION, "ScriptableObjectSaveLoadHelper`1"))
                                .MakeGenericType(libraryType);
        MethodInfo createMethod = managerType.GetMethod("CreateLibrary", BindingFlags.Instance | BindingFlags.Public)
                                                .MakeGenericMethod(libraryType);
        var helper = Activator.CreateInstance(helperType, new object[] { "colors", SaveType.Text });
        var library = createMethod.Invoke(managerInstance, new object[] {helper, Path.Combine("Assets/Editor", name) });

        // We can't cast library to the desired type so we get the Add method through reflection
        // and add the desired colours as presets through that.
        // After that the library can be saved!
        if ((UnityEngine.Object) library != null)
        {
            MethodInfo addPresetMethod = libraryType.GetMethod("Add");
            foreach(var color in colors)
            {
            addPresetMethod.Invoke(library, new object[] {color, color.ToString()});
            }

            MethodInfo saveMethod = managerType.GetMethod("SaveLibrary", BindingFlags.Instance | BindingFlags.Public)
                                            .MakeGenericMethod(libraryType);
            saveMethod.Invoke(managerInstance, new object[] { helper, library, Path.Combine("Assets/Editor", name) });
        }

        // The library could be returned as an Object or ScriptableObject reference if that was useful
        // I don't know of a way to cast it to the actual ColorPresetLibrary type.
    }
}
