using System.Collections.Generic;
using UnityEngine;

namespace InMotion.Engine
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Variant", menuName = "InMotion/Variant", order = 402)]
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
