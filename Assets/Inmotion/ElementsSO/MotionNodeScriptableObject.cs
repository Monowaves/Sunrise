using InMotion.SO;
using UnityEngine;

[System.Serializable]
public class MotionNodeScriptableObject : NodeScriptableObject
{
    [field: SerializeField] public InMotion.Motion TargetMotion { get; set; }
    [field: SerializeField] public NodeScriptableObject Next { get; set; }
}
