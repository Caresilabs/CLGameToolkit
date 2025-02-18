using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLGameToolkit.Serialization
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new();

        [SerializeField]
        private List<TValue> values = new();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            if (!Application.isEditor && keys.Count != values.Count)
            {
                throw new Exception(string.Format("The number of keys ({0}) and values ({1}) does not match", keys.Count, values.Count));
            }

            Clear();

            for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++) // Editor hack
            {
                Add(keys[i], values[i]);
            }
        }
    }
}
