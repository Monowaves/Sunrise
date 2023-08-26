using System.Collections.Generic;
using UnityEngine;

namespace InMotion.Engine
{   
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Motion", menuName = "InMotion/Motion", order = 403)]
    public class Motion : ScriptableObject
    {
        [HideInInspector] public bool UseCustomFramerate = false;
        [HideInInspector] public int Framerate = 20;
        [HideInInspector] public bool Looping = true;
        [HideInInspector] public List<Variant> Variants = new();
    }
}
