using System;
using UnityEngine;

namespace CLGameToolkit.Serialization
{
    /// <summary>
    /// Serialize Nullable value by wrapping it in a 1 sized array.
    /// Has overhead for smaller objects because of the wrapping array.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class SerializableNullable<TValue>
    {
        public TValue Value { get => HasValue ? value[0] : default; set => SetValue(value); }
        public bool HasValue => value != null;

        [SerializeField] private TValue[] value;

        public SerializableNullable(TValue value = default)
        {
            SetValue(value);
        }

        public override string ToString()
        {
            if (!HasValue) return "null";
            return Value.ToString();
        }

        public static implicit operator TValue(SerializableNullable<TValue> nullable)
        {
            return nullable is null || !nullable.HasValue ? default : nullable.Value;
        }

        protected void SetValue(TValue newValue)
        {
            if (newValue == null)
            {
                value = null;
                return;
            }

            if (value == null || value.Length == 0)
                value = new TValue[1];

            value[0] = newValue;
        }
    }
}
