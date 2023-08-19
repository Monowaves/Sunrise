using System.Collections.Generic;
using UnityEngine;

namespace InMotion
{   
    [System.Serializable]
    [CreateAssetMenu(menuName = "InMotion/Motion")]
    public class Motion : ScriptableObject
    {
        [HideInInspector] public bool UseCustomFramerate = false;

        [HideInInspector] public int Framerate = 20;

        [HideInInspector] public List<Variant> Variants = new();
    }
}
