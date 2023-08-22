using InMotion.Enumerations;
using UnityEngine;

namespace InMotion.SO
{
    [System.Serializable]
    public class NodeScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string NodeName { get; set; }
        [field: SerializeField] public InMotionType NodeType { get; set; }
    
        public void Initialize(string nodeName, InMotionType type)
        {
            NodeName = nodeName;
            NodeType = type;
        }
    }
}
