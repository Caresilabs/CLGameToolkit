using UnityEngine;
using System;
using System.Reflection;

namespace CLGameToolkit.Serialization
{
    [Serializable]
    public class ClassField<T>
    {
        public T Target { get; set; }
        public string FieldName;

        public Action<ClassField<T>> OnValueChanged;

        public R GetValue<R>()
        {
            return GetValue<R>(Target);
        }

        public R GetValue<R>(T overrideTarget)
        {
            var field = GetFieldInfo();

            if (field == null)
                return default;

            var value = field.GetValue(overrideTarget);

            if (typeof(R) == typeof(int) && value is bool boolVal) // Request convertion to int
            {
                return (R)((object)(boolVal ? 1 : 0));
            }

            return (R)value;
        }

        public void SetValue<R>(R value)
        {
            var field = GetFieldInfo();

            if (field.FieldType == typeof(bool) && value is int intVal) // Request convertion to int
            {
                field.SetValue(Target, intVal >= 1);
            }
            else
            {
                field.SetValue(Target, value);
            }

            OnValueChanged?.Invoke(this);
        }

        public FieldInfo GetFieldInfo()
        {
            return typeof(T).GetField(FieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public bool IsName(string fieldCompare)
        {
            return FieldName == fieldCompare;
        }
    }


#if UNITY_EDITOR

    [UnityEditor.CustomPropertyDrawer(typeof(ClassField<>), true)]
    public class ClassFieldDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            Type referenceType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType;
            Type genericType = referenceType.GetGenericArguments()?[0];

            UnityEditor.SerializedProperty fieldNameProp = property.FindPropertyRelative("FieldName");

            FieldInfo[] fields = genericType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            string[] options = Array.ConvertAll(fields, f => f.Name);

            int selectedIndex = Array.FindIndex(options, name => name == fieldNameProp.stringValue);

            if (string.IsNullOrEmpty(fieldNameProp.stringValue))
            {
                fieldNameProp.stringValue = options.Length > 0 ? options[0] : "";
                return;
            }

            if (selectedIndex < 0)
            {
                Logger.Error($"Field<{genericType.Name}> fieldname (\"{fieldNameProp.stringValue}\") is now invalid", property.serializedObject.targetObject);
                UnityEditor.EditorGUILayout.LabelField("Error! Update field name!");
                selectedIndex = 0;
            }

            UnityEditor.EditorGUI.BeginProperty(position, label, property);

            int newSelectedIndex = UnityEditor.EditorGUILayout.Popup($"Field<{genericType.Name}>", selectedIndex, options);

            if (newSelectedIndex != selectedIndex)
                fieldNameProp.stringValue = options.Length > 0 ? options[newSelectedIndex] : "";

            UnityEditor.EditorGUI.EndProperty();
        }
    }

#endif
}
