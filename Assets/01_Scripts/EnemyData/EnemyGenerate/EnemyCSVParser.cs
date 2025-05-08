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
                    exp = ParseInt(t, 4),

                    loots = new List<int>(),

                    attackEffect = ParseString(t, 8),
                    allyEffect = ParseString(t, 9),

                    skillIndices = new int[5],
                    skillDamages = new float[5],
                    skillPercents = new float[5]
                };

                //스킬 처리
                for (int j = 0; j < 5; j++)
                {
                    int baseCol = 10 + j * 3;
                    data.skillIndices[j] = ParseInt(t, baseCol);
                    data.skillDamages[j] = ParseFloat(t, baseCol + 1);
                    data.skillPercents[j] = ParseFloat(t, baseCol + 2);
                }

                // loot0 ~ loot2 처리
                for (int lootIndex = 5; lootIndex <= 7; lootIndex++)
                {
                    string raw = ParseString(t, lootIndex);
                    if (string.IsNullOrWhiteSpace(raw)) continue;

                    if (int.TryParse(raw, out int lootValue))
                        data.loots.Add(lootValue);
                }

                data.topPercentage = ParseFloat(t, 25);
                data.middlePercentage = ParseFloat(t, 26);
                data.bottomPercentage = ParseFloat(t, 27);

                data.atkBuff = ParseFloat(t, 28);
                data.defBuff = ParseFloat(t, 29);

                data.block = !string.IsNullOrWhiteSpace(t[30]);
                data.blind = !string.IsNullOrWhiteSpace(t[31]);
                data.stun = !string.IsNullOrWhiteSpace(t[32]);

                data.note = ParseString(t, 33);

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

