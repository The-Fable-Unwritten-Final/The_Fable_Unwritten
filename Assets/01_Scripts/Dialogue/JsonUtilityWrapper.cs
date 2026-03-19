using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity의 JsonUtility는 루트가 배열인 JSON을 직접 파싱할 수 없기 때문에
/// Wrapper 구조를 통해 List를 쉽게 파싱하도록 도와주는 클래스
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
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("[JsonUtilityWrapper] JSON 문자열이 비어 있습니다.");
            return new List<T>();
        }

        try
        {
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);

            if (wrapper == null)
            {
                Debug.LogWarning("[JsonUtilityWrapper] Wrapper 파싱 결과가 null입니다.");
                return new List<T>();
            }

            if (wrapper.data == null)
            {
                Debug.LogWarning("[JsonUtilityWrapper] JSON에 data 필드가 없거나 null입니다.");
                return new List<T>();
            }

            return wrapper.data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonUtilityWrapper] JSON 리스트 파싱 실패: {e.Message}");
            return new List<T>();
        }
    }

    public static string ToJsonList<T>(List<T> list)
    {
        var wrapper = new Wrapper<T>
        {
            data = list ?? new List<T>()
        };

        return JsonUtility.ToJson(wrapper, true);
    }
}