
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A presistent ID component that can be referenced from prefabs to scene.
/// 
/// Use @GameObjectRef<Transform> to assing e.g reference to this Transform
/// </summary>
public class GameObjectId : MonoBehaviour
{
    private static readonly Dictionary<string, GameObjectId> cache = new Dictionary<string, GameObjectId>();

    [field: SerializeField]
    [field: ReadOnly]
    public string Id { get; private set; }


    public static GameObjectId Find(string id)
    {
        return cache.GetValueOrDefault(id);
    }

    private void Awake()
    {
        cache[Id] = this;
    }

    private void OnDestroy()
    {
        cache.Remove(Id);
    }

    public static GameObjectRef<T> Ref<T>(T objectId) where T : Component
    {
        var obj = objectId.GetComponent<GameObjectId>();

        return obj.Ref<T>();
    }

    public GameObjectRef<T> Ref<T>() where T : Object
    {
        var objectRef = new GameObjectRef<T>();

        var prop = objectRef.GetType().GetField("gameObjectReference", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        prop.SetValue(objectRef, Id);

        return objectRef;
    }



#if UNITY_EDITOR
    private string lastId;

    private void OnValidate()
    {
        if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
        {
            if (Id != string.Empty)
                Id = "";

            return;
        }


        var currentKey = cache.GetValueOrDefault(Id ?? "");

        if (currentKey == this) return;

        // Hack to "undo" an Apply Prefab action
        if (lastId != null && Id == string.Empty)
            Id = lastId;

        if (Id == null || Id == string.Empty || currentKey != null) // No Id or Id is taken by another object. I.e copy.
        {
            Id = System.Guid.NewGuid().ToString();
        }

        cache[Id] = this;
        lastId = Id;
    }
#endif

}
