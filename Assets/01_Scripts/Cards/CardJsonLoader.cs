using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardJsonLoader 
{
    public static List<CardJsonData> Load(string resourcePath)
    {
        var cardList = new List<CardJsonData>();
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        
        if (jsonFile == null)
        {
            Debug.LogError($"[CardJsonLoader] Resources/{resourcePath} 경로에 JSON 파일이 없습니다.");
            return cardList;
        }
        try
        {
            cardList = JsonUtilityWrapper.FromJsonList<CardJsonData>(jsonFile.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[CardJsonLoader] JSON 파싱 중 오류 발생: " + e.Message);
        }
        return cardList;
    }
}
