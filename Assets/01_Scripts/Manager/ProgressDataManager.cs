using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProgressDataManager : MonoSingleton<ProgressDataManager>
{
    List<EventEffects> untillNextCombat = new List<EventEffects>(); // 다음 전투까지 지속되는 효과 리스트
    List<EventEffects> untillNextStage = new List<EventEffects>(); // 다음 스테이지까지 지속되는 효과 리스트
    List<EventEffects> untillEndAdventure = new List<EventEffects>(); // 모험이 끝날 때까지 지속되는 효과 리스트

    HashSet<int> usedRandomEvnent = new();     // RandomEvent 진행 유무(게임 재시작 및 실패 시 초기화 - ClearUsedEvents())
    Dictionary<int, StageTheme> stageThemes = new(); // 2~4 스테이지용 테마
    HashSet<StageTheme> eliteClearThemes = new(); // Theme 별 Elite Clear 리스트

    // 스테이지 방문 & 현재 노드
    public int StageIndex { get; set; }                // 현재 스테이지
    public int MinStageIndex { get; set; }             // 재시작 스테이지 (2스테이지 클리어시 2)
    public bool RetryFromStart { get; set; }           // 스테이지 실패시 재시작여부
    public bool StageCleared { get; set; }             // 전투 승리 여부
    public GraphNode CurrentBattleNode { get; set; }   // 현재 선택한 노드
    public StageData SavedStageData { get; private set; }               // 현재 진행 중인 스테이지 데이터
    public List<GraphNode> VisitedNodes { get; private set; } = new();  // 플레이어가 진행한 노드 리스트
    public StageTheme CurrentTheme { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        StageIndex = Mathf.Max(1, StageIndex);
        MinStageIndex = Mathf.Max(1, MinStageIndex);
        AssignTemesToStages();
    }

    private void Start()
    {
        LoadProgress();
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            SaveProgress();
        }
    }

    public void SaveProgress()
    {
        ProgressSaveData data = new ProgressSaveData();

        data.stageIndex = StageIndex;
        data.minStageIndex = MinStageIndex;
        data.retryFromStart = RetryFromStart;
        data.stageCleared = StageCleared;

        // CurrentBattleNode
        // SavedStageData
        // VisitedNodes
        data.currentTheme = (int)CurrentTheme;

        data.untilNextCombatEffects = untillNextCombat.Select(e => e.index).ToList();
        data.untilNextStageEffects = untillNextStage.Select(e => e.index).ToList();
        data.untilEndAdventureEffects = untillEndAdventure.Select(e => e.index).ToList();

        data.usedRandomEventIds = usedRandomEvnent.ToList();
        data.stageThemes = stageThemes.ToDictionary(pair => pair.Key, pair => (int)pair.Value);
        data.eliteClearThemes = eliteClearThemes.Select(e => (int)e).ToList();

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString("ProgressSaveData", json);
        PlayerPrefs.Save();

        Debug.Log("[ProgressDataManager] 진행 저장 완료");
    }

    public void LoadProgress()
    {
        if (!PlayerPrefs.HasKey("ProgressSaveData"))
        {
            Debug.LogWarning("[ProgressDataManager] 저장된 데이터가 없습니다.");
            return;
        }

        string json = PlayerPrefs.GetString("ProgressSaveData");
        ProgressSaveData data = JsonUtility.FromJson<ProgressSaveData>(json);

        StageIndex = data.stageIndex;
        MinStageIndex = data.minStageIndex;
        RetryFromStart = data.retryFromStart;
        StageCleared = data.stageCleared;

        // CurrentBattleNode
        // SavedStageData
        // VisitedNodes
        CurrentTheme = (StageTheme)data.currentTheme;

        untillNextCombat = data.untilNextCombatEffects
            .Select(index => EventEffectManager.Instance.eventEffectDict[index].Clone())
            .ToList();

        untillNextStage = data.untilNextStageEffects
            .Select(index => EventEffectManager.Instance.eventEffectDict[index].Clone())
            .ToList();

        untillEndAdventure = data.untilEndAdventureEffects
            .Select(index => EventEffectManager.Instance.eventEffectDict[index].Clone())
            .ToList();

        usedRandomEvnent = data.usedRandomEventIds.ToHashSet();
        stageThemes = data.stageThemes.ToDictionary(pair => pair.Key, pair => (StageTheme)pair.Value);
        eliteClearThemes = data.eliteClearThemes.Select(i => (StageTheme)i).ToHashSet();


        EventEffectManager.Instance.LoadEventEffectsData(untillNextCombat, untillNextStage, untillEndAdventure);
    }

    public void ResetProgress() // 초기화 및 저장
    {
        untillNextCombat.Clear();
        untillNextStage.Clear();
        untillEndAdventure.Clear();

        usedRandomEvnent.Clear();
        stageThemes.Clear();
        eliteClearThemes.Clear();

        StageIndex = 0;
        MinStageIndex = 0;
        RetryFromStart = false;
        StageCleared = false;
        CurrentBattleNode = null;
        SavedStageData = null;
        VisitedNodes.Clear();
        CurrentTheme = default;

        SaveProgress();
        Debug.Log("[ProgressDataManager] 데이터 초기화 완료");
    }



    public void UpdateEventEffectsData(List<EventEffects> com, List<EventEffects> stage, List<EventEffects> adv)
    {
        untillNextCombat = com;
        untillNextStage = stage;
        untillEndAdventure = adv;
    }

    /// <summary>
    /// 스테이지 상태 저장 (맵 데이터 및 방문 노드)
    /// </summary>
    public void SaveStageState(StageData data, List<GraphNode> visited)
    {
        SavedStageData = data;
        VisitedNodes = new List<GraphNode>(visited);
    }

    /// <summary>
    /// 스테이지 상태 초기화 (새 시작 등)
    /// </summary>
    public void ClearStageState()
    {
        //새로하기 기능 있으면 재시작 시테이지도 초기화 시켜줘야함
        SavedStageData = null;
        VisitedNodes.Clear();
    }

    /// <summary>
    /// 사용된 랜덤 이벤트 인덱스 초기화
    /// </summary>
    public void ClearUsedEvents()
    {
        usedRandomEvnent.Clear();
    }

    /// <summary>
    /// 현재 전투 노드 설정
    /// </summary>
    public void SetCurrentBattleNode(GraphNode node)
    {
        CurrentBattleNode = node;
    }

    private void AssignTemesToStages()
    {
        stageThemes[2] = StageTheme.Wisdom;
        stageThemes[3] = StageTheme.Love;
        stageThemes[4] = StageTheme.Courage;

        // 랜덤한 순서로 진행 되는 로직, 타 태마도 완성 시에 위 로직 제거 후 주석 해제
        //List<StageTheme> themePool = new() { StageTheme.Wisdom, StageTheme.Love, StageTheme.Courage };
        //var shuffled = themePool.OrderBy(x => Random.value).ToList();

        //for (int i = 0; i < themePool.Count; i++)
        //{
        //    stageThemes[2 + i] = shuffled[i];
        //}
    }
    public StageTheme GetThemeForStage(int stageIndex)
    {
        return stageThemes.TryGetValue(stageIndex, out var theme) ? theme : StageTheme.Tutorial;
    }

    public void SetTheme(StageTheme theme)
    {
        CurrentTheme = theme;
    }

    //data
    public void EliteClear(StageTheme theme)
    {
        eliteClearThemes.Add(theme);
    }

    //data
    public bool IsEliteClear(StageTheme theme)
    {
        return eliteClearThemes.Contains(theme);
    }

    public RandomEventData GetRandomEvent(StageTheme theme)
    {
        var available = DataManager.Instance.allRandomEvents
            .Where(x => x.theme == theme && !usedRandomEvnent.Contains(x.index))
            .ToList();

        if (available.Count == 0) return null;

        var selected = available[UnityEngine.Random.Range(0, available.Count)];
        usedRandomEvnent.Add(selected.index);
        return selected;
    }
}

[System.Serializable]
public class ProgressSaveData
{
    public int stageIndex;
    public int minStageIndex;
    public bool retryFromStart;
    public bool stageCleared;

    public List<int> visitedNodeIds = new();
    public int currentNodeId;
    public int currentTheme;

    public List<int> untilNextCombatEffects = new();
    public List<int> untilNextStageEffects = new();
    public List<int> untilEndAdventureEffects = new();

    public List<int> usedRandomEventIds = new();
    public Dictionary<int, int> stageThemes = new();
    public List<int> eliteClearThemes = new();
}

[System.Serializable]
public class SavedEventEffect
{
    public int index; // EventEffects의 고유 ID만 저장
}
