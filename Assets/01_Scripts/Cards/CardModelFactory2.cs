using System.Collections.Generic;
using UnityEngine;

public static class CardModelFactory2
{
    public static CardModel Create(CardJsonData2 data, List<CardEffectBase> effects)
    {
        var card = ScriptableObject.CreateInstance<CardModel>();

        // ===== 기본 정보 =====
        card.index = data.index;
        card.cardName = LocaleDataManager.GetLocalizedCard(data.name);
        card.cardText = LocaleDataManager.GetLocalizedCard(data.text);
        card.FlavorText = LocaleDataManager.GetLocalizedCard(data.flavortext);

        // ===== 비용 / 메타 =====
        card.manaCost = data.cost;
        card.characterClass = (CharacterClass)data.@class;
        card.type = (CardType)data.type;
        card.targetCount = data.target_num;
        card.targetType = (TargetType)data.target_type;
        card.note = data.note;

        // ===== 확장 메타 =====
        card.switchType = data.switchType;
        card.evolveCount = data.evolveCount;
        card.evolveTarget = data.evolveTarget;
        card.keywords = data.keywords != null ? new List<string>(data.keywords) : new List<string>();

        // ===== 비주얼 =====
        card.illustration = LoadSpriteSafe($"Cards/Illustration/{data.illustration}", data.illustration);
        card.chClass = LoadSpriteSafe($"Cards/Class/class_{data.@class}", $"class_{data.@class}");
        card.cardType = LoadSpriteSafe($"Cards/Type/type_{data.type}", $"type_{data.type}");
        card.cardImage = data.cardframe;
        card.cardFrame = LoadSpriteSafe($"Cards/Frame/{data.cardframe}", data.cardframe);

        // ===== 효과 =====
        card.effects = effects != null ? new List<CardEffectBase>(effects) : new List<CardEffectBase>();

        // ===== 연출 =====
        card.skillEffectName = data.skilleffect;

        return card;
    }

    private static Sprite LoadSpriteSafe(string fullPath, string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return null;

        var sprite = Resources.Load<Sprite>(fullPath);
        if (sprite == null)
            Debug.LogWarning($"[CardModelFactory] Sprite 리소스를 찾을 수 없습니다: {fullPath}");

        return sprite;
    }
}