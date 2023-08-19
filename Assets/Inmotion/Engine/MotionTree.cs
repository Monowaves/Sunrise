using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using InMotion.Data.Save;

namespace InMotion
{
    [Serializable]
    [CreateAssetMenu(menuName = "InMotion/Motion Tree")]
    public class MotionTree : ScriptableObject
    {
        [field: SerializeField] public MotionTreeParameters Parameters { get; private set; }
        public MotionTreeSaveData SavedData;

        public List<(string, object)> RegisteredParameters
        {
            get { return RegisteredParametersNames.Zip(RegisteredParametersValues, (x, y) => ValueTuple.Create(x, y)).ToList(); }
        }

        public List<string> RegisteredParametersNames
        {
            get { return Parameters.Keys.ToList(); }
        }

        public List<object> RegisteredParametersValues
        {
            get { return Parameters.Values.Cast<object>().ToList(); }
        }
    }

    [Serializable]
    public class MotionTreeParameters : SerializableDictionary<string, string> {}
}
