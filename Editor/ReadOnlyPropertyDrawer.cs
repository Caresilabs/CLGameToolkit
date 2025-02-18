using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool enabledState = GUI.enabled;
        GUI.enabled = false;
        EditorGUILayout.PropertyField(property, label, true);
        GUI.enabled = enabledState;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}
