using System;
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
    public struct SerializableDict<K, V>
    {
        [SerializeField]
        public SerializableKeyValuePair<K, V>[] Pairs;

        public override string ToString()
        {
            return $"{string.Join(", ", Pairs ?? Array.Empty<SerializableKeyValuePair<K,V>>())}";
        }
    }
}