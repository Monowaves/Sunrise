using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InMotion.SO
{
    public class NodeContainerScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public InMotionDictionary<GroupScriptableObject, List<NodeScriptableObject>> MotionTreeGroups { get; set; }
        [field: SerializeField] public List<NodeScriptableObject> UngroupedNodes { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            MotionTreeGroups = new InMotionDictionary<GroupScriptableObject, List<NodeScriptableObject>>();
            UngroupedNodes = new List<NodeScriptableObject>();
        }
    }
}
