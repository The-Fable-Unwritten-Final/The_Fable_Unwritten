using System.Collections.Generic;
using UnityEngine;

public static class CardDatabaseLoader
{
    public static List<CardModel> LoadAll(string csvPath)
    {   
        var cards = new List<CardModel>();
        var cardDatas = CardJsonLoader.Load("ExternalFiles/CardDataFinal");

        Debug.Log($"[CardDatabaseLoader] dataCount={cardDatas?.Count ?? -1}");

        if (cardDatas == null)
            return cards;

        foreach (var data in cardDatas)
        {
            var effects = NewCardEffectBuilder.Build(data);
            var card = CardModelFactory.Create(data, effects);
            cards.Add(card);
        }
        Debug.Log($"[CardDatabaseLoader] 최종 카드 수={cards.Count}");

        return cards;
    }

    public static List<CardModel> LoadAllNew(string csvPath)
    {
        var cards = new List<CardModel>();
        var cardDatas = CardJsonLoader2.Load("ExternalFiles/cards");

        foreach(var data in cardDatas)
        {
            var effects = CardEffectBuilder2.Build(data);
            var card = CardModelFactory2.Create(data, effects);
            cards.Add(card);
        }
        return cards;
    }
}
