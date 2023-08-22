using System.Linq;
using InMotion.Engine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace InMotion.EditorOnly.Drawers
{
    [CustomEditor(typeof(MotionExecutor))]
    public class MotionExecutorEditor : Editor
    {
        private MotionExecutor _current;
    
        private SerializedProperty _motionTree;
        private SerializedProperty _target;
        private SerializedProperty _framerate;
        private SerializedProperty _callbacks;
        
        private void OnEnable() 
        {
            _current = (MotionExecutor)target;
    
            _motionTree = serializedObject.FindProperty("MotionTree");
            _target = serializedObject.FindProperty("Target");
            _framerate = serializedObject.FindProperty("Framerate");
            _callbacks = serializedObject.FindProperty("Callbacks");
        }
    
        public override void OnInspectorGUI()
        {
            DrawMotionTreeField();
    
            GUILayout.Space(10f);
            DrawProperties();
    
            GUILayout.Space(10f);
            DrawCallbacks();
    
            serializedObject.ApplyModifiedProperties();
        }
    
        private void DrawMotionTreeField()
        {
            _motionTree.objectReferenceValue = EditorGUILayout.ObjectField
            (
                _motionTree.objectReferenceValue,
                typeof(MotionTree),
                allowSceneObjects: false
            );
        }
    
        private void DrawProperties()
        {
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
    
            EditorGUILayout.PropertyField(_target);
            EditorGUILayout.PropertyField(_framerate);
        }
    
        private void DrawCallbacks()
        {
            foreach (var callback in _current.Callbacks)
            {
                EditorGUILayout.TextField(callback.Key);
            }
    
            if (GUILayout.Button("Add"))
            {
                _current.Callbacks.Add("new callback", new UnityEvent());
            }
    
            if (GUILayout.Button("Remove"))
            {
                _current.Callbacks.Remove(_current.Callbacks.Last().Key);
            }
        }
    }
}
