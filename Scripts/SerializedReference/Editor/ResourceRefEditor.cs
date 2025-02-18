using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ResourceRef))]
[CustomPropertyDrawer(typeof(ResourceRef<>))]
public class ResourceRefEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty pathRef = property.FindPropertyRelative("path");
        SerializedProperty guidRef = property.FindPropertyRelative("assetGUID");
        Object loadedResourceObject = GetResourceObject(guidRef.stringValue);

        System.Type referenceType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType;
        System.Type generic = referenceType == typeof(ResourceRef) ? typeof(GameObject) : referenceType.GetGenericArguments()?[0];

        string name = label.text;
        Object selectedAsset = EditorGUI.ObjectField(position, name, loadedResourceObject, generic, false);

        if (property.serializedObject.isEditingMultipleObjects)
            return;

        if (selectedAsset == null)
        {
            pathRef.stringValue = "";
            guidRef.stringValue = "";
            property.serializedObject.ApplyModifiedProperties();
            return;
        }

        string newPath = AssetDatabase.GetAssetPath(selectedAsset);
        if (newPath == null)
        {
            Debug.LogWarning("The prefab " + selectedAsset.name + " can't be found.");
        }
        else
        {
            if (!newPath.Contains("Resources"))
            {
                Debug.LogWarning("The asset is not in a Resources folder!");
                return;
            }

            GameObject assetAsGameObject = selectedAsset as GameObject;
            if (assetAsGameObject != null && assetAsGameObject.GetComponent<ResourceId>() == null)
            {
                Debug.LogWarning($"The asset {selectedAsset} is missing a recommended ResourceId component");
            }

            pathRef.stringValue = ResourceRef.ConvertToResourcesPath(newPath);
            guidRef.stringValue = AssetDatabase.AssetPathToGUID(newPath);
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    protected Object GetResourceObject(string assetGUID)
    {
        if (string.IsNullOrEmpty(assetGUID))
            return null;

        string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
        return AssetDatabase.LoadAssetAtPath<Object>(assetPath);
    }
}