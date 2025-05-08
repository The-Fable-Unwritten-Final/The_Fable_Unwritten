#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 적 CSV 파일을 파싱하고 ScriptableObject로 자동 생성하는 전체 흐름 관리 클래스
/// </summary>
public static class EnemyInitializer
{
    private const string CsvPath = "Assets/Resources/ExternalFiles/Enemy.csv";

    [MenuItem("Tools/Import/Generate EnemyData from CSV")]
    public static void ImportAndGenerate()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"[EnemyDataImporter] CSV 파일이 존재하지 않습니다: {CsvPath}");
            return;
        }

        List<EnemyParsed> parsedList = EnemyCSVParser.Parse(CsvPath);
        if (parsedList == null || parsedList.Count == 0)
        {
            Debug.LogWarning("[EnemyDataImporter] 파싱된 데이터가 없습니다.");
            return;
        }

        EnemyDataGenerator.GenerateFromParsed(parsedList);
    }
}
#endif
