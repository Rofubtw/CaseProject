using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool. Reuses GameObjects instead of Instantiate/Destroy cycles.
/// T must be a Component on the pooled prefab.
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> available = new Queue<T>();

    public ObjectPool(T prefab, Transform parent, int initialSize = 0)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
            available.Enqueue(CreateInstance());
    }

    /// <summary>
    /// Returns a pooled object. Creates a new one if pool is empty.
    /// </summary>
    public T Get()
    {
        T item = available.Count > 0 ? available.Dequeue() : CreateInstance();
        item.gameObject.SetActive(true);
        return item;
    }

    /// <summary>
    /// Returns an object back to the pool.
    /// </summary>
    public void Release(T item)
    {
        item.gameObject.SetActive(false);
        available.Enqueue(item);
    }

    private T CreateInstance()
    {
        T instance = Object.Instantiate(prefab, parent);
        instance.gameObject.SetActive(false);
        return instance;
    }
}
