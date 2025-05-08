using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// EnemyAct.csv를 기반으 EnemySkillData ScriptableObject를 생성하는 통합 실행 클래스
/// </summary>
public static class EnemySkillInitializer
{
    private const string CsvPath = "Assets/Resources/ExternalFiles/EnemyAct.csv";

    public static void ImportAndGenerate()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"[EnemySkillInitializer] CSV 파일이 존재하지 않습니다: {CsvPath}");
            return;
        }

        List<EnemyAct> parsedList = EnemyActCSVParser.ParseEnemyAct(CsvPath);
        if (parsedList == null || parsedList.Count == 0)
        {
            Debug.LogWarning("[EnemySkillInitializer] 파싱된 스킬 데이터가 없습니다.");
            return;
        }

        EnemySkillDataGenerator.GenerateFromParsed(parsedList);
        Debug.Log($"[EnemySkillInitializer] 총 {parsedList.Count}개의 EnemySkillData 처리 완료.");
    }
}
