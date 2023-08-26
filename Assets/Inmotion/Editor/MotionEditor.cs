using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using InMotion.Engine;
using System.IO;

namespace InMotion.EditorOnly.Drawers
{
    [System.Serializable]
    [CustomEditor(typeof(Engine.Motion))]
    public class MotionEditor : Editor
    {
        private SerializedProperty _useCustomFramerate;
        private SerializedProperty _framerate;
        private SerializedProperty _variants;
        private SerializedProperty _looping;
    
        private AnimBool _animUseCustomFramerate;
    
        private void OnEnable() 
        {
            _useCustomFramerate = serializedObject.FindProperty("UseCustomFramerate");
            _framerate = serializedObject.FindProperty("Framerate");
            _variants = serializedObject.FindProperty("Variants");
            _looping = serializedObject.FindProperty("Looping");
    
            _animUseCustomFramerate = new AnimBool(_useCustomFramerate.boolValue);
            _animUseCustomFramerate.valueChanged.AddListener(Repaint);
        }
    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    
            _useCustomFramerate.boolValue = EditorGUILayout.Toggle("Use Custom Framerate", _useCustomFramerate.boolValue);
            _animUseCustomFramerate.target = _useCustomFramerate.boolValue;
    
            if (EditorGUILayout.BeginFadeGroup(_animUseCustomFramerate.faded))
            {
                EditorGUILayout.BeginHorizontal();
    
                GUILayout.Space(30);
    
                _framerate.intValue = EditorGUILayout.IntField("Framerate", _framerate.intValue);
    
                EditorGUILayout.EndHorizontal();
            }
    
            EditorGUILayout.EndFadeGroup();
    
            _looping.boolValue = EditorGUILayout.Toggle("Looping", _looping.boolValue);
            
            GUILayout.Space(10);
    
            EditorGUILayout.PropertyField(_variants);
    
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Assets/Create/InMotion/Motion (From Variant)", false, 401)]
        public static void MotionFromVariantCreation()
        {
            string origin = AssetDatabase.GetAssetPath(Selection.activeObject);
            string directory = Path.GetDirectoryName(origin);
            string variantName = Path.GetFileName(origin);
            string motionName = variantName.Replace("Variant", "Motion");
            string fullPath = $"{directory}/{motionName}";

            if (!string.IsNullOrEmpty(fullPath))
            {
                Engine.Motion newObject = CreateInstance<Engine.Motion>();

                Variant current = Selection.activeObject as Variant;
                newObject.Variants.Add(current);

                AssetDatabase.CreateAsset(newObject, fullPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = newObject;
            }
        }

        [MenuItem("Assets/Create/InMotion/Motion (From Variant)", true, 401)]
        private static bool MotionFromVariantValidation()
        {
            return Selection.activeObject is Variant;
        }
    }
}
