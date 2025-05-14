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

        
        var cards1 = new List<CardModel>();
        var cardDatas1 = CardJsonLoader.Load("ExternalFiles/CardsJson");

        foreach(var data in cardDatas1)
        {
            var effects = NewCardEffectBuilder.Build(data);
            var card = CardModelFactory.Create(data, effects);
            cards1.Add(card);
        }
        


        
        return cards1;
    }
}
