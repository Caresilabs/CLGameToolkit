using UnityEngine;

/// <summary>
/// Track the Resource Path during runtime
/// </summary>
[DisallowMultipleComponent]
public class ResourceId : MonoBehaviour
{
    [SerializeField, ReadOnly] private string Path;

    public ResourceRef<T> Ref<T>() where T : Object
    {
        return new ResourceRef<T>(Path);
    }

    public ResourceRef Ref()
    {
        return new ResourceRef(Path);
    }

    public static ResourceRef FromOrNull(ResourceId id)
    {
        if (id == null) return null;
        return id.Ref();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        if (!UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this))
        {
            return;
        }
    
        string newPath = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject)?.assetPath ??
                 UnityEditor.AssetDatabase.GetAssetPath(gameObject);

        newPath = ResourceRef.ConvertToResourcesPath(newPath);
        if (newPath != string.Empty)
        {
            this.Path = newPath;
            UnityEditor.EditorUtility.SetDirty(this); // Could not find a way to save prefab without always setting it as dirty...
        }
    }
#endif
}
