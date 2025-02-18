using UnityEngine;
using UnityEngine.Serialization;

public class LookAt : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("LockX")] private bool LookX = true;
    [SerializeField, FormerlySerializedAs("LockY")] private bool LookY = true;
    [SerializeField, FormerlySerializedAs("LockZ")] private bool LookZ = true;

    [Header("Config")]
    [SerializeField] private float Speed = 40f;
    [SerializeField] private float MaxDistance = 0;

    // TODO: Choose target
    // TODO: Choose transform space

    // TODO: Delay

    private Transform target;

    void Start()
    {
        target = Camera.main.transform;
    }

    void Update()
    {
        Vector3 camTargetDir = target.position - transform.position;
        Vector3 rotation = Quaternion.LookRotation(camTargetDir).eulerAngles;
        Quaternion currentRotation = transform.rotation;
        Vector3 currentEulerRotation = currentRotation.eulerAngles;

        if (MaxDistance > 0 && camTargetDir.magnitude > MaxDistance)
            return;

        Quaternion targetRotation = Quaternion.Euler(LookX ? rotation.x : currentEulerRotation.x, LookY ? rotation.y : currentEulerRotation.y, LookZ ? rotation.z : currentEulerRotation.z);

        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Speed * Time.deltaTime);
    }
}
