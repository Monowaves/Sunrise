using System.Collections.Generic;
using InMotion.Enumerations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InMotion.EditorOnly.Windows
{
    public class MotionTreeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private MotionTreeGraphView _graphView;
        private Texture2D _indentationIcon;
        
        public void Initialize(MotionTreeGraphView graphView)
        {
            _graphView = graphView;
            
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }
    
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Insert Node")),
    
                new SearchTreeGroupEntry(new GUIContent("Animation Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Motion Node", _indentationIcon))
                {
                    level = 2,
                    userData = InMotionType.Motion
                },
    
                new SearchTreeGroupEntry(new GUIContent("Logic Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Branch Node", _indentationIcon))
                {
                    level = 2,
                    userData = InMotionType.Branch
                },
                new SearchTreeEntry(new GUIContent("Return Node", _indentationIcon))
                {
                    level = 2,
                    userData = InMotionType.Return
                },
    
                new SearchTreeGroupEntry(new GUIContent("Groups"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", _indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                },
            };
    
            return searchTreeEntries;
        }
    
        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 targetMouse = _graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case InMotionType.Motion:

                    Node nodeM = _graphView.CreateNode("Sample Motion", InMotionType.Motion, targetMouse);
                    _graphView.AddElement(nodeM);

                    return true;
    
                case InMotionType.Branch:

                    Node nodeB = _graphView.CreateNode("Sample Branch", InMotionType.Branch, targetMouse);
                    _graphView.AddElement(nodeB);

                    return true;
    
                case InMotionType.Return:

                    Node nodeR = _graphView.CreateNode("Return", InMotionType.Return, targetMouse);
                    _graphView.AddElement(nodeR);

                    return true;
    
                case Group _:

                    _graphView.CreateGroup("Hey look! I create this from Search Window!", targetMouse);

                    return true;

                default:
                    return false;
            }
        }
    }
    
}
