using System.Collections.Generic;
using InMotion.EditorOnly.GraphElements;

namespace InMotion.EditorOnly.Data.Error
{
    public class GroupErrorData
    {
        public InMotionErrorData ErrorData { get; set; }
        public List<MotionTreeGroup> Groups { get; set; }

        public GroupErrorData()
        {
            ErrorData = new InMotionErrorData();
            Groups = new List<MotionTreeGroup>();
        }
    }
}
