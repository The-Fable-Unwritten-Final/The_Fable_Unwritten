using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class EnemyActCSVParser
{
    /// <summary>
    /// Resources.Load<TextAsset>()로 불러온 csvText.text에서 파싱
    /// </summary>
    public static List<EnemyAct> ParseEnemyAct(string text)
    {
        var list = new List<EnemyAct>();

        var lines = text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] t = lines[i].Split(',');

            try
            {
                var act = new EnemyAct
                {
                    index = ParseInt(t, 0),
                    targetType = (TargetType)ParseInt(t, 1),
                    targetNum = ParseInt(t, 2),

                    target_front = ParseBool(t, 3),
                    target_center = ParseBool(t, 4),
                    target_back = ParseBool(t, 5),

                    atk_buff = ParseInt(t, 6),
                    def_buff = ParseInt(t, 7),

                    buff_time = ParseInt(t, 8),
                    block = ParseBool(t, 9),
                    stun = ParseInt(t, 10)
                };

                list.Add(act);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EnemyCSVParser] {i + 1}번째 줄 파싱 오류: {e.Message}");
            }
        }

        return list;
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

    private static bool ParseBool(string[] tokens, int index)
    {
        if (index >= tokens.Length) return false;
        string val = tokens[index].Trim();
        return !string.IsNullOrEmpty(val) && val != "0";
    }
}
