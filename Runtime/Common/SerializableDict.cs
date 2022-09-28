using System;
using System.Collections.Generic;
using UnityEngine;

namespace PluginSet.Core
{
    [Serializable]
    public struct SerializableKeyValuePair<K, V>
    {
        [SerializeField]
        public K Key;

        [SerializeField]
        public V Value;

        public override string ToString()
        {
            return $"[{Key?.ToString() ?? "NULL"}]: {Value?.ToString() ?? "NULL"}";
        }
    }

    [Serializable]
    public class SerializableDict<K, V>
    {
        [SerializeField]
        public SerializableKeyValuePair<K, V>[] Pairs;

        public SerializableDict()
        {
            
        }
        
        public SerializableDict(Dictionary<K, V> dictionary)
        {
            Pairs = new SerializableKeyValuePair<K, V>[dictionary.Count];
            var iter = dictionary.GetEnumerator();
            var index = 0;
            while (iter.MoveNext())
            {
                var kv = iter.Current;
                Pairs[index++] = new SerializableKeyValuePair<K, V>()
                {
                    Key = kv.Key,
                    Value = kv.Value,
                };
            }
            iter.Dispose();
        }

        public override string ToString()
        {
            return $"{string.Join(", ", Pairs ?? Array.Empty<SerializableKeyValuePair<K,V>>())}";
        }
    }
}