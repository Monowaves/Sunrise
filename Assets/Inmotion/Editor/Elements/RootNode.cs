using InMotion.EditorOnly.Utilities;
using InMotion.EditorOnly.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace InMotion.EditorOnly.GraphElements
{
    public class RootNode : Node
    {
        public void Initialize(MotionTreeGraphView graphView, Vector2 position)
        {
            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("mott-node__main-container");
            extensionContainer.AddToClassList("mott-node__extension-container");

            titleContainer.AddToClassList("mott-node__title_container");
            inputContainer.AddToClassList("mott-node__input-container");
            outputContainer.AddToClassList("mott-node__output-container");
        }
    
        public void Draw()
        {
            Label root = new Label("Root");
            titleContainer.Insert(0, root);
            root.AddToClassList("mott-node_root-text");

            Port next = this.InsertPort(Direction.Output, "Next", capacity: Port.Capacity.Single);
            outputContainer.Add(next);

            RefreshExpandedState();
        }
    }
}
