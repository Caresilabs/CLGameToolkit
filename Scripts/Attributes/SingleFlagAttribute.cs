using System;
using UnityEngine;

namespace CLGameToolkit.Attributes
{
    public class SingleFlagAttribute : PropertyAttribute { }
   
#if UNITY_EDITOR

    [UnityEditor.CustomPropertyDrawer(typeof(SingleFlagAttribute))]
    public class SingleEnumFlagSelectAttributeEditor : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            Type enumType = fieldInfo.FieldType;

            if (!enumType.IsEnum && enumType != typeof(int))
            {
                UnityEditor.EditorGUI.LabelField(position, label.text, "Use SingleEnumFlagSelect with int or enum.");
                return;
            }

            var displayTexts = new System.Collections.Generic.List<GUIContent>();
            var enumValues = new System.Collections.Generic.List<int>();

            foreach (var displayText in Enum.GetValues(enumType))
            {
                displayTexts.Add(new GUIContent(displayText.ToString()));
                enumValues.Add((int)displayText);
            }

            property.intValue = UnityEditor.EditorGUI.IntPopup(position, label, property.intValue, displayTexts.ToArray(), enumValues.ToArray());
        }
    }

#endif

}
