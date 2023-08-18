using System.Collections;
using System.Collections.Generic;
using InMotion.GraphElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InMotion.Data.Error
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
