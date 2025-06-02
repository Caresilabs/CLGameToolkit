using UnityEngine;

namespace CLGameToolkit.Physics
{
    public class CollisionAudio : MonoBehaviour
    {
        [SerializeField] private SoundContainer Sound;
        [SerializeField] private float MinVelocity = -1;
        [SerializeField] private float PitchVariance = .1f;
        [SerializeField] private LayerMask Layer = -1;

        // TODO: Velocity for max volume. I.e scale volume with velocity multiplier

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

            if (MinVelocity >= 0 && MinVelocity > collision.relativeVelocity.magnitude)
                return;

            Play();
        }

        public void Play()
        {
            AudioManager.PlaySFX(Sound.Clip, transform, Sound.Volume, Sound.Pitch + Random.Range(-PitchVariance, PitchVariance));
        }

        // We can't parent when we destroy it.
        public void PlayAtDestroy()
        {
            AudioManager.PlaySFX(Sound.Clip, transform.position, Sound.Volume, Sound.Pitch + Random.Range(-PitchVariance, PitchVariance));
        }
    }
}
