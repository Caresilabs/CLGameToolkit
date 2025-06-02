using System;
using System.Reflection;
using CLGameToolkit.Serialization;
using UnityEngine;

namespace CLGameToolkit.GameSettings
{
    public abstract class SettingsManager<T> : MonoBehaviour
    {
        public static Action<T> OnSettingsSaved;
        public T Current { get; private set; }

        [SerializeField] private CanvasGroup SettingsGroup;
        [SerializeField] protected bool RequireConfirm;

        public void Open()
        {
            Current = CloneSettings();
            SettingsGroup.gameObject.SetActive(true);
        }

        public void Close()
        {
            SettingsGroup.gameObject.SetActive(false);

            if (!RequireConfirm)
                ApplySettings();
        }

        public void ApplySettings()
        {
            if (SaveSettings())
                OnSettingsSaved?.Invoke(Current);
        }

        protected bool IsChanged(T other)
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var value1 = field.GetValue(Current);
                var value2 = field.GetValue(other);
                if (!value1.Equals(value2))
                    return true;
            }
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value1 = prop.GetValue(Current);
                var value2 = prop.GetValue(other);
                if (!value1.Equals(value2))
                    return true;
            }
            return false;
        }

        public void NotifyFieldUpdate(ClassField<T> field)
        {
            if (RequireConfirm)
                return;

            OnFieldUpdated(field);
        }

        protected abstract bool SaveSettings();
        protected abstract T CloneSettings();
        protected abstract void OnFieldUpdated(ClassField<T> field);
    }
}
