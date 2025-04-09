using System.Collections.Generic;

public static class CardDatabaseLoader
{
    public static List<CardModel> LoadAll(string csvPath)
    {
        var cards = new List<CardModel>();
        var cardDatas = CardCSVParser.Parse(csvPath);

        foreach (var data in cardDatas)
        {
            var effects = CardEffectBuilder.Build(data);
            var card = CardModelFactory.Create(data, effects);
            cards.Add(card);
        }
        return cards;
    }
}
