using UnityEngine;
using UnityEngine.Events;

namespace CLGameToolkit.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class TriggerZone : MonoBehaviour
    {
        [SerializeField] protected bool DestroyAfterTrigger = true;
        [SerializeField] protected LayerMask Layer = -1;

        [TagField]
        [SerializeField] protected string Tag;

        [Space(10)]
        [Header("Events")]
        [SerializeField] protected UnityEvent<Collider> OnTriggerEvent;
        [SerializeField] protected UnityEvent<Collider> OnExitEvent;

        public void ForceTrigger()
        {
            ExecuteTrigger(GetComponent<Collider>());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Layer != (Layer | (1 << other.gameObject.layer)))
                return;

            if (Tag != string.Empty && !other.CompareTag(Tag) || !enabled)
                return;

            ExecuteTrigger(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (Layer != (Layer | (1 << other.gameObject.layer)))
                return;

            if (Tag != string.Empty && !other.CompareTag(Tag) || !enabled)
                return;

            OnExitEvent.Invoke(other);
        }

        protected virtual void ExecuteTrigger(Collider other)
        {
            OnTriggerEvent.Invoke(other);

            if (DestroyAfterTrigger)
            {
                Destroy(gameObject, Time.deltaTime);
                OnTriggerEvent.RemoveAllListeners();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(Tag))
                Tag = "Player";
        }
#endif
    }
}

