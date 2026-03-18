using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonDebugValidator
{
    public static void ValidateJson(string resourcePath)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        if (jsonFile == null)
        {
            Debug.LogError($"JSON 파일 없음: Resources/{resourcePath}");
            return;
        }

        try
        {
            using var sr = new StringReader(jsonFile.text);
            using var reader = new JsonTextReader(sr);

            while (reader.Read())
            {
                // 끝까지 읽기만 해도 문법 오류 위치를 잡아줌
            }

            Debug.Log("JSON 문법 정상");
        }
        catch (JsonReaderException e)
        {
            Debug.LogError(
                $"JSON 문법 오류\n" +
                $"Line: {e.LineNumber}, Position: {e.LinePosition}\n" +
                $"Message: {e.Message}"
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"기타 오류: {e}");
        }
    }
}