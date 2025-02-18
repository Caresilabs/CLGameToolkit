using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameObjectRef))]
[CustomPropertyDrawer(typeof(GameObjectRef<>))]
public class GameObjectRefEditor : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        SerializedProperty referenceProperty = property.FindPropertyRelative("Id");

        System.Type referenceType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType;
        bool hasNoGeneric = referenceType == typeof(GameObjectRef);
        System.Type generic = hasNoGeneric ? typeof(GameObjectId) : referenceType.GetGenericArguments()?[0];

        GameObjectId obj = GameObjectId.Find(referenceProperty.stringValue);
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        Object newObject = EditorGUI.ObjectField(rect, $"{label.text} <{(hasNoGeneric ? string.Empty : generic?.Name)}>", obj, generic, allowSceneObjects: true);

        if (property.serializedObject.isEditingMultipleObjects)
            return;

        if (newObject == null)
        {
            referenceProperty.stringValue = null;
            return;
        }

        GameObjectId gameObjectId = (newObject as Component).GetComponent<GameObjectId>();
        if (gameObjectId == null)
        {
            referenceProperty.stringValue = null;
            property.serializedObject.ApplyModifiedProperties();
            Logger.Warn($"{newObject.name} is missing GameObjectId Component!");
            return;
        }

        if (gameObjectId.Id == string.Empty)
        {
            referenceProperty.stringValue = null;
            property.serializedObject.ApplyModifiedProperties();
            Logger.Warn($"Cannot select {newObject.name} as it is a prefab");
            return;
        }

        if (referenceProperty.stringValue != gameObjectId.Id)
        {
            referenceProperty.stringValue = gameObjectId.Id;
            referenceProperty.serializedObject.ApplyModifiedProperties();
        }

    }
}
