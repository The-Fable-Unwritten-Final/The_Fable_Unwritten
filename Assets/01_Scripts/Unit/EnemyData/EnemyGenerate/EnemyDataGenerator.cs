#if UNITY_EDITOR
using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EnemyParsed를 EnemyData 형식으로 변환하여 ScriptableObjects 목록에 저장하는 클래스
/// </summary>
public static class EnemyDataGenerator
{
    private const string EnemySavePath = "Assets/05._ScriptableObjects/Enemy/";
    private const string ContainerPath = "Assets/05._ScriptableObjects/Enemy/EnemyContainer/EnemyDataContainer.asset";

    /// <summary>
    /// parsed 된 데이털f enemydata 형식으로 변환하여 저장
    /// </summary>
    /// <param name="parsedList"></param>
    public static void GenerateFromParsed(List<EnemyParsed> parsedList)
    {
        if (!Directory.Exists(EnemySavePath))
            Directory.CreateDirectory(EnemySavePath);

        foreach (var parsed in parsedList)
        {
            string fileName = $"{parsed.id}_{parsed.enemyName}.asset";
            string fullPath = Path.Combine(EnemySavePath, fileName);

            EnemyData existing = AssetDatabase.LoadAssetAtPath<EnemyData>(fullPath);

            // Dictionary 형태로 스킬 구성하여 임시 저장
            List<EnemySkill> parsedSkills = new();
            AddSkillIfValid(parsedSkills, parsed.skillIndices[0], parsed.skillDamages[0], parsed.skillPercents[0]);
            AddSkillIfValid(parsedSkills, parsed.skillIndices[1], parsed.skillDamages[1], parsed.skillPercents[1]);
            AddSkillIfValid(parsedSkills, parsed.skillIndices[2], parsed.skillDamages[2], parsed.skillPercents[2]);
            AddSkillIfValid(parsedSkills, parsed.skillIndices[3], parsed.skillDamages[3], parsed.skillPercents[3]);
            AddSkillIfValid(parsedSkills, parsed.skillIndices[4], parsed.skillDamages[4], parsed.skillPercents[4]);

            bool needCreate = existing == null || !IsSame(existing, parsed, parsedSkills);

            if (needCreate)
            {
                EnemyData data = existing != null ? existing : ScriptableObject.CreateInstance<EnemyData>();

                data.IDNum = parsed.id;
                data.EnemyName = parsed.enemyName;
                data.MaxHP = parsed.hp;
                data.CurrentHP = parsed.hp;
                data.illust = parsed.art; // 필드가 private면 public 프로퍼티 사용
                data.exp = parsed.exp;

                data.loot.Clear();
                foreach(var i in parsed.loots)
                {
                    data.loot.Add(i);
                }

                data.AttackSkillEffect = parsed.attackEffect; // 정확한 필드명과 연결 확인
                data.AllySkillEffect = parsed.allyEffect;

                // 스킬 설정
                data.ClearSkills();
                foreach (var skill in parsedSkills)
                {
                    data.AddSkill(skill);
                }

                data.TopStance = parsed.topPercentage;
                data.MiddleStance = parsed.middlePercentage;
                data.BottomStance = parsed.bottomPercentage;

                data.ATKValue = parsed.atkBuff; // 첫 스킬 데미지
                data.DEFValue = parsed.defBuff;

                data.blind = parsed.blind;
                data.stun = parsed.stun;
                data.block = parsed.block;

                data.note = parsed.note;
                data.type = (EnemyType)parsed.type;

                data.currentStance = StancValue.EStancType.Middle;
                data.animationController = FindAnimatorController(parsed.art);

                if (existing == null)
                {
                    AssetDatabase.CreateAsset(data, fullPath);
                }
                else
                {
                    EditorUtility.SetDirty(data);
                }
            }

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UpdateEnemyContainer();
    }


    /// <summary>
    /// 스킬화 하여 dict에 저장
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="index"></param>
    /// <param name="damage"></param>
    /// <param name="percentage"></param>
    private static void AddSkillIfValid(List<EnemySkill> list, int index, float damage, float percentage)
    {
        if (index == 0) return;

        list.Add(new EnemySkill
        {
            skillIndex = index,
            damage = damage,
            percentage = percentage
        });
    }

    /// <summary>
    /// 적 데이터 및 스킬 데이터의 변화가 있는지 확인하는 함수
    /// </summary>
    /// <param name="data">이미 저장되어 있는 적 데이터</param>
    /// <param name="parsed">가져온 데이터</param>
    /// <param name="parsedSkills">가져온 스킬 데이터</param>
    /// <returns></returns>
    private static bool IsSame(EnemyData data, EnemyParsed parsed, List<EnemySkill> parsedSkills)
    {
        if (data.IDNum != parsed.id) return false;
        if (data.EnemyName != parsed.enemyName) return false;
        if (data.MaxHP != parsed.hp) return false;
        if (data.ATKValue != parsed.atkBuff) return false;
        if (!Mathf.Approximately(data.DEFValue, parsed.defBuff)) return false;

        List<EnemySkill> existingSkills = data.SkillList;
        if (existingSkills == null || existingSkills.Count != parsedSkills.Count)
            return false;

        foreach (var skill in parsedSkills)
        {
            var match = existingSkills.Find(s => s.skillIndex == skill.skillIndex);
            if (match == null) return false;

            if (!Mathf.Approximately(match.damage, skill.damage)) return false;
            if (!Mathf.Approximately(match.percentage, skill.percentage)) return false;
        }

        return true;
    }

    public static void UpdateEnemyContainer()
    {
        var guids = AssetDatabase.FindAssets("t:EnemyData", new[] { EnemySavePath });
        var allEnemies = guids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<EnemyData>(path))
            .Where(e => e != null)
            .ToList();

        EnemyDataContainer container = AssetDatabase.LoadAssetAtPath<EnemyDataContainer>(ContainerPath);
        if (container == null)
        {
            container = ScriptableObject.CreateInstance<EnemyDataContainer>();
            AssetDatabase.CreateAsset(container, ContainerPath);
            Debug.Log("[EnemyDataContainer] 새로 생성됨");
        }

        var so = new SerializedObject(container);
        var prop = so.FindProperty("enemyDataList");
        prop.ClearArray();
        for (int i = 0; i < allEnemies.Count; i++)
        {
            prop.InsertArrayElementAtIndex(i);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = allEnemies[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(container);
        AssetDatabase.SaveAssets();
    }

    private static RuntimeAnimatorController FindAnimatorController(string illustName)
    {
        if (string.IsNullOrEmpty(illustName)) return null;

        string[] guids = AssetDatabase.FindAssets($"{illustName} t:AnimatorController", new[] { "Assets/04_Animation/Enemy" });

        if (guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
    }
}
#endif