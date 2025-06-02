using UnityEngine;

namespace CLGameToolkit.Physics
{
    public class PhysicsObject : MonoBehaviour
    {

        [SerializeField, ReadOnly] private Rigidbody Body;


        public void AddAngularVelocityY(float ySpeed)
        {
            Body.angularVelocity += new Vector3(0, ySpeed, 0);
        }

        public void AddUpwardForce(float ySpeed)
        {
            Body.AddForce(Vector3.up * ySpeed, ForceMode.Force);
        }


#if UNITY_EDITOR

        private void OnValidate()
        {
            if (Body == null)
                Body = GetComponent<Rigidbody>();
        }

#endif
    }
}
