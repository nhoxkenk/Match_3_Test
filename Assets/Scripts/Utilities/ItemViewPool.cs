using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public sealed class ItemViewPool
{
    private readonly Transform m_owner;
    private readonly Dictionary<GameObject, ObjectPool<Transform>> m_pools = new Dictionary<GameObject, ObjectPool<Transform>>();
    private readonly Dictionary<string, GameObject> m_prefabs = new Dictionary<string, GameObject>();
    private readonly Dictionary<Transform, ObjectPool<Transform>> m_active = new Dictionary<Transform, ObjectPool<Transform>>();

    public ItemViewPool(Transform owner)
    {
        m_owner = owner;
    }

    public Transform Get(string prefabName, Transform parent)
    {
        GameObject prefab;
        if (!m_prefabs.TryGetValue(prefabName, out prefab))
        {
            prefab = Resources.Load<GameObject>(prefabName);
            if (!prefab) return null;
            m_prefabs.Add(prefabName, prefab);
        }
        return Get(prefab, parent);
    }

    public Transform Get(GameObject prefab, Transform parent)
    {
        ObjectPool<Transform> pool;
        if (!m_pools.TryGetValue(prefab, out pool))
        {
            pool = new ObjectPool<Transform>(prefab.transform, m_owner);
            m_pools.Add(prefab, pool);
        }

        Transform view = pool.Get(parent, Vector3.zero);
        SpriteRenderer sprite = view.GetComponent<SpriteRenderer>();
        if (sprite) sprite.sortingOrder = 0;
        m_active.Add(view, pool);
        return view;
    }

    public void Release(Transform view)
    {
        ObjectPool<Transform> pool;
        if (!view || !m_active.TryGetValue(view, out pool)) return;
        view.DOKill();
        m_active.Remove(view);
        pool.Release(view);
    }

    public void ReleaseAll()
    {
        // Include exploding views that are no longer attached to a cell.
        foreach (Transform view in new List<Transform>(m_active.Keys))
            Release(view);
    }
}
