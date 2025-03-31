using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automatically assigns a GUID string to the field once and makes it read-only in the inspector.
/// </summary>
public class AutoGuidAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AutoGuidAttribute))]
public class AutoGuidDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [AutoGuid] with string fields only");
            return;
        }

        if (string.IsNullOrEmpty(property.stringValue))
        {
            property.stringValue = System.Guid.NewGuid().ToString();
            property.serializedObject.ApplyModifiedProperties(); // Apply immediately
        }

        // Add right-click context menu
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        var evt = Event.current;

        if (evt.type == EventType.ContextClick && position.Contains(evt.mousePosition))
        {
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Regenerate GUID"), false, () =>
            {
                property.stringValue = System.Guid.NewGuid().ToString();
                property.serializedObject.ApplyModifiedProperties();
            });
            menu.ShowAsContext();
            evt.Use();
        }


        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(position, property, label);
        EditorGUI.EndDisabledGroup();
    }
}
#endif
