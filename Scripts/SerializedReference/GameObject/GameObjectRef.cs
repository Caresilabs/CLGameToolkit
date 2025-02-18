using UnityEngine;

[System.Serializable]
public class GameObjectRef<T> : Identifiable where T : Object
{
    public Transform transform => GameObjectId.Find(Id)?.transform;
    public bool Exists => Id != string.Empty;

    private T cachedReference;
    private bool hasCache;

    public GameObjectRef()
    { }

    public GameObjectRef(string id)
    {
        Id = id;
    }

    public GameObjectRef<TNew> Convert<TNew>() where TNew : Object
    {
        return new GameObjectRef<TNew>(Id);
    }

    public static implicit operator T(GameObjectRef<T> reference)
    {
        return GetComponent(reference);
    }

    public static implicit operator GameObjectRef(GameObjectRef<T> reference)
    {
        return new GameObjectRef() { Id = reference.Id };
    }

    public static implicit operator Transform(GameObjectRef<T> reference)
    {
        return reference.transform;
    }

    //public static bool operator ==(GameObjectRef<T> obj1, T obj2)
    //{
    //    return obj2 != null && (T)obj1 == obj2;
    //}

    //public static bool operator !=(GameObjectRef<T> obj1, T obj2)
    //{
    //    return (obj1 == obj2) == false;
    //}

    public static bool operator ==(GameObjectRef<T> obj1, GameObjectId gameObjectId)
    {
        return gameObjectId != null && obj1.Id == gameObjectId.Id;
    }

    public static bool operator !=(GameObjectRef<T> obj1, GameObjectId gameObjectId)
    {
        return (obj1 == gameObjectId) == false;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    private static T GetComponent(GameObjectRef<T> reference)
    {
        if (object.ReferenceEquals(reference, null)) return null;

        if (reference.hasCache && reference.cachedReference == null)
        {
            reference.hasCache = false; // Missing pointer
            reference.cachedReference = null;
        }

        if (!reference.hasCache && reference.Id != string.Empty)
        {
            var transform = reference.transform;

            if (transform == null)
                return null;

            var component = transform.GetComponent<T>();
            reference.cachedReference = component;
            reference.hasCache = true;
            return component;
        }

        return reference.cachedReference;
    }
}

[System.Serializable]
public class GameObjectRef : GameObjectRef<Transform>
{
    public GameObjectRef()
    { }

    public GameObjectRef(string id)
    {
        Id = id;
    }
}

public abstract class Identifiable
{
    [SerializeField] protected string Id;

    public override bool Equals(object obj)
    {
        if (obj is Identifiable identifiable)
            return Id == identifiable.Id;

        return false;
    }

    public override int GetHashCode()
    {
        return Id != null ? Id.GetHashCode() : 0;
    }

    public static bool operator ==(Identifiable obj1, Identifiable obj2)
    {
        if (((object)obj1 == null) || ((object)obj2 == null))
            return false;

        return obj1.Id == obj2.Id;
    }

    public static bool operator !=(Identifiable obj1, Identifiable obj2)
    {
        return (obj1 == obj2) == false;
    }
}
