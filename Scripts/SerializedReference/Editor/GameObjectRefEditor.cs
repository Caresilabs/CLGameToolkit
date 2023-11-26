using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameObjectRef))]
[CustomPropertyDrawer(typeof(GameObjectRef<>))]
public class GameObjectRefEditor : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        SerializedProperty referenceProperty = property.FindPropertyRelative("gameObjectReference");

        System.Type referenceType = fieldInfo.FieldType;
        var generic = referenceType == typeof(GameObjectRef) ? typeof(Transform) : referenceType.GetGenericArguments()?[0];

        GameObjectId obj = GameObjectId.Find(referenceProperty.stringValue);
        GameObjectId newObject = EditorGUI.ObjectField(rect, $"{label.text} <{generic?.Name}>", obj, typeof(GameObjectId), allowSceneObjects: true) as GameObjectId;
  
        referenceProperty.stringValue = newObject != null ? newObject.Id : null;

        // Check the generic target and only attach if the component exists
        if (generic != null && newObject != null && newObject != obj && newObject.GetComponent(generic) == null)
        {
            referenceProperty.stringValue = null;
        }
    }
}
