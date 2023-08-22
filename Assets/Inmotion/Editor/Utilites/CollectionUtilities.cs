using System.Collections.Generic;

namespace InMotion.EditorOnly.Utilities
{
    public static class CollectionUtility
    {
        public static void AddItem<K, V>(this InMotionDictionary<K, List<V>> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);

                return;
            }

            dictionary.Add(key, new List<V>() { value });
        }
    }
}
