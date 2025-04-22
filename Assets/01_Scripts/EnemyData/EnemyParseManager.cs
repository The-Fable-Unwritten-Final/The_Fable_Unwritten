using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 한 번만 CSV를 파싱해서 ID → EnemyParsed 딕셔너리로 보관
/// EnemyPattern에서는 이 매니저만 참조함.
/// </summary>
public static class EnemyParseManager
{
    // 외부에서 읽기 전용으로 접근 = ParsedDict
    public static readonly Dictionary<int, EnemyParsed> ParsedDict;


    static EnemyParseManager()
    {
        // 실제 프로젝트 내 CSV 경로로 수정필요
        string csvPath = Path.Combine(Application.dataPath, "Resources/ExternalFiles/Enemy.csv");

        // 기존 EnemyCSVParser.Parse()를 그대로 사용
        var list = EnemyCSVParser.Parse(csvPath);

        ParsedDict = new Dictionary<int, EnemyParsed>(list.Count);
        foreach (var p in list)
            ParsedDict[p.id] = p;

        Debug.Log($"[EnemyParseManager] CSV 로드 완료: {ParsedDict.Count}개 엔트리");
    }
}
