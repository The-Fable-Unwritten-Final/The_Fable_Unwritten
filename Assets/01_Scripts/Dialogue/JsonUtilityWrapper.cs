using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity의 JsonUtility는 루트가 배열인 JSON을 직접 파싱할 수 없기 때문에
/// 이를 감싸는 Wrapper 구조를 통해 List를 쉽게 파싱하도록 도와주는 클래스
/// </summary>
public static class JsonUtilityWrapper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> data;
    }

    public static List<T> FromJsonList<T>(string json)
    {
        return JsonUtility.FromJson<Wrapper<T>>(json).data;
    }

    public static string ToJsonList<T>(List<T> list)
    {
        var wrapper = new Wrapper<T> { data = list };
        return JsonUtility.ToJson(wrapper, true);
    }
}
