using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

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

    public static void LoadCardCsv(TextAsset csv)
    {
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
            var cols = lines[i].Split(',');
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
}
