using InMotion.EditorOnly.Utilities;
using InMotion.EditorOnly.Windows;
using InMotion.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace InMotion.EditorOnly.GraphElements
{
    public class BranchNode : MotionTreeNode
    {
        public Port OutputTrue { get; set; }
        public Port OutputFalse { get; set; }

        public string Condition { get; set; }

        private TextField _condition;

        public override void Initialize(string nodeName, MotionTreeGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            NodeType = Enumerations.InMotionType.Branch;
        }

        public override void Draw()
        {
            base.Draw();

            OutputTrue = this.InsertPort(Direction.Output, "True", capacity: Port.Capacity.Single);
            outputContainer.Add(OutputTrue);

            OutputFalse = this.InsertPort(Direction.Output, "False", capacity: Port.Capacity.Single);
            outputContainer.Add(OutputFalse);

            Label label = new Label("Condition");

            extensionContainer.Add(label);

            _condition = InMotionElementUtility.InsertTextField(Condition, onValueChanged =>
                Condition = onValueChanged.newValue
            );
            extensionContainer.Add(_condition);

            Button check = InMotionElementUtility.InsertButton("Check", CheckCondition);
            extensionContainer.Add(check);
            
            RefreshExpandedState();
        }

        private void CheckCondition()
        {
            (string, object)[] registeredParameters = GraphView.MotionTreeWindow.CurrentMotionTree.RegisteredParameters;

            if (Conditioner.StringToCondition(Condition, registeredParameters))
            {
                Debug.Log("true");
            }
            else
            {
                Debug.Log("false");
            }
        }
    }
}
