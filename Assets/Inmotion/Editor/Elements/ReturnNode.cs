using InMotion.EditorOnly.Windows;
using UnityEngine;

namespace InMotion.EditorOnly.GraphElements
{
    public class ReturnNode : MotionTreeNode
    {
        public override void Initialize(string nodeName, MotionTreeGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            NodeType = Enumerations.InMotionType.Return;
        }

        public override void Draw()
        {
            base.Draw();

            RefreshExpandedState();
        }
    }
}
