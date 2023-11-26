using UnityEngine;

[System.Serializable]
public class GameObjectRef<T> where T : Object
{
    [SerializeField] private string gameObjectReference;
    
    public Transform transform { get { return GameObjectId.Find(gameObjectReference)?.transform; } }
    public bool Exists { get { return gameObjectReference != null; } }

    private T cachedReference;
    private bool hasCache;

    public bool IsSame(GameObjectId other)
    {
        return other.Id == gameObjectReference;
    }

    public static implicit operator T(GameObjectRef<T> reference)
    {
        if (reference.hasCache == false && reference.gameObjectReference != null)
        {
            var component = reference.transform.GetComponent<T>();
            reference.cachedReference = component;
            reference.hasCache = true;
            return component;
        }

        return reference.cachedReference;
    }

    public static implicit operator Transform(GameObjectRef<T> reference)
    {
        return reference.transform;
    }

}

[System.Serializable]
public class GameObjectRef : GameObjectRef<Transform>
{

}
