using System;
using System.Linq;
using CLGameToolkit.Serialization;
using CLGameToolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLGameToolkit.GameSettings
{
    public abstract class BaseSettingsEntry<T> : MonoBehaviour
    {
        [SerializeField] private ClassField<T> Field;

        private SettingsManager<T> manager;

        private Slider slider;
        private Toggle toggle;
        private ToggleButtonGroup toggleGroup;
        private TMP_Dropdown dropdown;

        // TODO: Optimize to not remove listener
        // TODO: Make Manager mandatory in validate
        private void OnEnable()
        {
            if (manager == null)
            {
                manager = GetComponentInParent<SettingsManager<T>>();
                Field.OnValueChanged += manager.NotifyFieldUpdate;
            }

            Field.Target = manager.Current;

            if (slider != null || TryGetComponent(out slider))
            {
                slider.SetValueWithoutNotify(Field.GetValue<float>());
                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener(Field.SetValue);
                return;
            }

            if (toggle != null || TryGetComponent(out toggle))
            {
                toggle.SetIsOnWithoutNotify(Field.GetValue<bool>());
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(Field.SetValue);
                return;
            }


            Type fieldType = Field.GetFieldInfo().FieldType;
            if (fieldType.IsEnum)
            {
                int startValue = Field.GetValue<int>();

                int[] enumValues = Enum.GetValues(fieldType).Cast<int>().OrderBy(x => x).ToArray(); // TODO: A bit slow, and the ::GetValues function sorts them in (u_int32) 
                startValue = Array.IndexOf(enumValues, startValue); // When using any enum, we need to convert it to an index

                if (dropdown != null || TryGetComponent(out dropdown))
                {
                    dropdown.onValueChanged.RemoveAllListeners();
                    dropdown.onValueChanged.AddListener((index) => Field.SetValue(enumValues[index]));
                    dropdown.SetValueWithoutNotify(startValue);
                }
                else if (toggleGroup != null || TryGetComponent(out toggleGroup))
                {
                    toggleGroup.OnValueChanged.RemoveAllListeners();
                    toggleGroup.OnValueChanged.AddListener((index) => Field.SetValue(enumValues[index]));
                    toggleGroup.SetValueWithoutNotify(startValue);
                }
                return;
            }


            if (dropdown != null || TryGetComponent(out dropdown))
            {
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener(Field.SetValue);
                dropdown.SetValueWithoutNotify(Field.GetValue<int>());
                return;
            }

            if (toggleGroup != null || TryGetComponent(out toggleGroup))
            {
                toggleGroup.SetValueWithoutNotify(Field.GetValue<int>());
                toggleGroup.OnValueChanged.RemoveAllListeners();
                toggleGroup.OnValueChanged.AddListener(Field.SetValue);
                return;
            }
        }
    }
}
