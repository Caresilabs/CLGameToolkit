using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ResourceRef<>))]
public class ResourceRefEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty pathRef = property.FindPropertyRelative("path");
        SerializedProperty guidRef = property.FindPropertyRelative("assetGUID");
        var sceneObject = GetResourceObject(guidRef.stringValue);

        var reference = fieldInfo.GetValue(property.serializedObject.targetObject);
        var generic = reference.GetType().GetGenericArguments()?[0];

        Object prefab = EditorGUI.ObjectField(position, label, sceneObject, generic, false);

        if (prefab == null)
        {
            pathRef.stringValue = "";
            guidRef.stringValue = "";
        }
        else if (prefab.name != pathRef.stringValue)
        {
            var newPath = AssetDatabase.GetAssetPath(prefab);
            if (newPath == null)
            {
                Debug.LogWarning("The prefab " + prefab.name + " can't be found.");
            }
            else
            {
                if (!newPath.Contains("Resources"))
                {
                    Debug.LogWarning("The asset is not in a Resources folder!");
                    return;
                }

                pathRef.stringValue = newPath;
                guidRef.stringValue = AssetDatabase.AssetPathToGUID(newPath);
            }
        }

    }
    protected GameObject GetResourceObject(string assetGUID)
    {
        if (string.IsNullOrEmpty(assetGUID))
            return null;

        var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

        return AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
    }
}