using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CardModelFactory
{
    public static CardModel Create(CardJsonData data, List<CardEffectBase> effects)
    {
        var card = ScriptableObject.CreateInstance<CardModel>();
        card.index = data.index;
        card.cardName = data.name;
        card.cardText = data.text;
        card.manaCost = data.cost;
        card.characterClass = (CharacterClass)data.@class;

        card.type = (CardType)data.type;
        card.targetCount = data.target_num;
        card.targetType = (TargetType)data.target_type;
        card.note = data.note;
        card.FlavorText = data.flavortext;

        // 리소스에서 Sprite 할당
        card.illustration = LoadSprite($"Cards/Illustration/{data.illustration}");
        card.chClass = LoadSprite($"Cards/Class/class_{data.@class}");
        card.cardType = LoadSprite($"Cards/Type/type_{data.type}");
        card.cardImage = data.cardframe;
        card.cardFrame = LoadSprite($"Cards/Frame/{data.cardframe}");
        card.effects = effects;

        card.skillEffectName = data.skilleffect;
        return card;
    }

    private static Sprite LoadSprite(string path)
    {
        var sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"[CardModelFactory] Sprite 리소스를 찾을 수 없습니다: {path}");
        return sprite;
    }
}