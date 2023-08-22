using UnityEngine;

namespace InMotion.SO
{
    [System.Serializable]
    public class BranchNodeScriptableObject : NodeScriptableObject
    {
        [field: SerializeField] public string Condition { get; set; }
        [field: SerializeField] public NodeScriptableObject True { get; set; }
        [field: SerializeField] public NodeScriptableObject False { get; set; }
    }
}
