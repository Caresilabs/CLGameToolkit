using System.Collections.Generic;
using CLGameToolkit.Serialization;
using UnityEngine;

namespace CLGameToolkit.Gameplay
{
    public class FootstepSound : MonoBehaviour
    {
        [SerializeField] private float StrideLength = 0.8f; // Distance between footsteps
        [SerializeField] private SerializableDictionary<SurfaceType, SurfaceAudio> Footsteps;
        [SerializeField] private bool UseRaycastAudio;

        [Header("Raycast")]
        [SerializeField] private LayerMask RaycastMask;
        [SerializeField] private Vector3 RaycastOffset = new(0, .1f, 0);

        private Vector3 lastFootstepPosition;
        private float footstepsDistance;

        private void Start()
        {
            lastFootstepPosition = transform.position;
        }

        private void Update()
        {
            float distanceMoved = Vector3.Distance(lastFootstepPosition, transform.position); // TODO fall will trigger
            footstepsDistance += distanceMoved;

            if (footstepsDistance >= StrideLength) // will maybe not play exactly as it resets the distance?
            {
                SurfaceType surfaceType = GetSurfaceType();
                if (surfaceType == SurfaceType.Invalid)
                    return;

                PlayFootstep(surfaceType);
                footstepsDistance %= StrideLength; // Modulus?
            }

            lastFootstepPosition = transform.position;
        }

        private void PlayFootstep(SurfaceType surfaceType)
        {
            var surface = Footsteps.GetValueOrDefault(surfaceType, Footsteps.GetValueOrDefault(SurfaceType.Default));

            if (surface != null)
            {
                // if (UseRaycastAudio)
                //     AudioRaycasterNavMesh.PlaySFX(surface.Sound.RandomNullable(), transform.position, surface.Volume, Random.Range(.9f, 1.1f));
                // else
                AudioManager.PlaySFX(surface.Sound, transform.position + RaycastOffset, surface.Volume, Random.Range(.9f, 1.1f));
            }
        }

        private Collider lastCollider;
        private SurfaceType lastSurface;

        private SurfaceType GetSurfaceType()
        {
            if (!UnityEngine.Physics.Raycast(transform.position + RaycastOffset, Vector3.down, out RaycastHit hit, 1.5f, RaycastMask, QueryTriggerInteraction.Ignore))
            {
                return SurfaceType.Invalid;
            }

            Collider collider = hit.collider;
            if (collider == lastCollider)
                return lastSurface;

            Renderer renderer = collider.GetComponent<Renderer>();
            lastCollider = collider;

            SurfaceType surface = SurfaceType.Default;

            if (renderer == null || renderer.sharedMaterial == null)
            {
                if (collider.gameObject.layer == 4) // Unity Water Layer
                {
                    lastSurface = SurfaceType.Water;
                    return lastSurface;
                }

                return surface;
            }

            string name = renderer.sharedMaterial.name;

            foreach (var footstep in Footsteps)
            {
                string matchString = string.IsNullOrEmpty(footstep.Value.OverrideMaterialName) ? footstep.Key.ToString() : footstep.Value.OverrideMaterialName;
                if (name.Contains(matchString))
                {
                    surface = footstep.Key;
                    break;
                }
            }

            lastSurface = surface;
            return surface;
        }

        public enum SurfaceType
        {
            Default,
            Wood,
            Stone,
            Tiles,
            Carpet,
            Asphalt,
            Grass,
            Metal,
            Water,


            Custom01 = 50,
            Custom02,
            Custom03,
            Custom04,
            Custom05,

            Invalid = 99,
        }

        [System.Serializable]
        public class SurfaceAudio
        {
            public string OverrideMaterialName;

            public float Volume = 1f;

            [Tooltip("Noise modifier for this surface. 1.0 = normal, < 1.0 = quieter (carpet), > 1.0 = louder (metal)")]
            [Range(0.1f, 2.0f)]
            public float NoiseModifier = 1f;

            public AudioClip[] Sound;
        }

        /// <summary>
        /// Gets the noise modifier for the current surface type
        /// </summary>
        public float GetCurrentSurfaceNoiseModifier()
        {
            SurfaceType currentSurface = GetSurfaceType();
            if (currentSurface == SurfaceType.Invalid)
                return 1f;

            var surface = Footsteps.GetValueOrDefault(currentSurface);
            return surface?.NoiseModifier ?? 1f;
        }
    }
}