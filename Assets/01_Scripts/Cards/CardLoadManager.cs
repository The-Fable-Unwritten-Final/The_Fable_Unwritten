using System.Collections.Generic;
using UnityEngine;

public class CardSystemInitializer : MonoBehaviour
{
    [Header("CSV 파일 (Resources 폴더 하위 경로)")]
    public string csvRelativePath = "ExternalFiles/Cards.csv";

    [Header("로드된 카드들")]
    public List<CardModel> loadedCards;

    void Awake()
    {
        LoadCardDatabase();
    }

    public void LoadCardDatabase()
    {
        string fullPath = $"{Application.dataPath}/Resources/{csvRelativePath}";
        loadedCards = CardDatabaseLoader.LoadAll(fullPath);

        Debug.Log($"총 {loadedCards.Count}장의 카드 로드 완료!");
    }
}