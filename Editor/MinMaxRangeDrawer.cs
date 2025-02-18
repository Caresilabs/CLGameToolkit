using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxRangeInt))]
[CustomPropertyDrawer(typeof(MinMaxRange))]
public class MinMaxRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float indentLength = 32f;
        float labelWidth = EditorGUIUtility.labelWidth;
        float floatFieldWidth = EditorGUIUtility.fieldWidth * 0.75f;
        float sliderWidth = position.width - labelWidth - 2.0f * floatFieldWidth;
        float sliderPadding = 24.0f;

        Rect labelRect = new Rect(
            position.x,
            position.y,
            labelWidth,
            position.height);

        Rect sliderRect = new Rect(
            position.x + labelWidth + floatFieldWidth + sliderPadding - indentLength,
            position.y,
            sliderWidth - 2.0f * sliderPadding + indentLength,
            position.height);

        Rect minFloatFieldRect = new Rect(
            position.x + labelWidth - indentLength,
            position.y,
            floatFieldWidth + indentLength,
            position.height);

        Rect maxFloatFieldRect = new Rect(
            position.x + labelWidth + floatFieldWidth + sliderWidth - indentLength,
            position.y,
            floatFieldWidth + indentLength,
            position.height);

        System.Type referenceType = fieldInfo.FieldType;
        bool isInt = referenceType == typeof(MinMaxRangeInt);

        SerializedProperty minValueProp = property.FindPropertyRelative("MinValue");
        SerializedProperty maxValueProp = property.FindPropertyRelative("MaxValue");
        MinMaxRangeAttribute minMaxSliderAttribute = fieldInfo.GetCustomAttribute<MinMaxRangeAttribute>();

        float minRange = minMaxSliderAttribute?.MinValue ?? int.MinValue;
        float maxRange = minMaxSliderAttribute?.MaxValue ?? int.MaxValue;

        float newMinValue = isInt ? minValueProp.intValue : minValueProp.floatValue;
        float newMaxValue = isInt ? maxValueProp.intValue : maxValueProp.floatValue;

        EditorGUI.LabelField(labelRect, label.text);
        EditorGUI.MinMaxSlider(sliderRect, ref newMinValue, ref newMaxValue, minRange, maxRange);

        if (isInt)
        {
            newMinValue = EditorGUI.IntField(minFloatFieldRect, (int)newMinValue);
            newMinValue = Mathf.Clamp(newMinValue, minRange, Mathf.Min(maxRange, newMaxValue));

            newMaxValue = EditorGUI.IntField(maxFloatFieldRect, (int)newMaxValue);
            newMaxValue = Mathf.Clamp(newMaxValue, Mathf.Max(minRange, newMinValue), maxRange);

            minValueProp.intValue = (int)newMinValue;
            maxValueProp.intValue = (int)newMaxValue;
        }
        else
        {
            newMinValue = EditorGUI.FloatField(minFloatFieldRect, newMinValue);
            newMinValue = Mathf.Clamp(newMinValue, minRange, Mathf.Min(maxRange, newMaxValue));

            newMaxValue = EditorGUI.FloatField(maxFloatFieldRect, newMaxValue);
            newMaxValue = Mathf.Clamp(newMaxValue, Mathf.Max(minRange, newMinValue), maxRange);

            minValueProp.floatValue = newMinValue;
            maxValueProp.floatValue = newMaxValue;
        }


    }
}
