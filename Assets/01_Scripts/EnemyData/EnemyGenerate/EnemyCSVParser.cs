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
                    art = ParseString(t, 2),
                    hp = ParseInt(t, 3),
                    hpScale = ParseFloat(t, 4),
                    damageScale = ParseFloat(t, 5),
                    skillEffect = ParseString(t, 6),

                    skill0 = ParseInt(t, 7),
                    damage0 = ParseInt(t, 8),
                    percentage0 = ParseFloat(t, 9),

                    skill1 = ParseInt(t, 10),
                    damage1 = ParseInt(t, 11),
                    percentage1 = ParseFloat(t, 12),

                    skill2 = ParseInt(t, 13),
                    damage2 = ParseInt(t, 14),
                    percentage2 = ParseFloat(t, 15),

                    topPercentage = ParseFloat(t, 16),
                    middlePercentage = ParseFloat(t, 17),
                    bottomPercentage = ParseFloat(t, 18),

                    atkBuff = ParseFloat(t, 19),
                    defBuff = ParseFloat(t, 20),

                    block = !string.IsNullOrWhiteSpace(t[21]),
                    blind = !string.IsNullOrWhiteSpace(t[22]),
                    stun = !string.IsNullOrWhiteSpace(t[23]),
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
