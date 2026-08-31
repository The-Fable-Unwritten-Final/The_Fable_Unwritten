using System;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyActCSVParser
{
    // =========================================================
    // V1 Legacy Parser
    // 기존 EnemyAct 파일을 계속 사용할 수 있도록 유지
    // =========================================================

    public static List<EnemyAct> ParseEnemyAct(string text)
    {
        var list = new List<EnemyAct>();

        if (string.IsNullOrWhiteSpace(text))
            return list;

        string[] lines = text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string line = lines[i].TrimEnd('\r');
            string[] t = line.Split(',');

            if (!TryParseInt(t, 0, out int index))
                continue;

            try
            {
                var act = new EnemyAct
                {
                    index = index,

                    targetType = (TargetType)ParseInt(t, 1),
                    targetNum = ParseInt(t, 2),

                    target_front = ParseBool(t, 3),
                    target_center = ParseBool(t, 4),
                    target_back = ParseBool(t, 5),

                    atk_buff = ParseInt(t, 6),
                    def_buff = ParseInt(t, 7),
                    buff_time = ParseInt(t, 8),
                    block = ParseBool(t, 9),
                    stun = ParseInt(t, 10)
                };

                list.Add(act);
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"[EnemyActCSVParser/V1] {i + 1}번째 줄 파싱 오류: {e.Message}"
                );
            }
        }

        Debug.Log($"[EnemyActCSVParser/V1] EnemyAct {list.Count}개 로드 완료");
        return list;
    }


    // =========================================================
    // V2 Final Parser
    //
    // 0  index
    // 1  target_type
    // 2  target_num
    // 3  target_front
    // 4  target_center
    // 5  target_back
    //
    // 6  arg_effect1
    // 7  arg1
    // 8  arg_target1
    //
    // 9  arg_effect2
    // 10 arg2
    // 11 arg_target2
    //
    // 12 use_condition
    // 13 use_condition_value
    //
    // 14 value_modifier
    // 15 modifier_status
    // 16 modifier_value
    // 17 modifier_condition_value
    //
    // 18 special_logic
    // =========================================================

    public static List<EnemyAct> ParseEnemyActV2(string text)
    {
        var list = new List<EnemyAct>();

        if (string.IsNullOrWhiteSpace(text))
            return list;

        string[] lines = text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string line = lines[i].TrimEnd('\r');
            string[] t = line.Split(',');

            if (!TryParseInt(t, 0, out int index))
                continue;

            try
            {
                var act = new EnemyAct
                {
                    index = index,

                    targetType = (TargetType)ParseInt(t, 1),
                    targetNum = ParseInt(t, 2),

                    target_front = ParseBool(t, 3),
                    target_center = ParseBool(t, 4),
                    target_back = ParseBool(t, 5),

                    // Effect 1
                    arg_effect1 = ParseEnum(t, 6, EnemyEffectType.None),

                    arg1 = ParseInt(t, 7),

                    arg_target1 = ParseEnum(t, 8, EnemyEffectTarget.None),

                    // Effect 2
                    arg_effect2 = ParseEnum(t, 9, EnemyEffectType.None),

                    arg2 = ParseInt(t, 10),

                    arg_target2 = ParseEnum(t, 11, EnemyEffectTarget.None),

                    // 스킬 사용 조건
                    useCondition = ParseEnum(t, 12, EnemyUseCondition.None),

                    useConditionValue = ParseInt(t, 13),

                    // 조건부 수치 계산
                    valueModifier = ParseEnum(t, 14, EnemyValueModifier.None),

                    modifierStatus = ParseEnum(t, 15, EnemyEffectType.None),

                    modifierValue = ParseInt(t, 16),

                    modifierConditionValue = ParseInt(t, 17),

                    // 특수 기믹
                    specialLogic = ParseEnum(
                        t, 18, EnemySpecialLogic.None)
                };

                list.Add(act);
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"[EnemyActCSVParser/V2] {i + 1}번째 줄 파싱 오류: {e.Message}"
                );
            }
        }

        Debug.Log($"[EnemyActCSVParser/V2] EnemyAct {list.Count}개 로드 완료");
        return list;
    }


    // =========================================================
    // Common
    // =========================================================

    private static int ParseInt(string[] tokens, int index)
    {
        if (index >= tokens.Length)
            return 0;

        string value = tokens[index].Trim();

        if (string.IsNullOrWhiteSpace(value))
            return 0;

        return int.TryParse(value, out int result)
            ? result
            : 0;
    }


    private static bool TryParseInt(
        string[] tokens,
        int index,
        out int result)
    {
        result = 0;

        if (index >= tokens.Length)
            return false;

        string value = tokens[index].Trim();
        return int.TryParse(value, out result);
    }


    private static bool ParseBool(string[] tokens, int index)
    {
        if (index >= tokens.Length)
            return false;

        string value = tokens[index].Trim();

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value == "1")
            return true;

        if (value == "0")
            return false;

        return bool.TryParse(value, out bool result) && result;
    }


    private static T ParseEnum<T>(
        string[] tokens,
        int index,
        T defaultValue)
        where T : struct
    {
        if (index >= tokens.Length)
            return defaultValue;

        string value = tokens[index].Trim();

        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        if (Enum.TryParse(value, true, out T result))
            return result;

        Debug.LogWarning(
            $"[EnemyActCSVParser] {typeof(T).Name} 변환 실패: '{value}'"
        );

        return defaultValue;
    }
}