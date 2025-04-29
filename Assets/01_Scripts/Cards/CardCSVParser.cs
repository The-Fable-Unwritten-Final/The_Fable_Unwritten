using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CardCSVParser           //csv로부터 데이터를 받아 CardData 형식으로 우선 저장해 놓음
{
    public static List<CardData> Parse(string path)
    {
        var list = new List<CardData>();

        if (!File.Exists(path))
        {
            Debug.LogError($"[CardCSVParser] 경로에 CSV 파일이 없습니다: {path}");
            return list;
        }

        var lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] tokens = lines[i].Split(',');

            try
            {
                var data = new CardData
                {
                    index = ParseInt(tokens, 0),
                    cost = ParseInt(tokens, 1),
                    illustration = ParseString(tokens, 2),
                    cardName = ParseString(tokens, 3),
                    text = ParseString(tokens, 4),
                    type = ParseInt(tokens, 5),
                    classIndex = ParseInt(tokens, 6),
                    cardFrame = ParseString(tokens, 7),    
                    targetType = ParseInt(tokens, 8),
                    targetNum = ParseInt(tokens, 9),
                    damage = ParseFloat(tokens, 10),
                    discount = ParseInt(tokens, 11),
                    draw = ParseInt(tokens, 12),
                    redraw = ParseInt(tokens, 13),
                    atkBuff = ParseFloat(tokens, 14),
                    defBuff = ParseFloat(tokens, 15),
                    buffTime = ParseInt(tokens, 16),       
                    selfDamage = ParseFloat(tokens, 17),
                    block = ParseInt(tokens, 18),
                    blind = ParseInt(tokens, 19),
                    stun = ParseInt(tokens, 20),
                    characterStance = ParseString(tokens, 21),
                    description = ParseString(tokens, 22),
                    flavorText = ParseString(tokens,23),
                    skillEffect = ParseString(tokens, 24)
                };

                list.Add(data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CardCSVParser] {i + 1}번째 줄 파싱 오류: {e.Message}");
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
