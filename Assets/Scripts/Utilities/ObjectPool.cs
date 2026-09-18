using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One prefab per pool. Release borrowed instances instead of destroying them.
// The owner controls the pool's lifetime; release instances before destroying their parent.
public sealed class ObjectPool<T> where T : Component
{
    private readonly T m_prefab;
    private readonly Transform m_storage;
    private readonly Stack<T> m_available = new Stack<T>();
    private readonly HashSet<T> m_borrowed = new HashSet<T>();

    public int CountInactive => m_available.Count;

    public ObjectPool(T prefab, Transform owner)
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));
        if (owner == null) throw new ArgumentNullException(nameof(owner));

        m_prefab = prefab;
        m_storage = new GameObject(prefab.name + " Pool").transform;
        m_storage.SetParent(owner, false);
        m_storage.gameObject.SetActive(false);
    }

    public IEnumerator Prewarm(int count, int objectsPerFrame = 4)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (objectsPerFrame < 1) throw new ArgumentOutOfRangeException(nameof(objectsPerFrame));

        while (m_available.Count < count)
        {
            for (int i = 0; i < objectsPerFrame && m_available.Count < count; i++)
                m_available.Push(CreateInstance());

            yield return null;
        }
    }

    public T Get(Transform parent, Vector3 position)
    {
        T instance = m_available.Count > 0 ? m_available.Pop() : CreateInstance();
        m_borrowed.Add(instance);
        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.localRotation = m_prefab.transform.localRotation;
        instance.transform.localScale = m_prefab.transform.localScale;
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Release(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (!m_borrowed.Remove(instance))
            throw new InvalidOperationException("The instance was not borrowed from this pool or was already released.");

        instance.gameObject.SetActive(false);
        instance.transform.SetParent(m_storage, false);
        m_available.Push(instance);
    }

    private T CreateInstance()
    {
        T instance = UnityEngine.Object.Instantiate(m_prefab, m_storage);
        instance.gameObject.SetActive(false);
        return instance;
    }
}
