using System.Collections.Generic;
using UnityEngine;

public static class CardModelFactory
{
    public static CardModel Create(CardData data, List<CardEffectBase> effects)
    {
        var card = ScriptableObject.CreateInstance<CardModel>();

        card.index = data.index;
        card.cardName = data.cardName;
        card.cardText = data.text;
        card.manaCost = data.cost;
        card.characterClass = (CharacterClass)data.classIndex;

        card.type = (CardType)data.type; 
        card.targetCount = data.targetNum;
        card.targetType = (TargetType)data.targetType;
        card.characterStance = data.characterStance;
        card.note = data.description;
        card.FlavorText = data.flavorText;

        // 리소스에서 Sprite 할당
        card.illustration = LoadSprite($"Cards/Illustration/{data.illustration}");
        card.chClass = LoadSprite($"Cards/Class/class_{data.classIndex}");
        card.cardType = LoadSprite($"Cards/Type/type_{data.type}");
        card.cardImage = data.cardFrame;
        card.cardFrame = LoadSprite($"Cards/Frame/{data.cardFrame}");

        card.effects = effects;

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