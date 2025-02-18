using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneRef))]
public class SceneRefEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty pathRef = property.FindPropertyRelative("levelPath");
        SerializedProperty guidRef = property.FindPropertyRelative("assetGUID");
        var sceneObject = GetSceneObject(guidRef.stringValue);

        SceneAsset scene = (SceneAsset)EditorGUI.ObjectField(position, label, sceneObject, typeof(SceneAsset), false);

        if (property.serializedObject.isEditingMultipleObjects)
            return;

        if (scene == null)
        {
            pathRef.stringValue = "";
            guidRef.stringValue = "";
        }
        else if (scene.name != pathRef.stringValue)
        {
            string newPath = AssetDatabase.GetAssetPath(scene);
            if (newPath == null)
            {
                Debug.LogWarning("The scene " + scene.name + " cannot be used. To use this scene add it to the build settings for the project");
            }
            else
            {
                pathRef.stringValue = newPath;
                guidRef.stringValue = AssetDatabase.AssetPathToGUID(newPath);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

    }
    protected SceneAsset GetSceneObject(string sceneGUID)
    {
        if (string.IsNullOrEmpty(sceneGUID))
            return null;

        var assetPath = AssetDatabase.GUIDToAssetPath(sceneGUID);

        return AssetDatabase.LoadAssetAtPath(assetPath, typeof(SceneAsset)) as SceneAsset;

        //foreach (var editorScene in EditorBuildSettings.scenes)
        //{
        //    if (editorScene.path.IndexOf(sceneGUID) != -1)
        //    {
        //        return AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(SceneAsset)) as SceneAsset;
        //    }
        //}
        //Debug.LogWarning("Scene [" + sceneGUID + "] cannot be used. Add this scene to the 'Scenes in the Build' in build settings.");
        //return null;
    }
}