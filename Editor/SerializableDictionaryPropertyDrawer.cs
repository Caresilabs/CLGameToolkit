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

                float halfWidth = rect.width * 0.5f;
                Rect keyRect = new Rect(rect.x, rect.y, halfWidth, rect.height);
                Rect valueRect = new Rect(rect.x + halfWidth, rect.y, halfWidth, rect.height);

                EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

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

                // TODO Doesnt work
                valueProperty.boxedValue = GetDefault(Type.GetType(valuesProperty.arrayElementType));
                valueProperty.boxedValue = GetDefault(Type.GetType(valuesProperty.arrayElementType));
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

    public static object GetDefault(Type type)
    {
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