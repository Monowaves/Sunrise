using UnityEngine;

namespace InMotion.SO
{
    [System.Serializable]
    public class MotionNodeScriptableObject : NodeScriptableObject
    {
        [field: SerializeField] public Engine.Motion TargetMotion { get; set; }
        [field: SerializeField] public bool Transition { get; set; }
        [field: SerializeField] public NodeScriptableObject Next { get; set; }
    }
}
