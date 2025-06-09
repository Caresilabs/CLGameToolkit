using UnityEngine;

namespace CLGameToolkit.Physics
{
    public class CollisionAudio : MonoBehaviour
    {
        [SerializeField] private SoundContainer[] Sounds;
        [SerializeField, HideInInspector] private SoundContainer Sound; // @Depracated

        [Space]
        [SerializeField, Min(0)] private float MinVelocity = 0;
        [SerializeField, Min(-1)] private float MaxVelocity = -1; // Velocity at which volume is at max
        [SerializeField] private float PitchVariance = .1f;
        [SerializeField] private LayerMask Layer = -1;

        private CallThrottler throttler;

        private void Awake()
        {
            throttler = new CallThrottler(.1f);
            throttler.Reset();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!throttler.CanCall())
                return;

            if (Layer != (Layer | (1 << collision.gameObject.layer)))
                return;

            float speed = collision.relativeVelocity.magnitude;
            if (MinVelocity >= 0 && MinVelocity > speed)
                return;

            float volumeMultiplier = MaxVelocity > 0 ? Mathf.InverseLerp(MinVelocity, MaxVelocity, speed) : 1f;
            Play(volumeMultiplier);
        }

        private void OnParticleCollision(GameObject other)
        {
            if (Layer != (Layer | (1 << other.layer)))
                return;

            // TODO: Check min velocity
            Play();
        }

        public void Play()
        {
            Play(1f);
        }

        public void Play(float volumeMultiplier)
        {
            if (volumeMultiplier == 0 || Sounds.Length == 0) return;
            var Sound = Sounds.Random();
            AudioManager.PlaySFX(Sound.Clip, transform, Sound.Volume * volumeMultiplier, Sound.Pitch + Random.Range(-PitchVariance, PitchVariance));
        }

        // We can't parent when we destroy it.
        public void PlayAtDestroy()
        {
            if (Sounds.Length == 0) return;
            var sound = Sounds.Random();
            AudioManager.PlaySFX(sound.Clip, transform.position, sound.Volume, sound.Pitch + Random.Range(-PitchVariance, PitchVariance));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Sound != null)
            {
                // Migrate old Sound to Sounds array
                if (Sounds == null || Sounds.Length == 0)
                {
                    Sounds = new[] { Sound };
                    UnityEditor.EditorUtility.SetDirty(this); // Marks object as dirty to save changes
                }

                Sound = null; // Clear old field
            }

            if (MaxVelocity > 0 && MaxVelocity < MinVelocity)
            {
                MaxVelocity = MinVelocity;
            }
        }
#endif
    }
}
