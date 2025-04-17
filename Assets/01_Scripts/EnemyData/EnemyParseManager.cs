using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// CSV로부터 적 데이터를 파싱해 ID별로 보관하는 매니저 클래스.
/// 한 번만 초기화하며, 이후에는 ParsedDict에서 바로 꺼내 쓸 수 있게 만들었음
/// </summary>
public static class EnemyParseManager
{

    public static Dictionary<int, EnemyParsed> ParsedDict { get; private set; }

    private static bool initialized = false;

    /// <summary>
    /// 지정된 CSV 경로를 파싱해 ParsedDict를 초기화
    /// 중복 호출해도 한 번만 실행됨
    /// </summary>
    /// <param name="csvPath">파싱할 CSV 파일의 경로</param>
    public static void Initialize(string csvPath)
    {
        if (initialized) return;
        var list = EnemyCSVParser.Parse(csvPath); //EnemyCSVParser 파싱
        ParsedDict = list.ToDictionary(p => p.id, p => p);
        initialized = true;
        Debug.Log($"[EnemyParseManager] {ParsedDict.Count}개의 적 데이터를 로드했습니다.");
    }
}
