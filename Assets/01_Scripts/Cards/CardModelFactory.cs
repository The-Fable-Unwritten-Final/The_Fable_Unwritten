using System.Collections.Generic;
using UnityEngine;

public static class CardModelFactory
{
    public static CardModel Create(CardData data, List<CardEffectBase> effects)
    {
        var card = ScriptableObject.CreateInstance<CardModel>();
        card.index = data.index;
        card.cardName = data.name;
        card.cardText = data.text;
        card.manaCost = data.cost;
        card.characterClass = (CharacterClass)data.classIndex;
        card.effects = effects;
        return card;
    }
}