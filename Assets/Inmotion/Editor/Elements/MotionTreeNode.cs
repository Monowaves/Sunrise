using InMotion.Enumerations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using InMotion.EditorOnly.Utilities;
using InMotion.EditorOnly.Windows;
using InMotion.Utilities;

namespace InMotion.EditorOnly.GraphElements
{
    public class MotionTreeNode : Node
    {
        public string ID { get; set; }
        public string NodeName { get; set; }
        public InMotionType NodeType { get; set; }

        private Color _defaultBGColor;

        public MotionTreeGraphView GraphView;

        public MotionTreeGroup Group { get; set; }

        public virtual void Initialize(string nodeName, MotionTreeGraphView graphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            NodeName = nodeName;
            GraphView = graphView;
            _defaultBGColor = new Color(51f / 255f, 51f / 255f, 51f / 255f);

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("mott-node__main-container");
            extensionContainer.AddToClassList("mott-node__extension-container");

            titleContainer.AddToClassList("mott-node__title_container");
            inputContainer.AddToClassList("mott-node__input-container");
            outputContainer.AddToClassList("mott-node__output-container");
        }

        public virtual void Draw()
        {
            TextField motionName = InMotionElementUtility.InsertTextField(NodeName, onValueChanged =>
            {
                TextField target = (TextField)onValueChanged.target;
                target.value = onValueChanged.newValue.RemoveSpecialCharacters();

                if (Group != null)
                {
                    MotionTreeGroup currentGroup = Group;

                    GraphView.RemoveGroupedNode(this, Group);

                    NodeName = target.value;

                    GraphView.AddGroupedNode(this, currentGroup);
                }
                else
                {
                    GraphView.RemoveUngroupedNode(this);
        
                    NodeName = target.value;
        
                    GraphView.AddUngroupedNode(this);
                }

            }, "mott-node__nodename-textfield");

            titleContainer.Insert(0, motionName);

            Port input = this.InsertPort(Direction.Input, "Input", capacity: Port.Capacity.Multi);
            inputContainer.Add(input);
        }

        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                GraphView.DeleteElements(port.connections);
            }
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = _defaultBGColor;
        }
    }
}
