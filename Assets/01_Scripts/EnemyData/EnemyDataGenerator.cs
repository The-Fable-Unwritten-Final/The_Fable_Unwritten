using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EnemyParsed를 EnemyData 형식으로 변환하여 ScriptableObjects 목록에 저장하는 클래스
/// </summary>
public static class EnemyDataGenerator
{
    private const string EnemySavePath = "Assets/05._ScriptableObjects/Enemy/";
    private const string ContainerPath = "Assets/02_Prefab/KSJ_Test/SO/EnemyDataContainer.asset";

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
            var parsedSkills = new Dictionary<int, EnemySkill>();
            AddSkillIfValid(parsedSkills, parsed.skill0, parsed.damage0, parsed.percentage0);
            AddSkillIfValid(parsedSkills, parsed.skill1, parsed.damage1, parsed.percentage1);
            AddSkillIfValid(parsedSkills, parsed.skill2, parsed.damage2, parsed.percentage2);

            bool needCreate = existing == null || !IsSame(existing, parsed, parsedSkills);

            if (needCreate)
            {
                EnemyData data = existing != null ? existing : ScriptableObject.CreateInstance<EnemyData>();

                data.IDNum = parsed.id;
                data.EnemyName = parsed.enemyName;
                data.MaxHP = parsed.hp;
                data.CurrentHP = parsed.hp;
                data.ATKValue = parsed.damage0;
                data.DEFValue = parsed.defBuff;
                data.currentStance = EnemyData.StancValue.EStancType.Middle;

                // 스킬 설정
                data.ClearSkills();
                foreach (var kvp in parsedSkills)
                {
                    data.AddSkill(kvp.Key, kvp.Value);
                }

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
    private static void AddSkillIfValid(Dictionary<int, EnemySkill> dict, int index, float damage, float percentage)
    {
        if (index == 0) return;

        dict[index] = new EnemySkill
        {
            skillIndex = index,
            damage = damage,
            percentage = percentage
        };
    }

    /// <summary>
    /// 적 데이터 및 스킬 데이터의 변화가 있는지 확인하는 함수
    /// </summary>
    /// <param name="data">이미 저장되어 있는 적 데이터</param>
    /// <param name="parsed">가져온 데이터</param>
    /// <param name="parsedSkills">가져온 스킬 데이터</param>
    /// <returns></returns>
    private static bool IsSame(EnemyData data, EnemyParsed parsed, Dictionary<int, EnemySkill> parsedSkills)
    {
        if (data.IDNum != parsed.id) return false;
        if (data.EnemyName != parsed.enemyName) return false;
        if (data.MaxHP != parsed.hp) return false;
        if (data.ATKValue != parsed.damage0) return false;
        if (!Mathf.Approximately(data.DEFValue, parsed.defBuff)) return false;

        var existingSkills = data.SkillDict;
        if (existingSkills == null || existingSkills.Count != parsedSkills.Count)
            return false;

        foreach (var kvp in parsedSkills)
        {
            if (!existingSkills.ContainsKey(kvp.Key)) return false;

            var a = existingSkills[kvp.Key];
            var b = kvp.Value;

            if (!Mathf.Approximately(a.damage, b.damage)) return false;
            if (!Mathf.Approximately(a.percentage, b.percentage)) return false;
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
        Debug.Log($"[EnemyDataContainer] 총 {allEnemies.Count}개 EnemyData 등록 완료");
    }
}
