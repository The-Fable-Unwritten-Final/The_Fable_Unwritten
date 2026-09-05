#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CardTableImporter
{
    private class CardTableRow
    {
        public int index;
        public int cost;
        public string name;
        public string text;
        public int type;
        public int characterClass;
        public string cardFrame;
        public int targetType;
        public int targetNum;

        public int arg1;
        public int arg2;
        public int arg3;
        public int arg4;
        public int arg5;

        public string flavorText;
        public string skillEffects;
        public List<string> keywordTypes = new();
        public string switchType;

        public int nextEvolveCardId;
        public int evolveCount;

        public string note;

        public List<int> soundIndexes = new();
        public List<float> soundDelays = new();
    }

    [Serializable]
    private class CardJsonWrapper
    {
        public List<CardJsonData> data;
    }

    public static void Import()
    {
        string csvPath = EditorUtility.OpenFilePanel("Card Table CSV 선택", "", "csv");

        if (string.IsNullOrEmpty(csvPath))
            return;

        string jsonPath = FindCardDataFinalPath();

        if (string.IsNullOrEmpty(jsonPath))
        {
            Debug.LogError("[CardTableImporter] CardDataFinal.json을 찾을 수 없습니다.");
            return;
        }

        List<CardTableRow> rows = ParseTable(csvPath);

        if (rows.Count == 0)
        {
            Debug.LogError("[CardTableImporter] 읽어온 카드 데이터가 없습니다.");
            return;
        }

        CardJsonWrapper wrapper = LoadCardJson(jsonPath);

        if (wrapper == null || wrapper.data == null)
        {
            Debug.LogError("[CardTableImporter] CardDataFinal.json 로드 실패");
            return;
        }

        if (!EditorUtility.DisplayDialog(
            "Card Table 반영",
            $"{rows.Count}개의 카드 데이터를 CardDataFinal.json에 반영합니다.\n\n" +
            "기존 effects 구조는 유지하고 수치만 변경합니다.\n계속하시겠습니까?",
            "반영",
            "취소"))
        {
            return;
        }

        Dictionary<int, CardJsonData> cardDict = new();

        foreach (CardJsonData card in wrapper.data)
        {
            if (card == null)
                continue;

            cardDict[card.index] = card;
        }

        int updated = 0;
        int skipped = 0;

        foreach (CardTableRow row in rows)
        {
            if (!cardDict.TryGetValue(row.index, out CardJsonData card))
            {
                Debug.LogWarning(
                    $"[CardTableImporter] 기존 CardDataFinal에 없는 카드: {row.index} - {row.name}"
                );

                skipped++;
                continue;
            }

            UpdateCard(card, row);
            updated++;
        }

        BackupJson(jsonPath);
        SaveCardJson(jsonPath, wrapper);

        AssetDatabase.Refresh();

        Debug.Log(
            $"[CardTableImporter] 완료 - 반영 {updated}개 / 건너뜀 {skipped}개"
        );
    }

    // =========================================================
    // CSV
    // =========================================================

    private static List<CardTableRow> ParseTable(string path)
    {
        List<CardTableRow> rows = new();

        string text = File.ReadAllText(path, Encoding.UTF8);
        List<List<string>> csv = ParseCsv(text);

        if (csv.Count == 0)
        {
            Debug.LogError("[CardTableImporter] CSV가 비어 있습니다.");
            return rows;
        }

        int headerIndex = FindHeaderRow(csv);

        if (headerIndex < 0)
        {
            Debug.LogError(
                "[CardTableImporter] 변수명 헤더를 찾을 수 없습니다. " +
                "index / cost / name 컬럼을 확인해주세요."
            );

            return rows;
        }

        List<string> header = csv[headerIndex];
        Dictionary<string, int> columns = BuildColumnMap(header);

        Debug.Log(
            $"[CardTableImporter] 헤더 발견: {headerIndex + 1}행 / " +
            $"컬럼 {columns.Count}개"
        );

        for (int i = headerIndex + 1; i < csv.Count; i++)
        {
            List<string> values = csv[i];

            if (!TryGetInt(values, columns, "index", out int index))
                continue;

            CardTableRow row = new CardTableRow
            {
                index = index,
                cost = GetInt(values, columns, "cost"),
                name = Get(values, columns, "name"),
                text = Get(values, columns, "text"),
                type = GetInt(values, columns, "type"),
                characterClass = GetInt(values, columns, "class"),
                cardFrame = Get(values, columns, "cardFrame"),
                targetType = GetInt(values, columns, "targetType"),
                targetNum = GetInt(values, columns, "targetNum"),

                arg1 = GetInt(values, columns, "arg1"),
                arg2 = GetInt(values, columns, "arg2"),
                arg3 = GetInt(values, columns, "arg3"),
                arg4 = GetInt(values, columns, "arg4"),
                arg5 = GetInt(values, columns, "arg5"),

                flavorText = Get(values, columns, "flavorText"),
                skillEffects = Get(values, columns, "skillEffects"),
                switchType = Get(values, columns, "switchType"),

                nextEvolveCardId = GetInt(values, columns, "nextEvolveCardId"),
                evolveCount = GetInt(values, columns, "evolveCount"),

                note = Get(values, columns, "note")


            };

            row.keywordTypes = ParseList(Get(values, columns, "keywordTypes"));
            row.soundIndexes = ParseIntList(Get(values, columns, "soundIndexes")
);

            row.soundDelays = ParseFloatList(Get(values, columns, "soundDelays"));

            rows.Add(row);
        }

        Debug.Log(
            $"[CardTableImporter] CSV 카드 {rows.Count}개 읽음"
        );

        return rows;
    }
    private static Dictionary<string, int> BuildColumnMap(List<string> header)
    {
        Dictionary<string, int> result =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < header.Count; i++)
        {
            string key = header[i]?.Trim();

            if (string.IsNullOrEmpty(key))
                continue;

            if (!result.ContainsKey(key))
                result.Add(key, i);
        }

        return result;
    }

    private static List<int> ParseIntList(string value)
    {
        List<int> result = new();

        if (string.IsNullOrWhiteSpace(value))
            return result;

        string[] split = value.Split(',');

        foreach (string item in split)
        {
            if (int.TryParse(item.Trim(), out int parsed))
                result.Add(parsed);
        }

        return result;
    }

    private static List<float> ParseFloatList(string value)
    {
        List<float> result = new();

        if (string.IsNullOrWhiteSpace(value))
            return result;

        string[] split = value.Split(',');

        foreach (string item in split)
        {
            if (float.TryParse(
                item.Trim(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float parsed))
            {
                result.Add(parsed);
            }
        }

        return result;
    }

    private static List<List<string>> ParseCsv(string text)
    {
        List<List<string>> rows = new();
        List<string> row = new();
        StringBuilder field = new();

        bool quoted = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"')
            {
                if (quoted && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    quoted = !quoted;
                }

                continue;
            }

            if (c == ',' && !quoted)
            {
                row.Add(field.ToString());
                field.Clear();
                continue;
            }

            if ((c == '\n' || c == '\r') && !quoted)
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;

                row.Add(field.ToString());
                field.Clear();

                if (!IsEmptyRow(row))
                    rows.Add(row);

                row = new List<string>();
                continue;
            }

            field.Append(c);
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());

            if (!IsEmptyRow(row))
                rows.Add(row);
        }

        return rows;
    }

    private static bool IsEmptyRow(List<string> row)
    {
        foreach (string value in row)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return false;
        }

        return true;
    }

    private static string Get(
        List<string> values,
        Dictionary<string, int> columns,
        string column)
    {
        if (!columns.TryGetValue(column, out int index))
            return "";

        if (index < 0 || index >= values.Count)
            return "";

        return values[index]?.Trim() ?? "";
    }

    private static int GetInt(
        List<string> values,
        Dictionary<string, int> columns,
        string column)
    {
        string value = Get(values, columns, column);

        if (int.TryParse(value, out int result))
            return result;

        return 0;
    }

    private static bool TryGetInt(
        List<string> values,
        Dictionary<string, int> columns,
        string column,
        out int result)
    {
        return int.TryParse(Get(values, columns, column), out result);
    }

    private static List<string> ParseList(string value)
    {
        List<string> result = new();

        if (string.IsNullOrWhiteSpace(value))
            return result;

        string[] split = value.Split(',');

        foreach (string item in split)
        {
            string trimmed = item.Trim();

            if (!string.IsNullOrEmpty(trimmed))
                result.Add(trimmed);
        }

        return result;
    }

    // =========================================================
    // Card 기본 데이터
    // =========================================================

    private static void UpdateCard(CardJsonData card, CardTableRow row)
    {
        card.cost = row.cost;
        card.name = row.name;
        card.text = row.text;
        card.type = row.type;
        card.@class = row.characterClass;
        card.cardframe = row.cardFrame;
        card.target_type = row.targetType;
        card.target_num = row.targetNum;

        card.flavortext = row.flavorText;
        card.skilleffect = row.skillEffects;

        card.evolveCount = row.evolveCount;
        card.evolveTarget = row.nextEvolveCardId;

        card.keywords = row.keywordTypes;
        card.switchType = row.switchType;
        card.note = row.note;

        card.soundIndexes = new List<int>(row.soundIndexes);
        card.soundDelays = new List<float>(row.soundDelays);

        // illustration은 기존 JSON 값 유지

        UpdateEffectValues(card, row);
    }

    // =========================================================
    // Effect 수치 갱신
    // =========================================================

    private static void UpdateEffectValues(CardJsonData card, CardTableRow row)
    {
        int a1 = row.arg1;
        int a2 = row.arg2;
        int a3 = row.arg3;
        int a4 = row.arg4;
        int a5 = row.arg5;

        switch (card.index)
        {
            // =================================================
            // Sophia
            // =================================================

            case 1000:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "burn", a2);
                SetConditionalResultValue(card, "isStance", "damagePercent", a3);
                break;

            case 1001:
                SetEffectValue(card, "atk", a1);
                SetConditionalResultValue(card, "enemyHasStatus", "atk", a2);
                break;

            case 1002:
                SetEffectValue(card, "self_damage", a1);
                SetEffectValue(card, "damage", a2);
                SetEffectValue(card, "burn", a3);
                SetConditionValue(card, "potentialGte", 0, a4);
                SetConditionalResultValue(card, "potentialGte", "damage", a2);
                break;

            case 1003:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "freeze", a2);
                SetEffectValue(card, "applyDamage", a3);
                break;

            case 1004:
                SetEffectValue(card, "self_damage", a1);
                SetEffectValue(card, "damage", a2);
                SetEffectValue(card, "freeze", a3);
                break;

            case 1005:
                SetEffectValue(card, "def", a1);
                SetConditionalResultValue(
                    card,
                    "justAfterSpecificStance",
                    "def",
                    a2
                );
                break;

            case 1006:
                SetEffectValue(card, "reduceNextCardCost", a1);
                SetEffectValue(card, "atk", -Mathf.Abs(a2));
                SetConditionValue(card, "potentialGte", 0, a3);
                SetConditionalResultValue(card, "potentialGte", "reduceCost", a4);
                break;

            case 1007:
                SetEffectValue(card, "heal", a1);
                break;

            case 1008:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "active", a2);
                SetConditionalResultValue(
                    card,
                    "usedAnyCardType",
                    "damage",
                    a3
                );
                SetConditionalResultValue(
                    card,
                    "justAfterStance",
                    "damagePercent",
                    a4
                );
                break;

            case 1009:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "burn", a2);
                break;

            case 1010:
                SetEffectValue(card, "draw", a1);
                SetEffectValue(card, "reduceDrawnCardCost", a2);
                SetConditionalResultValue(card, "isStance", "draw", a3);
                break;

            case 1011:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "active", a2);
                SetConditionalResultValue(card, "isStance", "damage", a1);
                break;

            case 1108:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "active", a2);
                SetConditionalResultValue(
                    card,
                    "justAfterStance",
                    "damagePercent",
                    a3
                );
                break;

            case 1109:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "burn", a2);
                SetConditionValue(card, "enemyStatusStackGte", 1, a3);
                SetConditionalResultValue(
                    card,
                    "enemyStatusStackGte",
                    "damage",
                    a1
                );
                break;

            case 1110:
                SetEffectValue(card, "draw", a1);
                SetEffectValue(
                    card,
                    "reduceRandomCardCostByFrozenEnemyCount",
                    a3
                );
                break;

            case 1111:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "active", a2);
                SetConditionalResultValue(
                    card,
                    "usedAllCardTypes",
                    "damage",
                    a1
                );
                break;

            case 1210:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "active", a2);

                SetConditionalResultValueByConditionValue(
                    card,
                    "isStance",
                    "Seek",
                    "damagePercent",
                    a3
                );

                SetConditionalResultValueByConditionValue(
                    card,
                    "isStance",
                    "Insight",
                    "freeze",
                    a4
                );
                break;

            case 1211:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "burn", a2);
                SetEffectValue(card, "lockPotentialCharge", a3);
                break;

            case 1300:
                SetEffectValue(card, "self_damage", a1);
                SetEffectValue(card, "damage", a2);
                SetEffectValue(card, "burn", a3);
                SetConditionValue(card, "potentialGte", 0, a4);
                SetConditionalResultValue(card, "potentialGte", "damage", a2);
                break;

            case 1301:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "burn", a2);
                SetConditionalResultValue(card, "isStance", "damagePercent", a3);
                break;

            case 1302:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "freeze", a2);
                SetConditionalResultValue(card, "isStance", "damage", a1);
                break;

            case 1303:
                SetEffectValue(card, "atk", a1);
                SetEffectValue(card, "active", a2);
                break;

            case 1400:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "burn", a2);
                break;

            // =================================================
            // Kayla
            // =================================================

            case 2000:
                SetEffectValue(card, "atk", a1);
                SetEffectValue(card, "bless", a2);
                SetConditionalResultValue(card, "isStance", "penance", a3);
                break;

            case 2001:
                SetEffectValue(card, "atk", a1);
                SetEffectValue(card, "reduceNextCardCost", a2);
                SetEffectValue(card, "bless", a3);
                SetConditionValue(card, "potentialGte", 0, a4);
                SetConditionalResultValue(card, "potentialGte", "reduceCost", a5);
                break;

            case 2002:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "sin", a2);
                SetConditionalResultValue(card, "isStance", "damage", a1);
                break;

            case 2003:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "sin", a2);
                SetConditionalResultValue(
                    card,
                    "usedCardType",
                    "damagePercent",
                    a3
                );
                break;

            case 2004:
                SetEffectValue(card, "heal", a1);
                SetEffectValue(card, "penance", a2);
                SetConditionalResultValue(card, "isStance", "heal", a3);
                break;

            case 2005:
                SetEffectValue(card, "heal", a1);
                SetEffectValue(card, "penance", a4);
                SetConditionValue(card, "sinTotalGte", 0, a3);
                SetConditionalResultValue(card, "sinTotalGte", "heal", a2);
                break;

            case 2006:
                SetEffectValue(card, "heal", a1);
                SetEffectValue(card, "draw", a2);
                SetEffectValue(card, "penance", a3);
                SetConditionalResultValue(card, "isStance", "heal", a4);
                break;

            case 2007:
                SetEffectValue(card, "atk", a1);
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "bless", a2);
                SetConditionalResultValue(
                    card,
                    "justAfterStance",
                    "def",
                    a3
                );
                break;

            case 2008:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "sin", a2);
                SetConditionValue(card, "sinTotalGte", 0, a3);
                SetConditionalResultValue(
                    card,
                    "sinTotalGte",
                    "damagePercent",
                    a4
                );
                break;

            case 2009:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "sin", a2);
                SetConditionalResultValue(card, "isStance", "damage", a1);
                break;

            case 2010:
                SetEffectValue(card, "heal", a1);
                SetEffectValue(card, "penance", a2);
                break;

            case 2011:
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "bless", a2);
                SetConditionalResultValue(card, "isStance", "def", a3);
                break;

            case 2108:
                SetEffectValue(card, "atk", a1);
                SetEffectValue(card, "sin", a2);
                break;

            case 2109:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "sin", a2);
                SetConditionValue(card, "sinTotalGte", 0, a4);
                SetConditionalResultValue(card, "sinTotalGte", "damage", a1);
                break;

            case 2110:
                SetEffectValue(card, "heal", a1);
                SetEffectValue(card, "penance", a2);
                SetEffectValue(card, "lockPotentialCharge", a4);
                break;

            case 2111:
                SetEffectValue(card, "multiplyBless", a1);
                SetEffectValue(card, "bless", a2);
                break;

            case 2210:
                SetEffectValue(card, "heal", a1);
                SetEffectValue(card, "healByPenance", a2);
                break;

            case 2211:
                SetEffectValue(card, "bless", a1);
                SetConditionalResultValue(card, "isStance", "bless", a2);
                break;

            case 2300:
                SetEffectValue(card, "multiplyBuff", a1);
                SetEffectValue(card, "bless", a2);
                break;

            case 2301:
                SetEffectValue(
                    card,
                    "damageByRemovedDebuffSumMultiplier",
                    a1
                );
                SetConditionalResultValue(
                    card,
                    "isStance",
                    "damagePercent",
                    a2
                );
                break;

            case 2400:
                SetEffectValue(card, "heal", a1);
                SetEffectValue(card, "bless", a2);

                SetConditionalResultValueByConditionValue(
                    card,
                    "isStance",
                    "Mercy",
                    "bless",
                    a3
                );

                SetConditionalResultValueByConditionValue(
                    card,
                    "isStance",
                    "Discipline",
                    "penance",
                    a4
                );
                break;

            // =================================================
            // Leon
            // =================================================

            case 3000:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "scar", a2);
                SetConditionalResultValue(card, "isStance", "damage", a1);
                break;

            case 3001:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "scar", a2);
                SetEffectValue(card, "applyDamage", a3);
                break;

            case 3002:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "stun", a2);
                SetEffectValue(card, "reduceNextCardCost", a3);
                SetConditionalResultValue(card, "isStance", "stun", a4);
                break;

            case 3003:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "draw", a2);
                SetEffectValue(card, "stun", a3);
                break;

            case 3004:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "stun", a2);
                SetConditionalResultValue(
                    card,
                    "isStance",
                    "damagePercent",
                    a3
                );
                break;

            case 3005:
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "draw", a2);
                SetEffectValue(card, "guard", a3);
                SetConditionalResultValue(card, "isStance", "def", a4);
                break;

            case 3006:
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "guard", a2);
                break;

            case 3007:
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "guard", a2);
                SetEffectValue(card, "atk", a3);
                SetConditionalResultValue(card, "isStance", "def", a4);
                break;

            case 3008:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "stun", a2);
                SetConditionalResultValue(
                    card,
                    "justAfterStanceEffect",
                    "damage",
                    a1
                );
                break;

            case 3009:
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "guard", a2);
                SetConditionValue(card, "potentialGte", 0, a3);
                SetConditionalResultValue(card, "potentialGte", "def", a4);
                break;

            case 3010:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "stun", a2);
                SetConditionalResultValue(
                    card,
                    "isStance",
                    "damagePercent",
                    a3
                );
                break;

            case 3011:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "scar", a2);

                SetConditionalResultValueByConditionValue(
                    card,
                    "enemyHasStatus",
                    "scar",
                    "damage",
                    a3
                );

                SetConditionalResultValueByConditionValue(
                    card,
                    "isStance",
                    "Rush",
                    "damage",
                    a4
                );
                break;

            case 3108:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "stun", a2);
                SetConditionalResultValue(
                    card,
                    "justAfterStance",
                    "damagePercent",
                    a3
                );
                break;

            case 3109:
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "guard", a2);
                SetConditionalResultValue(card, "isStance", "def", a3);
                break;

            case 3110:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "stun", a2);
                SetConditionalResultValue(
                    card,
                    "enemyHasStatus",
                    "damage",
                    a1
                );
                break;

            case 3111:
                SetEffectValue(card, "damage", a1);

                SetNthEffectValue(card, "scar", 0, a2);
                SetNthEffectValue(card, "scar", 1, a2);

                SetConditionalResultValue(
                    card,
                    "isStance",
                    "scar",
                    a2
                );
                break;

            case 3210:
                SetEffectValue(card, "truedamage", a1);
                SetEffectValue(card, "stun", a2);
                SetConditionalResultValue(
                    card,
                    "justAfterStance",
                    "damagePercent",
                    a3
                );
                break;

            case 3211:
                SetEffectValue(card, "damage", a1);
                SetEffectValue(card, "scar", a2);
                SetConditionalResultValue(card, "isStance", "damage", a1);
                break;

            case 3400:
                SetEffectValue(card, "def", a1);
                SetEffectValue(card, "guard", a2);

                // undying.value는 "한 번만"이라는 구조값이라 유지.
                // arg3은 체력 하한값인데 현재 ResultEffect 구조로
                // 직접 저장되는 값이 아니므로 건드리지 않는다.

                SetConditionalResultValue(
                    card,
                    "justAfterStance",
                    "def",
                    a4
                );
                break;

            case 4000:
                // 이상실현 선택 카드. 현재 수치 없음.
                break;

            default:
                Debug.LogWarning(
                    $"[CardTableImporter] 수치 매핑이 없는 카드: {card.index}"
                );
                break;
        }
    }

    // =========================================================
    // Effect Helpers
    // =========================================================

    private static void SetEffectValue(
        CardJsonData card,
        string type,
        int value)
    {
        if (card.effects == null)
        {
            WarnMissing(card, $"Effect '{type}'");
            return;
        }

        CardEffect effect = card.effects.Find(e =>
            e != null &&
            e.type == type
        );

        if (effect == null)
        {
            WarnMissing(card, $"Effect '{type}'");
            return;
        }

        effect.value = value;
    }

    private static void SetNthEffectValue(
        CardJsonData card,
        string type,
        int nth,
        int value)
    {
        if (card.effects == null)
        {
            WarnMissing(card, $"Effect '{type}' [{nth}]");
            return;
        }

        int count = 0;

        foreach (CardEffect effect in card.effects)
        {
            if (effect == null || effect.type != type)
                continue;

            if (count == nth)
            {
                effect.value = value;
                return;
            }

            count++;
        }

        WarnMissing(card, $"Effect '{type}' [{nth}]");
    }

    private static void SetConditionalResultValue(
        CardJsonData card,
        string trigger,
        string resultType,
        int value)
    {
        if (card.effects == null)
        {
            WarnMissing(card, $"{trigger} -> {resultType}");
            return;
        }

        CardEffect effect = card.effects.Find(e =>
            e != null &&
            e.type == "conditional" &&
            e.condition != null &&
            e.condition.trigger == trigger &&
            e.result != null &&
            e.result.type == resultType
        );

        if (effect == null)
        {
            WarnMissing(card, $"{trigger} -> {resultType}");
            return;
        }

        effect.result.value = value;
    }

    private static void SetConditionalResultValueByConditionValue(
        CardJsonData card,
        string trigger,
        string conditionValue,
        string resultType,
        int value)
    {
        if (card.effects == null)
        {
            WarnMissing(
                card,
                $"{trigger}({conditionValue}) -> {resultType}"
            );
            return;
        }

        CardEffect effect = card.effects.Find(e =>
            e != null &&
            e.type == "conditional" &&
            e.condition != null &&
            e.condition.trigger == trigger &&
            ContainsConditionValue(e.condition, conditionValue) &&
            e.result != null &&
            e.result.type == resultType
        );

        if (effect == null)
        {
            WarnMissing(
                card,
                $"{trigger}({conditionValue}) -> {resultType}"
            );
            return;
        }

        effect.result.value = value;
    }

    private static void SetConditionValue(
        CardJsonData card,
        string trigger,
        int valueIndex,
        int value)
    {
        if (card.effects == null)
        {
            WarnMissing(card, $"Condition '{trigger}'");
            return;
        }

        CardEffect effect = card.effects.Find(e =>
            e != null &&
            e.type == "conditional" &&
            e.condition != null &&
            e.condition.trigger == trigger
        );

        if (effect == null)
        {
            WarnMissing(card, $"Condition '{trigger}'");
            return;
        }

        if (effect.condition.value == null)
            effect.condition.value = new List<string>();

        while (effect.condition.value.Count <= valueIndex)
            effect.condition.value.Add("");

        effect.condition.value[valueIndex] = value.ToString();
    }

    private static bool ContainsConditionValue(
        EffectCondition condition,
        string value)
    {
        if (condition == null || condition.value == null)
            return false;

        foreach (string item in condition.value)
        {
            if (string.Equals(
                item,
                value,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void WarnMissing(CardJsonData card, string value)
    {
        Debug.LogWarning(
            $"[CardTableImporter] {card.index}: 기존 구조에서 {value} 찾지 못함"
        );
    }

    // =========================================================
    // JSON
    // =========================================================

    private static string FindCardDataFinalPath()
    {
        string[] guids = AssetDatabase.FindAssets("CardDataFinal t:TextAsset");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (Path.GetFileNameWithoutExtension(assetPath) != "CardDataFinal")
                continue;

            return Path.GetFullPath(assetPath);
        }

        return null;
    }

    private static CardJsonWrapper LoadCardJson(string path)
    {
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            return JsonUtility.FromJson<CardJsonWrapper>(json);
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[CardTableImporter] CardDataFinal 로드 실패\n{e}"
            );

            return null;
        }
    }

    private static void SaveCardJson(
        string path,
        CardJsonWrapper wrapper)
    {
        try
        {
            string json = JsonUtility.ToJson(wrapper, true);

            File.WriteAllText(
                path,
                json,
                new UTF8Encoding(false)
            );

            Debug.Log(
                $"[CardTableImporter] CardDataFinal 저장 완료\n{path}"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[CardTableImporter] CardDataFinal 저장 실패\n{e}"
            );
        }
    }

    private static void BackupJson(string path)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);

            string backupPath = Path.Combine(
                directory,
                $"{fileName}_Backup.json"
            );

            File.Copy(path, backupPath, true);

            Debug.Log(
                $"[CardTableImporter] 백업 생성\n{backupPath}"
            );
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                $"[CardTableImporter] 백업 생성 실패\n{e}"
            );
        }
    }
    private static int FindHeaderRow(List<List<string>> csv)
    {
        for (int i = 0; i < csv.Count; i++)
        {
            List<string> row = csv[i];

            bool hasIndex = false;
            bool hasCost = false;
            bool hasName = false;
            bool hasArg1 = false;

            foreach (string cell in row)
            {
                string value = cell?.Trim();

                if (string.Equals(
                    value,
                    "index",
                    StringComparison.OrdinalIgnoreCase))
                {
                    hasIndex = true;
                }
                else if (string.Equals(
                    value,
                    "cost",
                    StringComparison.OrdinalIgnoreCase))
                {
                    hasCost = true;
                }
                else if (string.Equals(
                    value,
                    "name",
                    StringComparison.OrdinalIgnoreCase))
                {
                    hasName = true;
                }
                else if (string.Equals(
                    value,
                    "arg1",
                    StringComparison.OrdinalIgnoreCase))
                {
                    hasArg1 = true;
                }
            }

            if (hasIndex && hasCost && hasName && hasArg1)
                return i;
        }

        return -1;
    }
}
#endif