using UnityEngine;
using UnityEngine.UI;

public class StanceIconDisplay : MonoBehaviour
{
    [SerializeField] private Image icon;

    [Header("Owner")]
    [SerializeField] private CharacterClass ownerClass;

    [Header("Icon Size")]
    [SerializeField] private float spriteScale = 0.05f;

    [Header("Sophia")]
    [SerializeField] private Sprite seekIcon;
    [SerializeField] private Sprite insightIcon;

    [Header("Kayla")]
    [SerializeField] private Sprite mercyIcon;
    [SerializeField] private Sprite disciplineIcon;

    [Header("Leon")]
    [SerializeField] private Sprite rushIcon;
    [SerializeField] private Sprite defenseIcon;

    private void Awake()
    {
        HideIcon();
    }

    public void UpdateIcon(StancType stance)
    {
        if (icon == null)
            return;

        // 이 UI가 담당하는 캐릭터의 스탠스인지 확인
        if (!IsOwnerStance(stance))
        {
            HideIcon();
            return;
        }

        Sprite sprite = GetStanceSprite(stance);

        if (sprite == null)
        {
            HideIcon();
            return;
        }

        icon.sprite = sprite;

        // Sprite 실제 크기에 맞춰 RectTransform 조절
        ApplySpriteSize(sprite);

        icon.enabled = true;
    }

    private Sprite GetStanceSprite(StancType stance)
    {
        return stance switch
        {
            StancType.Seek => seekIcon,
            StancType.Insight => insightIcon,

            StancType.Mercy => mercyIcon,
            StancType.Discipline => disciplineIcon,

            StancType.Rush => rushIcon,
            StancType.Defense => defenseIcon,

            _ => null
        };
    }

    private bool IsOwnerStance(StancType stance)
    {
        return ownerClass switch
        {
            CharacterClass.Sophia =>
                stance == StancType.Seek ||
                stance == StancType.Insight,

            CharacterClass.Kayla =>
                stance == StancType.Mercy ||
                stance == StancType.Discipline,

            CharacterClass.Leon =>
                stance == StancType.Rush ||
                stance == StancType.Defense,

            _ => false
        };
    }

    private void ApplySpriteSize(Sprite sprite)
    {
        if (sprite == null || icon == null)
            return;

        RectTransform rectTransform = icon.rectTransform;

        rectTransform.sizeDelta = new Vector2(
            sprite.rect.width * spriteScale,
            sprite.rect.height * spriteScale
        );
    }

    public void HideIcon()
    {
        if (icon == null)
            return;

        icon.sprite = null;
        icon.enabled = false;
    }
}