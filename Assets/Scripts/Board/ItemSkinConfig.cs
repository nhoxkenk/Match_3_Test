using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSkin", menuName = "Match3/Item Skin")]
public sealed class ItemSkinConfig : ScriptableObject
{
    public SkinType Skin;
    public NormalItemView Prefab;
    [Tooltip("Sprites in TYPE_ONE through TYPE_SEVEN order.")]
    public Sprite[] Sprites = new Sprite[7];
    public Vector3 Scale = Vector3.one;

    public Sprite GetSprite(NormalItem.eNormalType itemType)
    {
        int index = (int)itemType;
        if (Sprites == null || index < 0 || index >= Sprites.Length || Sprites[index] == null)
            throw new InvalidOperationException(name + " has no sprite for " + itemType);
        return Sprites[index];
    }
}
