using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using InMotion.Data.Save;

namespace InMotion.Engine
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Tree", menuName = "InMotion/Motion Tree", order = 404)]
    public class MotionTree : ScriptableObject
    {
        [field: SerializeField] public MotionTreeParameters Parameters { get; private set; }
        public MotionTreeSaveData SavedData;

        public (string key, object value)[] ZipParameters(Dictionary<string, string> parameters)
        {
            var keys = parameters.Keys.ToList();
            var values = parameters.Values.Cast<object>().ToList();

            return keys.Zip(values, (x, y) => ValueTuple.Create(x, y)).ToArray();
        }

        public (string, object)[] RegisteredParameters => ZipParameters(Parameters.ToDictionary(entry => entry.Key, entry => entry.Value));
    }

    [Serializable]
    public class MotionTreeParameters : InMotionDictionary<string, string> {}
}
