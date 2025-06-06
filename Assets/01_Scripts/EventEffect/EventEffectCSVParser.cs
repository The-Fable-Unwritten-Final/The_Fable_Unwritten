using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class EventEffectCSVParser : MonoBehaviour
{
    public static List<EventEffectData> Parse(string csvText)
    {
        var list = new List<EventEffectData>();

        var lines = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        for (int i = 1; i < lines.Length; i++)
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
                    note = ParseString(tokens, 16),
                    battle = ParseInt(tokens, 17)
                };

                list.Add(data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EventEffectCSVParser] {i + 1}번째 줄 파싱 오류: {e.Message}");
            }
        }

        return list;
    }

    private static bool ParseBool(string[] tokens, int index)
    {
        if (index >= tokens.Length) return false;
        string val = tokens[index].Trim();
        return !string.IsNullOrEmpty(val) && val != "0";
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
