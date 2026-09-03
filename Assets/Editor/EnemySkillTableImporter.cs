#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class EnemySkillTableImporter
{
    private class EnemySkillTableRow
    {
        public int index;
        public string skillName;
        public string monster;
        public string skillDesc;

        public int targetType;
        public int targetNum;

        public bool targetFront;
        public bool targetCenter;
        public bool targetBack;

        public int arg1;
        public int arg2;

        public string effect;
    }

    private struct ParsedEffect
    {
        public EnemyEffectType type;
        public EnemyEffectTarget target;
        public int value;

        public ParsedEffect(
            EnemyEffectType type,
            EnemyEffectTarget target,
            int value)
        {
            this.type = type;
            this.target = target;
            this.value = value;
        }
    }

    public static void Import()
    {
        string path = EditorUtility.OpenFilePanel(
            "Enemy Skill Table CSV 선택",
            "",
            "csv"
        );

        if (string.IsNullOrEmpty(path))
            return;

        List<EnemySkillTableRow> rows = ParseTable(path);

        if (rows.Count == 0)
        {
            Debug.LogError(
                "[EnemySkillTableImporter] 읽어온 Enemy Skill이 없습니다."
            );

            return;
        }

        if (!EditorUtility.DisplayDialog(
            "Enemy Skill Table 반영",
            $"{rows.Count}개의 Enemy Skill 데이터를 EnemyActV2로 변환합니다.\n계속하시겠습니까?",
            "반영",
            "취소"))
        {
            return;
        }

        List<EnemyAct> acts = new();

        foreach (EnemySkillTableRow row in rows)
        {
            EnemyAct act = BuildAct(row);
            acts.Add(act);
        }

        Save(acts);
    }

    private static List<EnemySkillTableRow> ParseTable(string path)
    {
        List<EnemySkillTableRow> result = new();

        string[] lines = File.ReadAllLines(path);

        if (lines.Length <= 4)
            return result;

        // 0 : 설명
        // 1 : 빈 줄/메타
        // 2 : 타입
        // 3 : 변수명
        // 4부터 실제 데이터
        for (int i = 3; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            List<string> t = ParseCsvLine(lines[i]);

            if (!int.TryParse(Get(t, 0), out int index))
                continue;

            EnemySkillTableRow row = new EnemySkillTableRow();

            row.index = index;
            row.skillName = Get(t, 1);
            row.monster = Get(t, 2);
            row.skillDesc = Get(t, 3);

            row.targetType = ParseInt(t, 4);
            row.targetNum = ParseInt(t, 5);

            row.targetFront = ParseBool(t, 6);
            row.targetCenter = ParseBool(t, 7);
            row.targetBack = ParseBool(t, 8);

            row.arg1 = ParseInt(t, 9);
            row.arg2 = ParseInt(t, 10);

            // 기존 테이블의 effect 컬럼
            row.effect = Get(t, 11);

            result.Add(row);
        }

        return result;
    }

    private static EnemyAct BuildAct(EnemySkillTableRow row)
    {
        EnemyAct act = new EnemyAct
        {
            index = row.index,

            targetType = (TargetType)row.targetType,
            targetNum = row.targetNum,

            target_front = row.targetFront,
            target_center = row.targetCenter,
            target_back = row.targetBack,

            arg_effect1 = EnemyEffectType.None,
            arg1 = 0,
            arg_target1 = EnemyEffectTarget.None,

            arg_effect2 = EnemyEffectType.None,
            arg2 = 0,
            arg_target2 = EnemyEffectTarget.None,

            useCondition = EnemyUseCondition.None,
            useConditionValue = 0,

            valueModifier = EnemyValueModifier.None,
            modifierStatus = EnemyEffectType.None,
            modifierValue = 0,
            modifierConditionValue = 0,

            specialLogic = EnemySpecialLogic.None,

            skilleffect = row.effect
        };

        ResolveGeneralEffects(act, row);
        ResolveCondition(act, row);
        ResolveModifier(act, row);
        ResolveSpecialLogic(act, row);
        ValidateAct(act, row);

        return act;
    }

    // =========================================================
    // 일반 효과
    // =========================================================

    private static void ResolveGeneralEffects(
        EnemyAct act,
        EnemySkillTableRow row)
    {
        List<ParsedEffect> effects =
            ParseEffects(row.skillDesc, row.targetType);

        if (effects.Count > 0)
        {
            act.arg_effect1 = effects[0].type;
            act.arg_target1 = effects[0].target;

            act.arg1 = ResolveSlotValue(
                effects[0].type,
                row.arg1,
                effects[0].value
            );
        }

        if (effects.Count > 1)
        {
            act.arg_effect2 = effects[1].type;
            act.arg_target2 = effects[1].target;

            act.arg2 = ResolveSlotValue(
                effects[1].type,
                row.arg2,
                effects[1].value
            );
        }
    }

    private static List<ParsedEffect> ParseEffects(
        string desc,
        int targetType)
    {
        List<ParsedEffect> result = new();

        if (string.IsNullOrWhiteSpace(desc))
            return result;

        string[] clauses = Regex.Split(desc, @"[,\.]");

        EnemyEffectTarget currentTarget =
            targetType == 0
                ? EnemyEffectTarget.None
                : EnemyEffectTarget.SkillTarget;

        foreach (string raw in clauses)
        {
            string clause = raw.Trim();

            if (string.IsNullOrEmpty(clause))
                continue;

            if (TryResolveExplicitTarget(
                clause,
                out EnemyEffectTarget explicitTarget))
            {
                currentTarget = explicitTarget;
            }

            EnemyEffectTarget target = currentTarget;

            // "피해 반사"를 일반 피해보다 먼저 확인
            if (TryAddSignedEffect(
                result,
                clause,
                "피해 반사",
                EnemyEffectType.Reflect,
                target))
            {
                continue;
            }

            if (clause.Contains("상태이상 면역"))
            {
                result.Add(
                    new ParsedEffect(
                        EnemyEffectType.Resist,
                        target,
                        1
                    )
                );

                continue;
            }

            if (clause.Contains("해로운 효과") &&
               clause.Contains("제거"))
            {
                result.Add(
                    new ParsedEffect(
                        EnemyEffectType.CleanseDebuff,
                        target,
                        1
                    )
                );

                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "공격력",
                EnemyEffectType.Attack,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "방어력",
                EnemyEffectType.Defense,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "화상",
                EnemyEffectType.Burn,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "빙결",
                EnemyEffectType.Freeze,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "활성",
                EnemyEffectType.Activate,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "축복",
                EnemyEffectType.Bless,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "죄악",
                EnemyEffectType.Crime,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "참회",
                EnemyEffectType.Penance,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "상처",
                EnemyEffectType.Scar,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "기절",
                EnemyEffectType.Stun,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "수호",
                EnemyEffectType.Guard,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "포텐셜 게이지",
                EnemyEffectType.Potential,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "지목",
                EnemyEffectType.Mark,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "강습 준비",
                EnemyEffectType.AssaultReady,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "방패전술",
                EnemyEffectType.ShieldTactic,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "연격",
                EnemyEffectType.Combo,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "표식",
                EnemyEffectType.TargetMark,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "대열",
                EnemyEffectType.Formation,
                target))
            {
                continue;
            }

            if (TryAddSignedEffect(
                result,
                clause,
                "합",
                EnemyEffectType.Hap,
                target))
            {
                continue;
            }

            if (clause.Contains("체력"))
            {
                if (TryAddSignedEffect(
                    result,
                    clause,
                    "체력",
                    EnemyEffectType.Heal,
                    target))
                {
                    continue;
                }
            }

            // 반드시 마지막
            if (TryAddDamage(
                result,
                clause,
                target))
            {
                continue;
            }
        }

        return result;
    }

    private static bool TryResolveExplicitTarget(
        string clause,
        out EnemyEffectTarget target)
    {
        if (clause.Contains("체력이 가장 낮은") ||
           clause.Contains("체력이 가장 적은"))
        {
            target = EnemyEffectTarget.LowestHpAlly;
            return true;
        }

        if (clause.Contains("자신에게") ||
           clause.Contains("자신의"))
        {
            target = EnemyEffectTarget.Self;
            return true;
        }

        if (clause.Contains("아군 전체"))
        {
            target = EnemyEffectTarget.Allies;
            return true;
        }

        if (clause.Contains("적 ") ||
           clause.Contains("적 전체") ||
           clause.Contains("대상에게") ||
           clause.Contains("대면 대상"))
        {
            target = EnemyEffectTarget.SkillTarget;
            return true;
        }

        target = EnemyEffectTarget.None;
        return false;
    }

    private static bool TryAddDamage(
        List<ParsedEffect> result,
        string clause,
        EnemyEffectTarget target)
    {
        int keywordIndex =
            clause.IndexOf("피해", StringComparison.Ordinal);

        if (keywordIndex < 0)
            return false;

        string after =
            clause.Substring(keywordIndex + "피해".Length);

        Match match = Regex.Match(after, @"-?\s*\d+");

        if (!match.Success)
            return false;

        string valueString =
            match.Value.Replace(" ", "");

        if (!int.TryParse(valueString, out int value))
            return false;

        result.Add(
            new ParsedEffect(
                EnemyEffectType.Damage,
                target,
                Mathf.Abs(value)
            )
        );

        return true;
    }

    private static bool TryAddSignedEffect(
        List<ParsedEffect> result,
        string clause,
        string keyword,
        EnemyEffectType type,
        EnemyEffectTarget target)
    {
        int keywordIndex =
            clause.IndexOf(keyword, StringComparison.Ordinal);

        if (keywordIndex < 0)
            return false;

        string after =
            clause.Substring(keywordIndex + keyword.Length);

        Match match =
            Regex.Match(after, @"[+-]?\s*\d+");

        if (!match.Success)
            return false;

        string valueString =
            match.Value.Replace(" ", "");

        if (!int.TryParse(valueString, out int value))
            return false;

        result.Add(
            new ParsedEffect(
                type,
                target,
                value
            )
        );

        return true;
    }

    private static int ResolveSlotValue(
        EnemyEffectType type,
        int tableValue,
        int descriptionValue)
    {
        int value =
            tableValue != 0
                ? tableValue
                : descriptionValue;

        switch (type)
        {
            case EnemyEffectType.Heal:
            case EnemyEffectType.Damage:

            case EnemyEffectType.Burn:
            case EnemyEffectType.Freeze:
            case EnemyEffectType.Activate:

            case EnemyEffectType.Bless:
            case EnemyEffectType.Crime:
            case EnemyEffectType.Penance:

            case EnemyEffectType.Scar:
            case EnemyEffectType.Stun:
            case EnemyEffectType.Guard:

            case EnemyEffectType.Block:
            case EnemyEffectType.Resist:
            case EnemyEffectType.CleanseDebuff:
            case EnemyEffectType.Reflect:

            case EnemyEffectType.Mark:
            case EnemyEffectType.AssaultReady:
            case EnemyEffectType.ShieldTactic:
            case EnemyEffectType.Combo:
            case EnemyEffectType.TargetMark:
            case EnemyEffectType.Formation:
            case EnemyEffectType.Hap:
                return Mathf.Abs(value);

            // Attack/Defense/Potential은 +/- 자체가 의미 있음
            case EnemyEffectType.Attack:
            case EnemyEffectType.Defense:
            case EnemyEffectType.Potential:
                return value;

            default:
                return value;
        }
    }

    // =========================================================
    // 사용 조건
    // =========================================================

    private static void ResolveCondition(
        EnemyAct act,
        EnemySkillTableRow row)
    {
        string desc = row.skillDesc;

        if (string.IsNullOrWhiteSpace(desc))
            return;

        Match match = Regex.Match(
            desc,
            @"체력\s*(\d+)%\s*이하.*(?:사용|때만)"
        );

        if (!match.Success)
            return;

        act.useCondition =
            EnemyUseCondition.SelfHpBelow;

        act.useConditionValue =
            int.Parse(match.Groups[1].Value);
    }

    // =========================================================
    // 수치 Modifier
    // =========================================================

    private static void ResolveModifier(
        EnemyAct act,
        EnemySkillTableRow row)
    {
        string desc = row.skillDesc;

        if (string.IsNullOrWhiteSpace(desc))
            return;

        // 101
        // 피격 대상이 상처 상태일 경우 해당 수치만큼 추가 피해
        if (desc.Contains("상처") &&
           desc.Contains("해당 수치만큼 추가 피해"))
        {
            act.valueModifier =
                EnemyValueModifier.AddTargetStatusValue;

            act.modifierStatus =
                EnemyEffectType.Scar;

            return;
        }

        // 합 x N
        Match selfStatusMultiply = Regex.Match(
            desc,
            @"합\s*[xX×]\s*(\d+)"
        );

        if (selfStatusMultiply.Success)
        {
            act.valueModifier =
                EnemyValueModifier.AddSelfStatusValueMultiply;

            act.modifierStatus =
                EnemyEffectType.Hap;

            act.modifierValue =
                int.Parse(
                    selfStatusMultiply.Groups[1].Value
                );

            return;
        }

        // HP 50% 이하일 경우 피해 +3
        Match hpBonus = Regex.Match(
            desc,
            @"체력\s*(\d+)%\s*이하.*피해\s*\+\s*(\d+)"
        );

        if (hpBonus.Success)
        {
            act.valueModifier =
                EnemyValueModifier.AddValueIfSelfHpBelow;

            act.modifierConditionValue =
                int.Parse(hpBonus.Groups[1].Value);

            act.modifierValue =
                int.Parse(hpBonus.Groups[2].Value);
        }
    }

    // =========================================================
    // 전용 기믹
    // =========================================================

    private static void ResolveSpecialLogic(
        EnemyAct act,
        EnemySkillTableRow row)
    {
        switch (row.index)
        {
            case 322:
                act.specialLogic =
                    EnemySpecialLogic.PotentialIfBoss;
                break;

            case 421:
                act.specialLogic =
                    EnemySpecialLogic.CommanderMarkedStrike;

                // 기본값은 지목이 없을 때 피해 8
                // 전용 로직에서 지목 대상이면 9 처리
                act.arg_effect1 =
                    EnemyEffectType.Damage;

                act.arg1 = 8;

                act.arg_target1 =
                    EnemyEffectTarget.SkillTarget;
                break;

            case 422:
                act.specialLogic =
                    EnemySpecialLogic.CommanderReorganize;
                break;

            case 423:
                act.specialLogic =
                    EnemySpecialLogic.CommanderCharge;
                break;

            case 433:
                // 현재 대면 대상에게 피해 10 + 합 x4
                act.arg_effect1 =
                    EnemyEffectType.Damage;

                act.arg1 = 10;

                act.arg_target1 =
                    EnemyEffectTarget.SkillTarget;

                act.valueModifier =
                    EnemyValueModifier.AddSelfStatusValueMultiply;

                act.modifierStatus =
                    EnemyEffectType.Hap;

                act.modifierValue = 4;

                act.arg_effect2 =
                    EnemyEffectType.None;

                act.arg2 = 0;

                act.arg_target2 =
                    EnemyEffectTarget.None;

                act.specialLogic =
                    EnemySpecialLogic.ConsumeHap;
                break;

            case 434:
                act.specialLogic =
                    EnemySpecialLogic.ForceNextIlsum;
                break;

            case 442:
                act.specialLogic =
                    EnemySpecialLogic.SamuraiSweep;
                break;

            case 462:
                act.specialLogic =
                    EnemySpecialLogic.AssaultReady;
                break;

            case 472:
                act.specialLogic =
                    EnemySpecialLogic.ShieldTactic;
                break;

            case 482:
                act.specialLogic =
                    EnemySpecialLogic.ComboStrike;
                break;

            case 492:
                act.specialLogic =
                    EnemySpecialLogic.ApplyTargetMarkWithoutTrigger;
                break;
        }
    }

    // =========================================================
    // 검증
    // =========================================================

    private static void ValidateAct(
        EnemyAct act,
        EnemySkillTableRow row)
    {
        if (row.arg1 != 0 &&
           act.arg_effect1 == EnemyEffectType.None)
        {
            Debug.LogWarning(
                $"[EnemySkillTableImporter] {row.index} - {row.skillName}: " +
                $"arg1={row.arg1}인데 Effect1을 판정하지 못했습니다.\n" +
                $"Desc: {row.skillDesc}"
            );
        }

        if (row.arg2 != 0 &&
           act.arg_effect2 == EnemyEffectType.None &&
           act.specialLogic == EnemySpecialLogic.None)
        {
            Debug.LogWarning(
                $"[EnemySkillTableImporter] {row.index} - {row.skillName}: " +
                $"arg2={row.arg2}인데 Effect2를 판정하지 못했습니다.\n" +
                $"Desc: {row.skillDesc}"
            );
        }

        if (act.arg_effect1 != EnemyEffectType.None &&
           act.arg_target1 == EnemyEffectTarget.None &&
           act.targetType != TargetType.None &&
           act.arg_effect1 != EnemyEffectType.Potential)
        {
            Debug.LogWarning(
                $"[EnemySkillTableImporter] {row.index} - {row.skillName}: " +
                $"Effect1 Target이 None입니다."
            );
        }
    }

    // =========================================================
    // EnemyActV2 저장
    // =========================================================

    private static void Save(List<EnemyAct> acts)
    {
        string[] guids =
            AssetDatabase.FindAssets("EnemyActV2 t:TextAsset");

        if (guids.Length == 0)
        {
            Debug.LogError(
                "[EnemySkillTableImporter] EnemyActV2.csv를 찾을 수 없습니다."
            );

            return;
        }

        string path =
            AssetDatabase.GUIDToAssetPath(guids[0]);

        acts.Sort((a, b) => a.index.CompareTo(b.index));

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            "index,target_type,target_num,target_front,target_center,target_back," +
            "arg_effect1,arg1,arg_target1,arg_effect2,arg2,arg_target2," +
            "use_condition,use_condition_value,value_modifier,modifier_status," +
            "modifier_value,modifier_condition_value,special_logic,effect"
        );

        foreach (EnemyAct act in acts)
            sb.AppendLine(BuildCsvLine(act));

        File.WriteAllText(
            path,
            sb.ToString(),
            new UTF8Encoding(false)
        );

        AssetDatabase.Refresh();

        Debug.Log(
            $"[EnemySkillTableImporter] EnemyActV2 변환 완료 - {acts.Count}개"
        );
    }

    private static string BuildCsvLine(EnemyAct act)
    {
        return string.Join(",",
            act.index,
            (int)act.targetType,
            act.targetNum,

            act.target_front ? 1 : 0,
            act.target_center ? 1 : 0,
            act.target_back ? 1 : 0,

            act.arg_effect1.ToString(),
            act.arg1,
            act.arg_target1.ToString(),

            act.arg_effect2.ToString(),
            act.arg2,
            act.arg_target2.ToString(),

            act.useCondition.ToString(),
            act.useConditionValue,

            act.valueModifier.ToString(),
            act.modifierStatus.ToString(),
            act.modifierValue,
            act.modifierConditionValue,

            act.specialLogic.ToString(),

            EscapeCsv(act.skilleffect)
        );
    }

    // =========================================================
    // CSV 공통
    // =========================================================

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(",") ||
           value.Contains("\"") ||
           value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        List<string> result = new();

        bool quoted = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (quoted &&
                   i + 1 < line.Length &&
                   line[i + 1] == '"')
                {
                    current += '"';
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
                result.Add(current);
                current = "";
                continue;
            }

            current += c;
        }

        result.Add(current);

        return result;
    }

    private static string Get(
        List<string> values,
        int index)
    {
        if (index < 0 || index >= values.Count)
            return "";

        return values[index].Trim();
    }

    private static int ParseInt(
        List<string> values,
        int index)
    {
        int.TryParse(
            Get(values, index),
            out int result
        );

        return result;
    }

    private static bool ParseBool(
        List<string> values,
        int index)
    {
        string value = Get(values, index);

        return value == "1" ||
               value.Equals(
                   "true",
                   StringComparison.OrdinalIgnoreCase
               );
    }
}
#endif