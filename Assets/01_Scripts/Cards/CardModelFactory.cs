using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CardModelFactory
{
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
        card.FlavorText = LocaleDataManager.GetLocalizedCard(data.flavortext);
        card.effects = effects;

        // 리소스에서 Sprite 할당
        card.illustration = LoadSprite($"Cards/Illustration/{data.illustration}");
        card.chClass = LoadSprite($"Cards/Class/class_{data.@class}");
        card.cardType = LoadSprite($"Cards/Type/type_{data.type}");
        card.cardImage = data.cardframe;
        card.cardFrame = LoadSprite($"Cards/Frame/{data.cardframe}");

        // 키워드 처리
        card.keywords = ParseKeywords(data.keywords);

        // 스위치 스탠스
        card.switchStance = ParseStancType(data.switchType);

        // 성장 정보
        card.evolveCount = data.evolveCount;
        card.evolveTarget = data.evolveTarget;

        // 키워드 기반 플래그 설정
        card.InitializeFromKeywords();
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


    private static List<CardKeyword> ParseKeywords(List<string> keywords)
    {
        var result = new List<CardKeyword>();
        if (keywords == null) return result;

        foreach (var keyword in keywords)
        {
            var parsed = keyword.ToLower() switch
            {
                "exhaust" => CardKeyword.Exhaust,
                "retain" => CardKeyword.Retain,
                "temporary" => CardKeyword.Temporary,
                "copy" => CardKeyword.Copy,
                "innate" => CardKeyword.Innate,
                "kill" => CardKeyword.Kill,
                "grow" => CardKeyword.Grow,
                "critical" => CardKeyword.Critical,
                "switch" => CardKeyword.Switch,
                _ => CardKeyword.None
            };

            if (parsed != CardKeyword.None)
                result.Add(parsed);
        }
        return result;
    }

    private static StancType ParseStancType(string stancType)
    {
        if (string.IsNullOrEmpty(stancType)) return StancType.None;

        return stancType.ToLower() switch
        {
            "seek" => StancType.Seek,
            "insight" => StancType.Insight,
            "mercy" => StancType.Mercy,
            "discipline" => StancType.Discipline,
            "rush" => StancType.Rush,
            "defense" => StancType.Defense,
            "None" => StancType.None,
            _ => StancType.None
        };
    }
}