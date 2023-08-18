using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using System;
using UnityEngine;
using InMotion.Utilities;
using UnityEditor.UIElements;

namespace InMotion
{
    public class MotionTreeEditorWindow : EditorWindow
    {
        private MotionTreeGraphView _graphView;

        public MotionTree CurrentMotionTree;
        public string CurrentMotionTreeFullpath;
        public string CurrentMotionTreePath;
        public string CurrentMotionTreeName;
        
        private Button _saveButton;

        [OnOpenAsset(1)]
        public static bool TryOpenMotionTree(int instanceID, int line)
        {
            UnityEngine.Object item = EditorUtility.InstanceIDToObject(instanceID);
            bool isMotionTree = item is MotionTree;

            if (isMotionTree)
            {
                MotionTreeEditorWindow window = GetWindow<MotionTreeEditorWindow>();
                window.titleContent = new GUIContent(item.name);
                window.CurrentMotionTree = (MotionTree)item;
                window.CurrentMotionTreePath = GetFullpathFromMotionTree((MotionTree)item).assetPath;
                window.CurrentMotionTreeName = GetFullpathFromMotionTree((MotionTree)item).assetName;
                window.CurrentMotionTreeFullpath = GetFullpathFromMotionTree((MotionTree)item).assetFullpath;

                window.Load(); //bgf
    
                return true;
            }
    
            return false;
        }

        public static (string assetPath, string assetName, string assetFullpath) GetFullpathFromMotionTree(MotionTree tree)
        {
            string[] seperated = AssetDatabase.GetAssetPath(tree).Split('/');
            string path = AssetDatabase.GetAssetPath(tree).Replace($"/{seperated[seperated.Length - 1]}", String.Empty);

            return (path, tree.name, AssetDatabase.GetAssetPath(tree));
        }

        private void CreateGUI()
        {   
            MotionTreeEditorWindow window = GetWindow<MotionTreeEditorWindow>();

            window.AddGraphView();
            window.AddToolbar();

            window.AddStyles();

            window.Load(); //скрруd
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("MotionTree/MotionTreeVariables.uss");

            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void AddGraphView()
        {
            _graphView = new MotionTreeGraphView(this, CurrentMotionTreePath);

            _graphView.StretchToParentSize();

            rootVisualElement.Add(_graphView);
        }

        private void AddToolbar()
        {
            MotionTreeEditorWindow window = GetWindow<MotionTreeEditorWindow>();

            Toolbar toolbar = new Toolbar();

            _saveButton = InMotionElementUtility.InsertButton("Save", () => window.Save());
            toolbar.Add(_saveButton); 

            Button reloadButton = InMotionElementUtility.InsertButton("Reload", () => window.Load());
            toolbar.Add(reloadButton); 

            Button clearButton = InMotionElementUtility.InsertButton("Clear", () => window.Clear());
            toolbar.Add(clearButton); 

            Button destroyButton = InMotionElementUtility.InsertButton("Destroy", () => window.DestroyAllSavings(window.CurrentMotionTree));
            toolbar.Add(destroyButton); 

            StyleSheet style = (StyleSheet)EditorGUIUtility.Load("MotionTree/MotionTreeToolbarStyles.uss");
            toolbar.styleSheets.Add(style);

            rootVisualElement.Add(toolbar);
        }

        private void Clear()
        {
            _graphView?.ClearGraph();
        }

        private void DestroyAllSavings(MotionTree item)
        {
            Close();
            InMotionIOUtility.DestroySave(item);
        }

        private void Load()
        {
            string assetName = CurrentMotionTreeName + "Graph";

            Clear();

            InMotionIOUtility.Initialize(_graphView, assetName);
            InMotionIOUtility.Load();
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(CurrentMotionTreeName))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name",
                    "Cant save the graph",
                    "No problem bro"
                );

                return;
            }

            ///Debug.Log(_currentMotionTreeFullpath);
            
            InMotionIOUtility.Initialize(_graphView, CurrentMotionTreeName);
            InMotionIOUtility.Save(CurrentMotionTreeFullpath);
        }

        public void SwitchSaving(bool enable)
        {
            _saveButton.SetEnabled(enable);
        }
    }
}