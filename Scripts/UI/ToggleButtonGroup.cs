using System;
using CLGameToolkit.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CLGameToolkit.UI
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class ToggleButtonGroup : MonoBehaviour
    {
        [SerializeField] private string[] Options; // TODO localization
        [SerializeField] private RectTransform ToggleTemplate;

        [FormerlySerializedAs("onValueChanged")] public UnityEvent<int> OnValueChanged;

        [FormerlySerializedAs("onSelect")] public UnityEvent OnSelect;
        [FormerlySerializedAs("onDeselect")] public UnityEvent OnDeselect;

        private Toggle[] toggles;
        private bool isSelected;

        private int selectedIndex;

        protected void Awake()
        {
            toggles = new Toggle[Options.Length];

            for (int i = 0; i < Options.Length; i++)
            {
                int index = i;
                var toggleContainer = Instantiate(ToggleTemplate, transform);
                toggleContainer.gameObject.SetActive(true);
                Toggle toggleInstance = toggleContainer.GetComponentInChildren<Toggle>();
                TMP_Text label = toggleInstance.GetComponentInChildren<TMP_Text>();

                if (label != null)
                    label.text = Options[i];

                toggles[i] = toggleInstance;
                toggleInstance.isOn = selectedIndex == i; // TODO: Add initial value
                toggleInstance.onValueChanged.AddListener(isOn =>
                {
                    SetExclusive(index);

                    if (isOn)
                        OnValueChanged?.Invoke(index);
                });

                toggleInstance.gameObject.AddComponent<ToggleButtonEvents>().Init(OnButtonSelected, OnButtonDeselected);
            }
        }

        private void OnDisable()
        {
            if (isSelected)
            {
                isSelected = false;
                OnDeselect.Invoke();
            }
        }

        private void OnButtonDeselected()
        {
            if (!isSelected)
                return;

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != null && !currentSelected.transform.IsChildOf(transform))
            {
                isSelected = false;
                OnDeselect.Invoke();
            }
        }

        private void OnButtonSelected()
        {
            if (isSelected)
                return;

            isSelected = true;
            OnSelect.Invoke();
        }

        private void SetExclusive(int selectedIndex)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].SetIsOnWithoutNotify(i == selectedIndex);
            }
        }

        public void SetToggle(int index)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].isOn = i == index;
            }
        }

        public void SetValueWithoutNotify(int index)
        {
            if (toggles == null)
            {
                selectedIndex = index;
                return;
            }

            SetExclusive(index);
        }

        private class ToggleButtonEvents : MonoBehaviour, ISelectHandler, IDeselectHandler
        {
            private Action onSelect;
            private Action onDeselect;

            public bool HasSelection { get; private set; }

            public void Init(Action onSelect, Action onDeselect)
            {
                this.onSelect = onSelect;
                this.onDeselect = onDeselect;
            }
            public void OnDeselect(BaseEventData eventData)
            {
                HasSelection = false;
                Timer.Delay(.02f, NotifyParentOnDeselect);

                //Logger.Warn($"OnDeselect: {eventData}::\n\nn\naw ");
                //EventSystem.current.SetSelectedGameObject
            }

            private void NotifyParentOnDeselect()
            {
                onDeselect?.Invoke();
            }

            public void OnSelect(BaseEventData eventData)
            {
                HasSelection = true;
                onSelect?.Invoke();
            }
        }

    }
}
