using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace CLGameToolkit.UI
{
    public class TabButton : Selectable, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField] private TabGroup TabGroup;

        [SerializeField] private UnityEvent OnTabSelected;
        [SerializeField] private UnityEvent OnTabDeselected;

        private bool IsCurrentTab;

        protected override void Awake()
        {
            base.Awake();

            if (TabGroup != null)
                TabGroup.Subscribe(this);
        }

        protected override void OnEnable()
        {
            if (IsCurrentTab)
                base.OnSelect(null); // Reselct tab upon enable

            base.OnEnable();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            TabGroup.OnTabSelected(this);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            TabGroup.OnTabSelected(this);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            TabGroup.OnTabEnter(this);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!IsCurrentTab)
                base.OnPointerExit(eventData);

            TabGroup.OnTabExit(this);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            SetAsCurrentTab();

            TabGroup.OnTabSelected(this);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            targetGraphic.color = IsCurrentTab ? Color.white * .8f : Color.white;

            if (!IsCurrentTab)
                base.OnDeselect(eventData);
        }

        public override void Select()
        {
            SetAsCurrentTab();
        }

        public void SelectTab()
        {

            base.OnSelect(null); // base or this?
            SetAsCurrentTab();
            targetGraphic.color = Color.white * .8f;

            OnTabSelected?.Invoke();
        }

        private void SetAsCurrentTab()
        {
            IsCurrentTab = true;
            targetGraphic.color = Color.white;
        }

        public void Deselect()
        {
            IsCurrentTab = false;

            base.OnPointerExit(null);
            OnDeselect(null);
            OnTabDeselected?.Invoke();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (TabGroup == null)
            {
                TabGroup = GetComponentInParent<TabGroup>();
            }
        }
#endif

    }
}
