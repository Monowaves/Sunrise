using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace InMotion.Engine
{
    [Serializable]
    public class Callbacks : SerializableDictionary<string, UnityEvent>, ISerializationCallbackReceiver
    {
        [SerializeField]
        public List<string> CallbacksKeys = new();

        [SerializeField]
        public List<UnityEvent> CallbacksValues = new();

        public new void OnBeforeSerialize()
        {
            CallbacksKeys = Keys.ToList();
            CallbacksValues = Values.ToList();
        }

        public new void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < Keys.Count; i++)
            {
                Add(CallbacksKeys[i], CallbacksValues[i]);
            }
        }
    }
}
