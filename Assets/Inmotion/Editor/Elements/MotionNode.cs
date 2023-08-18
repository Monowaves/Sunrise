using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using InMotion.Utilities;

namespace InMotion.GraphElements
{
    public class MotionNode : MotionTreeNode
    {
        public Port NextPort { get; set; }

        public Motion TargetMotion { get; set; }
        
        private ObjectField _targetMotion;

        public override void Initialize(string nodeName, MotionTreeGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            NodeType = Enumerations.InMotionType.Motion;
        }

        public override void Draw()
        {
            base.Draw();

            NextPort = this.InsertPort(Direction.Output, "Next", capacity: Port.Capacity.Single);;
            outputContainer.Add(NextPort);

            _targetMotion = InMotionElementUtility.InsertObjectField(typeof(Motion), "Target Motion", TargetMotion, onValueChanged =>
                TargetMotion = (Motion)_targetMotion.value
            );

            extensionContainer.Add(_targetMotion);

            RefreshExpandedState();
        }
    }
}
