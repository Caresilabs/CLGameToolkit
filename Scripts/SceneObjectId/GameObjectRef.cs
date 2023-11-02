using UnityEngine;

[System.Serializable]
public class GameObjectRef<T> where T : Object
{
    [SerializeField]
    private string gameObjectReference;

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
            var component = GameObjectId.Find(reference.gameObjectReference).GetComponent<T>();
            reference.cachedReference = component;
            reference.hasCache = true;
            return component;
        }

        return reference.cachedReference;
    }

}
