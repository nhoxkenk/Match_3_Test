using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class NormalItemView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_spriteRenderer;

    public void ApplySkin(ItemSkinConfig config, NormalItem.eNormalType itemType)
    {
        if (!m_spriteRenderer) m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spriteRenderer.sprite = config.GetSprite(itemType);
        transform.localScale = config.Scale;
    }
}
