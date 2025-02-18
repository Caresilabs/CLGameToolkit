using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ResourceRef<T> : BaseResourceRef where T : Object
{
    public bool Exists { get { return path != string.Empty; } }

    public ResourceRef() { }
    public ResourceRef(string path)
    {
        this.path = path;
    }

    public T Load()
    {
        return Load(path);
    }

    //public bool IsSame(Object obj)
    //{
    //    if (obj is ResourceRef<T> other)
    //        return other.path == path;

    //    return false;
    //}

    public ResourceRef<TNew> Convert<TNew>() where TNew : Object
    {
        return new ResourceRef<TNew>(path);
    }

    //public static implicit operator T(ResourceRef<T> reference)
    //{
    //    return Load(reference.path);
    //}

    public static implicit operator ResourceRef(ResourceRef<T> reference)
    {
        return new ResourceRef() { path = reference.path };
    }

    public void LoadAsync(System.Action<T> callback)
    {
        if (string.IsNullOrEmpty(path))
            throw new System.Exception("ResourceRef: Can not load null path!");

        var loadedAsset = Resources.LoadAsync(path);
        loadedAsset.completed += (ctx) =>
        {

            if (typeof(T) == typeof(GameObject))
            {
                callback(loadedAsset.asset as T);
            }
            else
            {
                callback((loadedAsset.asset as GameObject).GetComponent<T>());
            }
        };

    }

    public async Awaitable<T> LoadAsync()
    {
        if (string.IsNullOrEmpty(path))
            throw new System.Exception("ResourceRef: Can not load null path!");

        var asset = Resources.LoadAsync<T>(path);
        await asset;

        if (typeof(T) == typeof(GameObject))
            return (asset.asset as T);
        else
            return (asset.asset as GameObject).GetComponent<T>();
    }

    private static T Load(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new System.Exception("ResourceRef: Can not load null path!");

        Object loadedAsset = Resources.Load(path);

        if (typeof(T) == typeof(GameObject))
        {
            return loadedAsset as T;
        }
        else if (loadedAsset is ScriptableObject)
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
    public ResourceRef() { }
    public ResourceRef(string path) : base(path) { }
}

public abstract class BaseResourceRef
#if UNITY_EDITOR
     : ISerializationCallbackReceiver
#endif
{
    [SerializeField] protected string path;

    public override bool Equals(object obj)
    {
        if (obj is BaseResourceRef identifiable)
            return path == identifiable.path;

        return false;
    }

    public override int GetHashCode()
    {
        return path != null ? path.GetHashCode() : 0;
    }

    public static bool operator ==(BaseResourceRef obj1, BaseResourceRef obj2)
    {
        if (((object)obj1 == null) || ((object)obj2 == null))
            return false;


        return obj1.path == obj2.path;
    }

    public static bool operator !=(BaseResourceRef obj1, BaseResourceRef obj2)
    {
        return (obj1 == obj2) == false;
    }

#if UNITY_EDITOR
    [SerializeField] private string assetGUID;

    public void OnBeforeSerialize()
    {
        if (string.IsNullOrEmpty(assetGUID) || Application.isPlaying)
            return;

        var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
        path = ResourceRef.ConvertToResourcesPath(assetPath);
    }

    public void OnAfterDeserialize()
    { }
#endif
}
