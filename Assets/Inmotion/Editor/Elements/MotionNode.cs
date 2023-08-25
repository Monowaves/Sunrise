using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using InMotion.EditorOnly.Utilities;
using InMotion.EditorOnly.Windows;
using UnityEngine.UIElements;

namespace InMotion.EditorOnly.GraphElements
{
    public class MotionNode : MotionTreeNode
    {
        public Port NextPort { get; set; }

        public Engine.Motion TargetMotion { get; set; }
        
        private ObjectField _targetMotion;
        private Toggle _transition;

        public override void Initialize(string nodeName, MotionTreeGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            NodeType = Enumerations.InMotionType.Motion;
        }

        public override void Draw()
        {
            base.Draw();

            NextPort = this.InsertPort(Direction.Output, "Next", capacity: Port.Capacity.Single);
            outputContainer.Add(NextPort);

            _targetMotion = InMotionElementUtility.InsertObjectField(typeof(Motion), TargetMotion, onValueChanged =>
                TargetMotion = (Engine.Motion)_targetMotion.value
            );
            extensionContainer.Add(_targetMotion);

            RefreshExpandedState();
        }
    }
}
