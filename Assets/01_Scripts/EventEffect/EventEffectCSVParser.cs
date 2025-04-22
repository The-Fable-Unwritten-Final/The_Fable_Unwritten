using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EventEffectCSVParser : MonoBehaviour
{
    public static List<EventEffectData> Parse(string path)
    {
        var list = new List<EventEffectData>();

        if(!File.Exists(path))
        {
            Debug.LogError($"[EventEffectCSVParser] 경로에 CSV 파일이 없습니다: {path}");
            return list;
        }

        var lines = File.ReadAllLines(path);

        for(int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] tokens = lines[i].Split(',');

            try
            {
                var data = new EventEffectData
                {
                    index = ParseInt(tokens, 0),
                    text = ParseString(tokens, 1),
                    eventType = ParseInt(tokens, 2),
                    duration = ParseInt(tokens, 3),
                    sophia = ParseBool(tokens, 4),
                    kyla = ParseBool(tokens, 5),
                    leon = ParseBool(tokens, 6),
                    enemy = ParseBool(tokens, 7),
                    cardType = ParseInt(tokens, 8),
                    cost = ParseInt(tokens, 9),
                    hp = ParseInt(tokens, 10),
                    hpPercent = ParseFloat(tokens, 11),
                    newCardIndex = ParseInt(tokens, 12),
                    atk = ParseInt(tokens, 13),
                    def = ParseInt(tokens, 14),
                    unusable = ParseBool(tokens, 15),
                    note = ParseString(tokens, 16)
                };

                list.Add(data);
            }
            catch(System.Exception e)
            {
                Debug.LogWarning($"[EventEffectCSVParser] {i + 1}번째 줄 파싱 오류: {e.Message}");
            }
        }

        return list;
    }

    private static bool ParseBool(string[] tokens, int index)
    {
        if (index >= tokens.Length || string.IsNullOrWhiteSpace(tokens[index])) return false;
        return tokens[index].Trim().ToLower() == "true";
    }

    private static string ParseString(string[] tokens, int index)
    {
        return index < tokens.Length ? tokens[index] : string.Empty;
    }

    private static int ParseInt(string[] tokens, int index)
    {
        if (index >= tokens.Length || string.IsNullOrWhiteSpace(tokens[index])) return 0;
        int.TryParse(tokens[index], out int result);
        return result;
    }

    private static float ParseFloat(string[] tokens, int index)
    {
        if (index >= tokens.Length || string.IsNullOrWhiteSpace(tokens[index])) return 0f;
        float.TryParse(tokens[index], out float result);
        return result;
    }
}
