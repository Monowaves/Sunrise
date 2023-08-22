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

        private string _newCallbackName;
        
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
            EditorGUILayout.PropertyField(_callbacks, EditorStyles.boldFont);
        }
    }
}
