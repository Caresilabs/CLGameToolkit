using System;
using CLGameToolkit.Serialization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    private ReorderableList reorderableList;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (reorderableList == null)
        {
            SerializedProperty keysProperty = property.FindPropertyRelative("keys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("values");

            reorderableList = new ReorderableList(property.serializedObject, keysProperty, true, true, true, true);
            reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, label);
            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.y += 2;

                SerializedProperty keyProperty = keysProperty.GetArrayElementAtIndex(index);
                SerializedProperty valueProperty = valuesProperty.GetArrayElementAtIndex(index);
                float valueHeight = EditorGUI.GetPropertyHeight(valueProperty);
                bool isValueAnObject = valueHeight > EditorGUIUtility.singleLineHeight || !valueProperty.isExpanded;

                float halfWidth = rect.width * 0.5f;
                Rect keyRect = new Rect(rect.x, rect.y, halfWidth, rect.height);
                Rect valueRect = isValueAnObject ? new Rect(rect.x, rect.y + rect.height, rect.width, valueHeight) : new Rect(rect.x + halfWidth, rect.y, halfWidth, rect.height);

                EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, true);
            };
            reorderableList.elementHeightCallback = (int index) =>
            {
                var valueProperty = valuesProperty.GetArrayElementAtIndex(index);
                float valueHeight = EditorGUI.GetPropertyHeight(valueProperty);

                if (valueHeight <= EditorGUIUtility.singleLineHeight && valueProperty.isExpanded)
                    return EditorGUIUtility.singleLineHeight;

                return EditorGUI.GetPropertyHeight(keysProperty.GetArrayElementAtIndex(index)) + valueHeight;
            };
            reorderableList.onAddCallback = list =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;


                keysProperty.arraySize++;
                valuesProperty.arraySize++;

                SerializedProperty keyProperty = keysProperty.GetArrayElementAtIndex(index);
                SerializedProperty valueProperty = valuesProperty.GetArrayElementAtIndex(index);



                CreateNewDefaultFor(keyProperty);
                CreateNewDefaultFor(valueProperty);
            };
            reorderableList.onRemoveCallback = list =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this key-value pair?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
        }

        reorderableList.DoList(position);

        EditorGUI.EndProperty();
    }

    private void CreateNewDefaultFor(SerializedProperty prop)
    {
        string typeString = prop.type;
        if (typeString == nameof(Enum))
        {
            prop.intValue = -1;
        }
        else
        {
            var type = prop.boxedValue.GetType();
            prop.boxedValue = GetDefault(type);
        }
    }

    public static object GetDefault(Type type)
    {
        if (type == null) return null;
        return Activator.CreateInstance(type);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (reorderableList != null)
        {
            height += reorderableList.GetHeight();
        }
        return height;
    }
}