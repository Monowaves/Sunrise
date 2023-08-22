using System.Collections.Generic;
using InMotion.EditorOnly.GraphElements;

namespace InMotion.EditorOnly.Data.Error
{
    public class NodeErrorData
    {
        public InMotionErrorData ErrorData { get; set; }
        public List<MotionTreeNode> Nodes { get; set; }

        public NodeErrorData()
        {
            ErrorData = new InMotionErrorData();
            Nodes = new List<MotionTreeNode>();
        }
    }
}
