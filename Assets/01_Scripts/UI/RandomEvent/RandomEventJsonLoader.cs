using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomEventJsonLoader
{
    private const string JsonPath = "ExternalFiles/RandomEventText"; // 저장 경로

    public static List<RandomEventData> LoadAllEvents()
    {
        TextAsset json = Resources.Load<TextAsset>(JsonPath);
        if (json == null)
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다: {JsonPath}");
        }

        List<RandomEventData> eventList = JsonUtility.FromJson<Wrapper>(WrapJson(json.text)).events;

        foreach (var evnt in eventList)
        {
            evnt.illustrationSprite = Resources.Load<Sprite>($"RandomEvent/{evnt.illustration}");
            if (evnt.illustrationSprite == null)
                Debug.LogWarning($"일러스트 '{evnt.illustration}' 로드 실패");
        }

        Debug.Log($"랜덤 이벤트 {eventList.Count}개 로드됨.");
        return eventList;
    }

    [Serializable]
    private class Wrapper
    {
        public List<RandomEventData> events;
    }

    private static string WrapJson(string rawJson)
    {
        return "{\"events\":" + rawJson + "}";
    }
}
