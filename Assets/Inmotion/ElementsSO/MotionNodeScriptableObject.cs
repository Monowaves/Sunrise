using UnityEngine;

namespace InMotion.SO
{
    [System.Serializable]
    public class MotionNodeScriptableObject : NodeScriptableObject
    {
        [field: SerializeField] public InMotion.Engine.Motion TargetMotion { get; set; }
        [field: SerializeField] public NodeScriptableObject Next { get; set; }
    }
}
