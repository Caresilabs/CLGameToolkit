using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A presistent ID component that can be referenced from prefabs to scene.
/// 
/// Use @GameObjectRef<Transform> to assing e.g reference to this Transform
/// </summary>
[DisallowMultipleComponent]
public class GameObjectId : MonoBehaviour, ISerializationCallbackReceiver
{
    private static readonly Dictionary<string, GameObjectId> cache = new();

    [field: SerializeField]
    [field: ReadOnly]
    public string Id { get; private set; }

    public static GameObjectId Find(string id)
    {
        return cache.GetValueOrDefault(id);
    }

    private void OnDestroy()
    {
        if (Find(Id) == this) // this could have been overriden by new scene etc.
            cache.Remove(Id);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    { }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (Id != string.Empty)
            cache[Id] = this;
    }

    public static GameObjectRef<T> Ref<T>(T objectId) where T : Component
    {
        if (objectId == null || !objectId.TryGetComponent<GameObjectId>(out var obj))
        {
            Logger.Error($"Could not create Ref for {objectId}");
            return null;
        }

        return obj.Ref<T>();
    }

    public GameObjectRef<T> Ref<T>() where T : Object
    {
        return new GameObjectRef<T>(Id);
    }

    public GameObjectRef Ref()
    {
        return new GameObjectRef(Id);
    }

#if UNITY_EDITOR
    private string lastId;

    private void OnValidate()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
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
            EditorUtility.SetDirty(this);
        }

        cache[Id] = this;
        lastId = Id;
    }

    public static object From(Component unityObj)
    {
        if (unityObj == null) return null;
        return unityObj.GetComponent<GameObjectId>();
    }

    [MenuItem("CONTEXT/GameObjectId/Regenerate Id")]
    private static void RegenerateIdMenu(MenuCommand command)
    {
        GameObjectId obj = command.context as GameObjectId;
        obj.Id = string.Empty;
        obj.lastId = string.Empty;
        obj.OnValidate();
    }

#endif

}
