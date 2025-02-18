using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class ParticleSystemPool : GameObjectPool<ParticleSystem>
{
    public ParticleSystemPool() : base() { }
    public ParticleSystemPool(ParticleSystem prefab, int initialSize = 0) : base(prefab, initialSize) { }

    public ParticleSystem SpawnParticle(Transform parent)
    {
        var ps = Get(parent);
        ReleaseDelayed(ps, ps.main.duration + float.Epsilon);
        return ps;
    }

    public ParticleSystem SpawnParticle(Vector3 position)
    {
        var ps = Get();
        ps.transform.position = position;
        ReleaseDelayed(ps, ps.main.duration + float.Epsilon);
        return ps;
    }
}

[System.Serializable]
public class GameObjectPool<T> where T : Component
{
    [SerializeField] private T Prefab;

    private readonly Queue<T> objectQueue;
    private Transform parentTransform;

    public GameObjectPool() { this.objectQueue = new Queue<T>(5); }
    public GameObjectPool(int initialSize = 0) : this(null, initialSize) { }
    public GameObjectPool(T prefab, int initialSize = 0)
    {
        this.Prefab = prefab;
        this.objectQueue = new Queue<T>(initialSize);
        CreateContainerGO();

        for (int i = 0; i < initialSize; i++)
            CreateNew();
    }

    public T Get()
    {
        if (objectQueue.Count == 0)
            CreateNew();

        T obj = objectQueue.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public T Get(Transform parent)
    {
        var obj = Get();
        obj.transform.SetParent(parent);
        obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return obj;
    }

    public void Release(T obj)
    {
        if (obj == null) // Object got destroyed, don't put it back into the pool
            return;

#if UNITY_EDITOR
        // Test stuff. Remove me
        if (obj is Behaviour beh && !beh.enabled)
            Logger.Error($"Pool: Object return and component is disabled! {obj} under {obj.transform.parent}", obj);
#endif

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(parentTransform);
        objectQueue.Enqueue(obj);
    }

    public void ReleaseDelayed(T obj, float delay, bool ignoreTimeScale = false)
    {
        DOVirtual.DelayedCall(delay, () =>
            Release(obj)
        , ignoreTimeScale);
    }

    public GameObjectPool<T> DontDestroy()
    {
        Object.DontDestroyOnLoad(parentTransform);
        return this;
    }

    private void CreateNew()
    {
        CreateContainerGO();
        EnsureValidState();
        const string goName = "PoolItem";

        T newObj;

        if (Prefab == null)
        {
            GameObject go = new(goName);
            go.transform.parent = parentTransform;
            newObj = go.AddComponent<T>();
        }
        else
        {
            newObj = Object.Instantiate(Prefab, parentTransform);
            // newObj.name = goName;
        }

        newObj.gameObject.SetActive(false);
        objectQueue.Enqueue(newObj);
    }

    private void EnsureValidState()
    {
        bool isInvalid = false;

        foreach (T poolItem in objectQueue)
        {
            if (poolItem == null)
            {
                Logger.Warn($"Warning in ${parentTransform.name}: PoolItem was deleted without informing the pool!");
                isInvalid = true;
                break;
            }
        }

        if (isInvalid)
        {
            T[] filtered = objectQueue.Where(x => x != null).ToArray();
            objectQueue.Clear();
            foreach (var poolItem in filtered)
                objectQueue.Enqueue(poolItem);
        }
    }

    private void CreateContainerGO()
    {
        if (parentTransform == null)
            parentTransform = new GameObject($"@ObjectPool_{typeof(T).Name}_{(Prefab != null ? Prefab.name : string.Empty)}").transform;
    }
}
