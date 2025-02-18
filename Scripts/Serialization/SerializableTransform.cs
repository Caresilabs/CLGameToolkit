using System;
using UnityEngine;

namespace CLGameToolkit.Serialization
{
    [Serializable]
    public class SerializableTransform
    {
        public Vector3 Position => position;
        public Vector3 Rotation => rotation;
        public Vector3 Scale => scale;

        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private Vector3 scale;

        public SerializableTransform(Transform transform)
        {
            SetTransform(transform);
        }

        public void SetTransform(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation.eulerAngles;
            scale = transform.localScale;
        }

        public void GetTransform(Transform transform)
        {
            if (position != Vector3.zero)
                transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));

            if (scale != Vector3.zero)
                transform.localScale = scale;
        }
    }
}
