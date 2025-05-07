using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageSetttingController
{
    //Resources
    //private List<EnemyStageSpawnData> enemySpawnData;  // StageSpawnData 파싱 리스트
    //private List<RandomEventData> allRandomEvents;     // RandomEvent JSon 파싱 리스트
    //private Dictionary<int, Sprite> stageBackgrounds;  // StageBackground Image 파싱 리스트

    //Data
    //private HashSet<int> usedRandomEvnent = new();     // RandomEvent 진행 유무(게임 재시작 및 실패 시 초기화 - ClearUsedEvents())
    //private Dictionary<int, StageTheme> stageThemes = new(); // 2~4 스테이지용 테마
    //private HashSet<StageTheme> eliteClearThemes = new(); // Theme 별 Elite Clear 리스트
    

    //public int StageIndex { get; set; }                // 현재 스테이지
    //public int MinStageIndex { get; set; }             // 재시작 스테이지 (2스테이지 클리어시 2)
    //public bool RetryFromStart { get; set; }           // 스테이지 실패시 재시작여부
    //public bool StageCleared { get; set; }             // 전투 승리 여부
    //public GraphNode CurrentBattleNode { get; set; }   // 현재 선택한 노드
    //public StageData SavedStageData { get; private set; }               // 현재 진행 중인 스테이지 데이터
    //public List<GraphNode> VisitedNodes { get; private set; } = new();  // 플레이어가 진행한 노드 리스트
    //public StageTheme CurrentTheme { get; private set; }



    public void Initialize()
    {
        //enemySpawnData = StageSpawnSetCSVParser.LoadFromCSV() ?? new();
        //allRandomEvents = RandomEventJsonLoader.LoadAllEvents() ?? new();
        //stageBackgrounds = BackgoundLoader.LoadBackgrounds() ?? new();



        // 1스테이지 부터 시작 (기획자 요청으로 임시 2스테이지 부터 시작)
        //StageIndex = Mathf.Max(1, StageIndex);
        //MinStageIndex = Mathf.Max(1, MinStageIndex);
        //AssignTemesToStages(); // 테마 스테이지별로 배정
    }

    // Resources
    /// <summary>
    /// 특정 스테이지 및 노드타입에 해당하는 적 배치 데이터 리스트 반환
    /// </summary>

    //public List<EnemyStageSpawnData> GetEnemySpawnData(StageTheme theme, NodeType type)
    //{
    //    var result = enemySpawnData
    //    .Where(x => x.theme == theme && x.type == type)
    //    .ToList();

    //    Debug.Log($"[StageSettingController] 스폰 데이터 검색: Theme={theme}, Type={type}, 결과={result.Count}개");

    //    return result;
    //}


    // data
    /// <summary>
    /// 특정 스테이지에서 아직 등장하지 않은 랜덤 이벤트 중 하나를 반환하고, 사용
    /// </summary>
    //public RandomEventData GetRandomEvent(StageTheme theme)
    //{
    //    var available = allRandomEvents
    //        .Where(x => x.theme == theme && !usedRandomEvnent.Contains(x.index))
    //        .ToList();

    //    if (available.Count == 0) return null;

    //    var selected = available[UnityEngine.Random.Range(0, available.Count)];
    //    usedRandomEvnent.Add(selected.index);
    //    return selected;
    //}


    ///// <summary>
    ///// 특정 스테이지에 해당하는 배경 이미지 반환
    ///// </summary>
    //public Sprite GetBackground(int stageIndex)
    //{
    //    return stageBackgrounds.TryGetValue(stageIndex, out var sprite) ? sprite : null;
    //}


    ///// <summary>
    ///// 스테이지 상태 저장 (맵 데이터 및 방문 노드)
    ///// </summary>
    //public void SaveStageState(StageData data, List<GraphNode> visited)
    //{
    //    SavedStageData = data;
    //    VisitedNodes = new List<GraphNode>(visited);
    //}


    /// <summary>
    /// 스테이지 상태 초기화 (새 시작 등)
    /// </summary>
    //public void ClearStageState()
    //{
    //    //새로하기 기능 있으면 재시작 시테이지도 초기화 시켜줘야함
    //    SavedStageData = null;
    //    VisitedNodes.Clear();
    //}

    /// <summary>
    /// 사용된 랜덤 이벤트 인덱스 초기화
    /// </summary>
    //public void ClearUsedEvents()
    //{
    //    usedRandomEvnent.Clear();
    //}

    /// <summary>
    /// 현재 전투 노드 설정
    /// </summary>
    //public void SetCurrentBattleNode(GraphNode node)
    //{
    //    CurrentBattleNode = node;
    //}


    // data
    //private void AssignTemesToStages()
    //{
    //    stageThemes[2] = StageTheme.Wisdom;
    //    stageThemes[3] = StageTheme.Love;
    //    stageThemes[4] = StageTheme.Courage;

    //    // 랜덤한 순서로 진행 되는 로직, 타 태마도 완성 시에 위 로직 제거 후 주석 해제
    //    //List<StageTheme> themePool = new() { StageTheme.Wisdom, StageTheme.Love, StageTheme.Courage };
    //    //var shuffled = themePool.OrderBy(x => Random.value).ToList();

    //    //for (int i = 0; i < themePool.Count; i++)
    //    //{
    //    //    stageThemes[2 + i] = shuffled[i];
    //    //}
    //}

    //public StageTheme GetThemeForStage(int stageIndex)
    //{
    //    return stageThemes.TryGetValue(stageIndex, out var theme) ? theme : StageTheme.Tutorial;
    //}

    //public void SetTheme(StageTheme theme)
    //{
    //    CurrentTheme = theme;
    //}

    ////data
    //public void EliteClear(StageTheme theme)
    //{
    //    eliteClearThemes.Add(theme);
    //}

    ////data
    //public bool IsEliteClear(StageTheme theme)
    //{
    //    return eliteClearThemes.Contains(theme);
    //}
}
