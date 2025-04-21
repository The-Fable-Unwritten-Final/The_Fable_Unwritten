using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class DiaryCSVParser
{
    public static List<DiaryData> Parse(string path)
    {
        string fullPath = $"{Application.dataPath}/Resources/{path}";

        var list = new List<DiaryData>();

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[DiaryCSVParser] 경로에 CSV 파일이 없습니다: {path}");
            return list;
        }

        var lines = File.ReadAllLines(fullPath);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] tokens = lines[i].Split(',');

            try
            {
                var data = new DiaryData
                {
                    index = ParseInt(tokens, 0),
                    tag_num = ParseInt(tokens, 1),
                    title = ParseString(tokens, 2),
                    contents = ParseString(tokens, 3),
                    isOpen = false,
                };

                list.Add(data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DiaryCSVParser] {i + 1}번째 줄 파싱 오류: {e.Message}");
            }
        }

        return list;
    }

    private static int ParseInt(string[] tokens, int index)
    {
        if (int.TryParse(tokens[index], out int result))
        {
            return result;
        }
        else
        {
            Debug.LogError($"[DiaryCSVParser] {index}번째 인덱스 파싱 오류: {tokens[index]}");
            return 0;
        }
    }
    private static string ParseString(string[] tokens, int index)
    {
        if (index < tokens.Length)
        {
            return tokens[index];
        }
        else
        {
            Debug.LogError($"[DiaryCSVParser] {index}번째 인덱스 파싱 오류: {tokens[index]}");
            return string.Empty;
        }
    }
}


