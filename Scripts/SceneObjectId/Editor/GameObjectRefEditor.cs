using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameObjectRef<>))]
public class GameObjectRefEditor : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        SerializedProperty referenceProperty = property.FindPropertyRelative("gameObjectReference");

        var reference = fieldInfo.GetValue(property.serializedObject.targetObject);
        var generic = reference.GetType().GetGenericArguments()?[0];

        GameObjectId obj = GameObjectId.Find(referenceProperty.stringValue);
        GameObjectId newObject = EditorGUILayout.ObjectField($"{label.text} <{generic?.Name}>", obj, typeof(GameObjectId), allowSceneObjects: true) as GameObjectId;
  
        referenceProperty.stringValue = newObject != null ? newObject.Id : null;

        // Check the generic target and only attach if the component exists
        if (generic != null && newObject != null && newObject != obj && newObject.GetComponent(generic) == null)
        {
            referenceProperty.stringValue = null;
        }
    }
}
