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
        card.effects = effects;
        card.type = (CardType)data.type;
        card.targetCount = data.targetNum;
        card.targetType = (TargetType)data.targetType;
        card.characterStance = data.characterStance;
        card.note = data.description;
        //card.illustration = data.illustration;
        card.cardImage = data.cardImage;


        return card;
    }
}