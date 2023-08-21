using System.Collections.Generic;
using UnityEngine;

namespace InMotion
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "InMotion/Variant")]
    public class Variant : ScriptableObject
    {
        public MotionDirections Directions = MotionDirections.Simple;

        public List<Frame> FramesContainer = new();
    }

    public enum MotionDirections
    {
        Simple,
        Platformer,
        FourDirectional,
        EightDirectional
    }
}
