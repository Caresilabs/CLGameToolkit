using UnityEditor;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] private Vector3 Axis = Vector3.up;
    [SerializeField] private float Speed = 90f;

    [Space(10)]
    [SerializeField] private Space Space = Space.Self;
    [SerializeField] private Vector3 Pivot;

    [SerializeField, ReadOnly] private Rigidbody Rigidbody;

    public float SpeedMultiplier { get; set; } = 1f;

    void Update()
    {
        float basicSpeed = SpeedMultiplier > 1 ? Mathf.Max(Speed, 1f) : Speed; // If we have multiplier, make sure speed is atleast 1
        float frameSpeed = basicSpeed * SpeedMultiplier * Time.deltaTime;

        Rotate(frameSpeed);
    }

    private void Rotate(float angle)
    {
        if (Rigidbody != null)
        {
            if (Space == Space.Self)
                Rigidbody.RotateAround(transform.TransformPoint(Pivot), transform.TransformDirection(Axis), angle);
            else
                Rigidbody.RotateAround(transform.TransformPoint(Pivot), Axis, angle);
        }
        else
        {
            if (Space == Space.Self)
                transform.RotateAround(transform.TransformPoint(Pivot), transform.TransformDirection(Axis), angle);
            else
                transform.RotateAround(transform.TransformPoint(Pivot), Axis, angle);
        }
    }

    public void RotateRandom()
    {
        Rotate(Random.Range(0, 360));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
#endif
}
