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
        public MotionTreeParameter InspectorParameters;
        public MotionTreeSaveData SavedData;

        public void SetParameter(string key, string value)
        {
            InspectorParameters[key] = value;
        }

        public List<(string, object)> RegisteredParameters
        {
            get { return RegisteredParametersNames.Zip(RegisteredParametersValues, (x, y) => ValueTuple.Create(x, y)).ToList(); }
        }

        public List<string> RegisteredParametersNames
        {
            get { return InspectorParameters.Keys.ToList(); }
        }

        public List<object> RegisteredParametersValues
        {
            get { return InspectorParameters.Values.Cast<object>().ToList(); }
        }
    }

    [Serializable]
    public class MotionTreeParameter : SerializableDictionary<string, string> {} //kiki
}
