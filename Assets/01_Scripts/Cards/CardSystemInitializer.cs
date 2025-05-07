using System.Collections.Generic;
using UnityEngine;

public class CardSystemInitializer : MonoSingleton<CardSystemInitializer>
{
    [Header("CSV 파일 (Resources 폴더 하위 경로)")]
    public string csvRelativePath = "ExternalFiles/Cards";

    [Header("로드된 카드들")]
    public List<CardModel> loadedCards;

    public Dictionary<int, CardModel> cardLookup = new();


    public void LoadCardDatabase()
    {
        loadedCards = CardDatabaseLoader.LoadAll(csvRelativePath);
        cardLookup.Clear();

        foreach (var card in loadedCards)
        {
            cardLookup[card.index] = card;
        }

        Debug.Log($"총 {loadedCards.Count}장의 카드 로드 완료!");
    }
}