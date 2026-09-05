using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CardModelFactory
{
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public static CardModel Create(CardJsonData data, List<CardEffectBase> effects)
    {
        var card = ScriptableObject.CreateInstance<CardModel>();
        card.index = data.index;
        card.cardName = LocaleDataManager.GetLocalizedCard(data.name);
        card.cardText = LocaleDataManager.GetLocalizedCard(data.text);
        card.manaCost = data.cost;
        card.characterClass = (CharacterClass)data.@class;

        card.type = (CardType)data.type;
        card.targetCount = data.target_num;
        card.targetType = (TargetType)data.target_type;
        card.note = data.note;
        card.keywords = data.keywords != null
            ? new List<string>(data.keywords)
            : new List<string>(); card.switchType = data.switchType;
        card.evolveCount = data.evolveCount;
        card.evolveTarget = data.evolveTarget;

        card.FlavorText = LocaleDataManager.GetLocalizedCard(data.flavortext);

        // 리소스에서 Sprite 할당
        card.illustration = LoadSprite($"Cards/Illustration/illust_{data.index}"); card.chClass = LoadSprite($"Cards/Class/class_{data.@class}");
        card.cardType = LoadSprite($"Cards/Type", $"type_{data.type}");
        card.cardImage = data.cardframe;
        card.cardFrame = LoadSprite($"Cards/Frame/{data.cardframe}");
        card.effects = effects != null ? new List<CardEffectBase>(effects) : new List<CardEffectBase>();
        card.skillEffectName = data.skilleffect;
        card.soundIndexes = data.soundIndexes != null ? new List<int>(data.soundIndexes) : new List<int>();
        card.soundDelays = data.soundDelays != null ? new List<float>(data.soundDelays) : new List<float>();
        return card;
    }

    private static Sprite LoadSprite(string path)
    {
        var sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"[CardModelFactory] Sprite 리소스를 찾을 수 없습니다: {path}");
        return sprite;
    }

    // Multiple 스프라이트 시트에서 특정 스프라이트를 로드하는 메서드
    private static Sprite LoadSprite(string folderPath, string spriteName)
    {
        string cacheKey = $"{folderPath}/{spriteName}";
        
        if (spriteCache.ContainsKey(cacheKey))
            return spriteCache[cacheKey];

        // Multiple 스프라이트 시트에서 모든 스프라이트 로드
        var allSprites = Resources.LoadAll<Sprite>(folderPath);
        var sprite = System.Array.Find(allSprites, s => s.name == spriteName);
        
        if (sprite == null)
            Debug.LogWarning($"[CardModelFactory] Sprite를 찾을 수 없습니다: {cacheKey}");
        else
            spriteCache[cacheKey] = sprite;
            
        return sprite;
    }
}