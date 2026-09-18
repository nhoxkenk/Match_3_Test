using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ItemFactory
{
    private readonly ItemViewPool m_pool;
    private readonly Dictionary<SkinType, ItemSkinConfig> m_skins = new Dictionary<SkinType, ItemSkinConfig>();

    public ItemFactory(ItemViewPool pool, ItemSkinConfig[] skins)
    {
        m_pool = pool;
        if (skins == null) throw new ArgumentNullException(nameof(skins));
        foreach (ItemSkinConfig config in skins)
        {
            if (!config || !config.Prefab)
                throw new ArgumentException("Each item skin must have a config and a NormalItemView prefab.", nameof(skins));
            m_skins.Add(config.Skin, config);
        }
    }

    public Transform CreateNormal(NormalItem.eNormalType itemType, SkinType skin, Transform parent)
    {
        ItemSkinConfig config;
        if (!m_skins.TryGetValue(skin, out config))
            throw new ArgumentException("No item skin configured for " + skin, nameof(skin));
        config.GetSprite(itemType); // Validate before borrowing a pooled view.
        Transform view = m_pool.Get(config.Prefab.gameObject, parent);
        view.GetComponent<NormalItemView>().ApplySkin(config, itemType);
        return view;
    }

    public Transform Create(string prefabName, Transform parent)
    {
        return m_pool.Get(prefabName, parent);
    }

    public void Release(Transform view)
    {
        m_pool.Release(view);
    }
}
