using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class EnemyCSVParser
{
    public static List<EnemyParsed> Parse(string path)
    {
        var list = new List<EnemyParsed>();

        if (!File.Exists(path))
        {
            Debug.LogError($"[EnemyCSVParser] 경로에 CSV 파일이 없습니다: {path}");
            return list;
        }

        var lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] t = lines[i].Split(',');

            try
            {
                var data = new EnemyParsed
                {
                    id = ParseInt(t, 0),
                    enemyName = ParseString(t, 1),
                    hp = ParseInt(t, 2),
                    art = ParseString(t, 3),
                    atkBuff = ParseFloat(t, 4),
                    defBuff = ParseFloat(t, 5),
                    block = !string.IsNullOrWhiteSpace(t[6]),
                    blind = !string.IsNullOrWhiteSpace(t[7]),
                    stun = !string.IsNullOrWhiteSpace(t[8]),
                    skill0 = ParseInt(t, 9),
                    damage0 = ParseInt(t, 10),
                    percentage0 = ParseFloat(t, 11),
                    skill1 = ParseInt(t, 12),
                    damage1 = ParseInt(t, 13),
                    percentage1 = ParseFloat(t, 14),
                    skill2 = ParseInt(t, 15),
                    damage2 = ParseInt(t, 16),
                    percentage2 = ParseFloat(t, 17),
                    topPercentage = ParseFloat(t, 18),
                    middlePercentage = ParseFloat(t, 19),
                    bottomPercentage = ParseFloat(t, 20),
                    loot = ParseString(t, 21),
                    note = ParseString(t, 22)
                };

                list.Add(data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EnemyCSVParser] {i + 1}번째 줄 파싱 오류: {e.Message}");
            }
        }

        return list;
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
