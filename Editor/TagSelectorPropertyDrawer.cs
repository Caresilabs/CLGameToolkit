using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TagFieldAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        var tagAttribute = attribute as TagFieldAttribute;

        var rect = new Rect(position);
        rect.width = tagAttribute.AllowNull ? rect.width * 0.8f : rect.width;

        var value = property.stringValue;
        if (!tagAttribute.AllowNull && value == null)
        {
            value = UnityEditorInternal.InternalEditorUtility.tags[0];
        }


        property.stringValue = EditorGUI.TagField(rect, label, value);
        if (tagAttribute.AllowNull)
        {
            if (GUI.Button(new Rect(position.x + rect.width, position.y, position.width - rect.width, rect.height), "Clear"))
            {
                property.stringValue = string.Empty;
            }
        }


        EditorGUI.EndProperty();
    }
}
