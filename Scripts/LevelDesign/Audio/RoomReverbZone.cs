using UnityEngine;

namespace CLGameToolkit.Gameplay
{
    [RequireComponent(typeof(AudioReverbZone))]
    public class RoomReverbZone : MonoBehaviour
    {
        [SerializeField, ReadOnly] private AudioReverbZone ReverbZone;

        [SerializeField, Range(0f, 1f), Tooltip("Compact to expansive.")]
        private float Space = 0.5f;

        [SerializeField, Range(0f, 1f), Tooltip("Bright to soft.")]
        private float Softness = 0.5f;


#if UNITY_EDITOR
        private void OnValidate()
        {
            ReverbZone ??= GetComponent<AudioReverbZone>();
            ReverbZone.reverbPreset = AudioReverbPreset.User;

            ReverbZone.room = Mathf.RoundToInt(Mathf.Lerp(-7000f, -1000f, Space));
            ReverbZone.roomHF = Mathf.RoundToInt(Mathf.Lerp(-1000f, -9000f, Softness));
            ReverbZone.decayTime = Mathf.Lerp(0.15f, 8f, Space);
            ReverbZone.decayHFRatio = Mathf.Lerp(1.8f, 0.2f, Softness);
            ReverbZone.reflections = Mathf.RoundToInt(Mathf.Lerp(-9000f, -2500f, Space));
            ReverbZone.reverb = Mathf.RoundToInt(Mathf.Lerp(-8000f, -1000f, Space));
            ReverbZone.diffusion = Mathf.Lerp(30f, 100f, Space);
            ReverbZone.density = 100f;
        }
#endif
    }
}