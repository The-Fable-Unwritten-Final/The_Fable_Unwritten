using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class EnemyActCSVParser
{
    public static List<EnemyAct> Parse(string path)
    {
        var list = new List<EnemyAct>();

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
                var act = ScriptableObject.CreateInstance<EnemyAct>();

                act.index = ParseInt(t, 0);
                act.targetType = (TargetType)ParseInt(t, 1);
                act.targetNum = ParseInt(t, 2);

                act.target_front = ParseBool(t, 3);
                act.target_center = ParseBool(t, 4);
                act.target_back = ParseBool(t, 5);

                act.atk_buff = ParseInt(t, 6);
                act.def_buff = ParseInt(t, 7);

                act.buff_time = ParseInt(t, 8);
                act.block = ParseBool(t, 9);
                act.stun = ParseInt(t, 10);

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
