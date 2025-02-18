using UnityEngine;

[RequireComponent(typeof(AudioReverbZone))]
public class AudioReverbTriggerZone : MonoBehaviour
{
    [SerializeField, ReadOnly] private AudioReverbZone ReverbZone;
    [SerializeField, TagField] private string CameraTag;

    private int collidedNumber;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(CameraTag))
            return;

        collidedNumber++;
        ReverbZone.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(CameraTag))
            return;

        if (--collidedNumber == 0)
            ReverbZone.enabled = false;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        ReverbZone = GetComponent<AudioReverbZone>();

        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();

            gameObject.layer = 2; // Ignore Raycast layer
        }

        collider.isTrigger = true;

        ReverbZone.minDistance = collider.bounds.size.magnitude / 2f;
        ReverbZone.maxDistance = ReverbZone.minDistance * 1.1f;

        if (string.IsNullOrEmpty(CameraTag))
            CameraTag = "Player";

    }
#endif
}
