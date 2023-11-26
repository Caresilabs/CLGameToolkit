using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceRef<T> where T : Object
{
    [SerializeField] private string path;

#if UNITY_EDITOR
    [SerializeField] private string assetGUID;
#endif

    public T Load()
    {
        return Load(path);
    }

    public IEnumerable<T> LoadAsync()
    {
        var asset = Resources.LoadAsync(path);

        while (!asset.isDone)
            yield return null;

        yield return asset.asset as T;
    }


    public static implicit operator T(ResourceRef<T> reference)
    {
        return Load(reference.path);
    }

    private static T Load(string path)
    {
        var loadedAsset = Resources.Load(ConvertToResourcesPath(path));
        if (typeof(T) == typeof(GameObject))
        {
            return loadedAsset as T;
        }

        return (loadedAsset as GameObject).GetComponent<T>();
    }


    private const string RESOURCES_FOLDER_NAME = "/Resources/";
    public static string ConvertToResourcesPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return string.Empty;

        int folderIndex = assetPath.IndexOf(RESOURCES_FOLDER_NAME);

        folderIndex += RESOURCES_FOLDER_NAME.Length;

        int length = assetPath.Length - folderIndex;
        length -= assetPath.Length - assetPath.LastIndexOf('.');

        return assetPath.Substring(folderIndex, length);
    }
}

[System.Serializable]
public class ResourceRef : ResourceRef<GameObject>
{

}