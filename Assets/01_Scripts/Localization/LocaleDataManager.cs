using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Linq;
using System.Text;

public static class LocaleDataManager
{
    private static Dictionary<string, string[]> _cardTable = new Dictionary<string, string[]>();
    private static Dictionary<string, string[]> _dialogueTable = new Dictionary<string, string[]>();
    private static Dictionary<string, string[]> _uiTable = new Dictionary<string, string[]>();

    private static Dictionary<string, int> _langColumnMap = new Dictionary<string, int>()
    {
        {"en", 1},
        {"ko", 2},
        {"ja", 3}
    };

    public static void LoadCardCsv()
    {
        TextAsset csv = Resources.Load<TextAsset>("ExternalFiles/CardLocaleData");
        _cardTable = LoadCsvToDictionary(csv);
    }

    public static void LoadDialogueCsv(TextAsset csv)
    {
        _dialogueTable = LoadCsvToDictionary(csv);
    }

    public static void LoadUICsv(TextAsset csv)
    {
        _uiTable = LoadCsvToDictionary(csv);
    }

    private static Dictionary<string, string[]> LoadCsvToDictionary(TextAsset csv)
    {
        var dict = new Dictionary<string, string[]>();
        var lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var cols = ParseCsvLine(lines[i]);

            // 각 필드별로 큰따옴표 제거 처리
            for (int j = 0; j < cols.Length; j++)
            {
                cols[j] = TrimQuotes(cols[j]);
            }

            if (cols.Length > 1)
                dict[cols[0].Trim()] = cols;
        }

        return dict;
    }

    public static string GetLocalizedCard(string key)
    {
        return GetLocalizedStringFromDict(_cardTable, key);
    }

    public static string GetLocalizedDialogue(string key)
    {
        return GetLocalizedStringFromDict(_dialogueTable, key);
    }

    public static string GetLocalizedUI(string key)
    {
        return GetLocalizedStringFromDict(_uiTable, key);
    }

    private static string GetLocalizedStringFromDict(Dictionary<string, string[]> dict, string key)
    {
        if (!dict.TryGetValue(key, out var row))
            return $"#{key}";

        var langCode = CurrentLanguageCode;
        if (!_langColumnMap.TryGetValue(langCode, out int colIndex))
            colIndex = 1;

        if (colIndex >= row.Length)
            return $"#{key}";

        return row[colIndex];
    }

    public static string CurrentLanguageCode
    {
        get
        {
            var locale = LocalizationSettings.SelectedLocale ?? LocalizationSettings.AvailableLocales.Locales[0];
            return locale.Identifier.Code.Split('-')[0].ToLower();
        }
    }

    /// <summary>
    /// CSV 라인에서 쉼표(및 기타 여러 조건)로 구분된 값을 분리하는 메서드
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private static string[] ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')  // "" → "
                    {
                        current.Append('"');
                        i++; // skip second quote
                    }
                    else
                    {
                        inQuotes = false; // 닫힘
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        result.Add(current.ToString()); // 마지막 항목 추가
        return result.ToArray();
    }
    private static string TrimQuotes(string input)
    {
        if (input.Length >= 2 && input.StartsWith("\"") && input.EndsWith("\""))
        {
            // 양끝 큰따옴표 제거 + 내부 "" → "
            return input.Substring(1, input.Length - 2).Replace("\"\"", "\"");
        }
        return input;
    }
}
