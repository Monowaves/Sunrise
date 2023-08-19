using UnityEditor;
using UnityEngine;
using InMotion;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

[System.Serializable]
[CustomEditor(typeof(InMotion.Motion))]
public class MotionEditor : Editor
{
    InMotion.Motion current;

    private SerializedProperty _useCustomFramerate;
    private SerializedProperty _framerate;
    private SerializedProperty _variants;
    private SerializedProperty _looping;

    private AnimBool _animUseCustomFramerate;

    private void OnEnable() 
    {
        current = (InMotion.Motion)target;

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
}
