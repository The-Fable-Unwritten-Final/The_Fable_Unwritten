#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EnemyTableImporter
{
    private class EnemyTableRow
    {
        public int index;
        public string name;
        public int type;
        public string art;
        public float hp;

        public int[] skillIndexes = new int[5];
        public int[] skillPercents = new int[5];
    }

    public static void Import()
    {
        string path = EditorUtility.OpenFilePanel(
            "Enemy Table CSV 선택",
            "",
            "csv"
        );

        if (string.IsNullOrEmpty(path))
            return;

        List<EnemyTableRow> rows = Parse(path);

        if (rows.Count == 0)
        {
            Debug.LogError("[EnemyTableImporter] 읽어온 Enemy가 없습니다.");
            return;
        }

        if (!EditorUtility.DisplayDialog(
            "Enemy Table 반영",
            $"{rows.Count}개의 Enemy 데이터를 기존 EnemyData에 반영합니다.\n계속하시겠습니까?",
            "반영",
            "취소"))
        {
            return;
        }

        Apply(rows);
    }

    private static List<EnemyTableRow> Parse(string path)
    {
        List<EnemyTableRow> result = new();

        string[] lines = File.ReadAllLines(path);

        if (lines.Length <= 4)
            return result;

        for (int i = 3; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            List<string> t = ParseCsvLine(lines[i]);

            if (t.Count == 0)
                continue;

            if (!int.TryParse(Get(t, 0), out int index))
                continue;

            EnemyTableRow row = new EnemyTableRow();

            row.index = index;
            row.name = Get(t, 1);
            row.type = ParseInt(t, 2);
            row.art = Get(t, 3);
            row.hp = ParseFloat(t, 4);

            row.skillIndexes[0] = ParseInt(t, 5);
            row.skillPercents[0] = ParseInt(t, 6);

            row.skillIndexes[1] = ParseInt(t, 7);
            row.skillPercents[1] = ParseInt(t, 8);

            row.skillIndexes[2] = ParseInt(t, 9);
            row.skillPercents[2] = ParseInt(t, 10);

            row.skillIndexes[3] = ParseInt(t, 11);
            row.skillPercents[3] = ParseInt(t, 12);

            row.skillIndexes[4] = ParseInt(t, 13);
            row.skillPercents[4] = ParseInt(t, 14);

            result.Add(row);
        }

        return result;
    }

    private static void Apply(List<EnemyTableRow> rows)
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyData");

        Dictionary<int, EnemyData> enemies = new();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath);

            if (enemy == null)
                continue;

            enemies[enemy.IDNum] = enemy;
        }

        string enemyFolder = FindEnemyDataFolder();

        if (string.IsNullOrEmpty(enemyFolder))
        {
            Debug.LogError("[EnemyTableImporter] EnemyData 저장 폴더를 찾을 수 없습니다.");
            return;
        }

        int updated = 0;
        int created = 0;

        foreach (var row in rows)
        {
            EnemyData enemy;

            if (!enemies.TryGetValue(row.index, out enemy))
            {
                enemy = ScriptableObject.CreateInstance<EnemyData>();

                enemy.IDNum = row.index;

                string safeName = MakeSafeFileName(row.name);
                string assetPath =
                    $"{enemyFolder}/{row.index}_{safeName}.asset";

                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                AssetDatabase.CreateAsset(enemy, assetPath);

                enemies.Add(row.index, enemy);

                created++;

                Debug.Log(
                    $"[EnemyTableImporter] EnemyData 생성: {assetPath}"
                );
            }
            else
            {
                Undo.RecordObject(enemy, "Import Enemy Table");
                updated++;
            }

            enemy.EnemyName = row.name;
            enemy.type = (EnemyType)row.type;
            enemy.MaxHP = row.hp;
            enemy.illust = row.art;

            ApplySkills(enemy, row);

            EditorUtility.SetDirty(enemy);
        }

        AssetDatabase.SaveAssets();

        RefreshEnemyDataContainer();

        AssetDatabase.Refresh();

        Debug.Log(
            $"[EnemyTableImporter] 완료 - 갱신 {updated}, 생성 {created}"
        );
    }

    private static void ApplySkills(EnemyData enemy, EnemyTableRow row)
    {
        List<EnemySkill> skills = new();

        for (int i = 0; i < row.skillIndexes.Length; i++)
        {
            int skillIndex = row.skillIndexes[i];
            int percentage = row.skillPercents[i];

            if (skillIndex <= 0)
                continue;

            EnemySkill skill = new EnemySkill
            {
                skillIndex = skillIndex,
                percentage = percentage,
                damage = 0f
            };

            skills.Add(skill);
        }

        enemy.SkillList = skills;
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
                if (quoted && i + 1 < line.Length && line[i + 1] == '"')
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

    private static string Get(List<string> values, int index)
    {
        if (index < 0 || index >= values.Count)
            return "";

        return values[index].Trim();
    }

    private static int ParseInt(List<string> values, int index)
    {
        int.TryParse(Get(values, index), out int result);
        return result;
    }

    private static float ParseFloat(List<string> values, int index)
    {
        float.TryParse(Get(values, index), out float result);
        return result;
    }

    private static void RefreshEnemyDataContainer()
    {
        string[] containerGuids = AssetDatabase.FindAssets("t:EnemyDataContainer");

        if (containerGuids.Length == 0)
        {
            Debug.LogWarning("[EnemyTableImporter] EnemyDataContainer를 찾을 수 없습니다.");
            return;
        }

        string containerPath = AssetDatabase.GUIDToAssetPath(containerGuids[0]);
        EnemyDataContainer container =
            AssetDatabase.LoadAssetAtPath<EnemyDataContainer>(containerPath);

        if (container == null)
        {
            Debug.LogWarning("[EnemyTableImporter] EnemyDataContainer 로드 실패");
            return;
        }

        string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyData");

        List<EnemyData> enemies = new();

        foreach (string guid in enemyGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);

            if (enemy != null)
                enemies.Add(enemy);
        }

        enemies.Sort((a, b) => a.IDNum.CompareTo(b.IDNum));

        SerializedObject serializedObject = new SerializedObject(container);
        SerializedProperty listProperty =
            serializedObject.FindProperty("enemyDataList");

        if (listProperty == null)
        {
            Debug.LogError(
                "[EnemyTableImporter] EnemyDataContainer의 enemyDataList 필드를 찾을 수 없습니다."
            );
            return;
        }

        listProperty.ClearArray();

        for (int i = 0; i < enemies.Count; i++)
        {
            listProperty.InsertArrayElementAtIndex(i);

            SerializedProperty element =
                listProperty.GetArrayElementAtIndex(i);

            element.objectReferenceValue = enemies[i];
        }

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(container);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"[EnemyTableImporter] EnemyDataContainer 갱신 완료 - {enemies.Count}개"
        );
    }
    private static string FindEnemyDataFolder()
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyData");

        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        }

        const string fallback = "Assets/05_ScriptableObject/EnemyData";

        if (!AssetDatabase.IsValidFolder(fallback))
        {
            Debug.LogError(
                $"[EnemyTableImporter] EnemyData 폴더 없음: {fallback}"
            );

            return null;
        }

        return fallback;
    }

    private static string MakeSafeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Enemy";

        foreach (char c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');

        return fileName;
    }
}
#endif