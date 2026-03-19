using System.Collections.Generic;
using UnityEngine;

public static class CardJsonLoader2
{
    public static List<CardJsonData2> Load(string resourcePath)
    {
        var cardList = new List<CardJsonData2>();
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

        if (jsonFile == null)
        {
            Debug.LogError($"[CardJsonLoader2] Resources/{resourcePath} 경로에 JSON 파일이 없습니다.");
            return cardList;
        }

        try
        {
            cardList = JsonUtilityWrapper.FromJsonList<CardJsonData2>(jsonFile.text);
            if (cardList == null)
                cardList = new List<CardJsonData2>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CardJsonLoader2] JSON 파싱 중 오류 발생: {e.Message}");
            cardList = new List<CardJsonData2>();
        }

        return cardList;
    }
}