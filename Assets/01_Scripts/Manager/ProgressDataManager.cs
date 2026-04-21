using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static StageDataSaveHelper;
using System;

public partial class ProgressDataManager : MonoSingleton<ProgressDataManager>
{
    public const int MAX_ITEM_COUNT = 4;       //현재 전리품의 최종 개수

    [Header("기본 플레이어 파티 데이터")]
    [SerializeField] private PlayerPartySO defaultPlayerParty;
    [SerializeField]public List<PlayerData> PlayerDatas { get; private set; } = new();  //게임에 적용할 플레이어 데이터들.


    public GameStartType GameStartType { get; set; } = new();           //게임이 새로 시작한 게임인지 계속 진행되는 게임인지를 판별

    public HashSet<int> unlockedCards = new();          //unlock된 카드들의 index가 들어있는 hashset
    [SerializeField]public int[] itemCounts = new int[MAX_ITEM_COUNT];  //현재 전리품의 개수가 들어있는 배열

    List<EventEffects> untillNextCombat = new List<EventEffects>(); // 다음 전투까지 지속되는 효과 리스트
    List<EventEffects> untillNextStage = new List<EventEffects>(); // 다음 스테이지까지 지속되는 효과 리스트
    List<EventEffects> untillEndAdventure = new List<EventEffects>(); // 모험이 끝날 때까지 지속되는 효과 리스트

    // 문체 시스템
    public int currentDefID = 1;                     // 현재 적용 중인 문체 ID
    public int inkAmount = 0;                        // 보유 잉크
    public HashSet<int> unlockedStyles = new();     // 해금된 문체 ID 목록
    public HashSet<int> unlockedCharacterIDs = new(); // 해금된 캐릭터 ID 목록

    // 랜덤 이벤트
    HashSet<int> usedRandomEvent = new();     // RandomEvent 진행 유무(게임 재시작 및 실패 시 초기화 - ClearUsedEvents())
    HashSet<int> TriggeredRandomEvent = new(); // 인과 형식의 랜덤 이벤트가 활성화 된 경우 저장.
    Dictionary<int, StageTheme> stageThemes = new(); // 2~4 스테이지용 테마
    HashSet<StageTheme> eliteClearThemes = new(); // Theme 별 Elite Clear 리스트

    // 스테이지 방문 & 현재 노드 정보
    public HashSet<int> ProgressTutorial = new();
    public int StageIndex { get; set; }                // 현재 스테이지
    public int MinStageIndex { get; set; }             // 재시작 스테이지 (2스테이지 클리어시 2)
    public bool RetryFromStart { get; set; }           // 스테이지 실패시 재시작여부
    public bool StageCleared { get; set; }             // 전투 승리 여부
    public bool IsNewStage { get; set; }               // 새 스테이지 여부 (튜토리얼 용)
    public bool IsStageScene { get; set; }             // 마지막 플레이 중이였던 컨텐츠 스테이지씬 여부 (전투,랜덤이벤트,휴식 등..)
    public GraphNode CurrentBattleNode { get; set; }   // 현재 선택한 노드
    public StageData SavedStageData { get; private set; }               // 현재 진행 중인 스테이지 데이터
    public List<GraphNode> VisitedNodes { get; private set; } = new();  // 플레이어가 진행한 노드 리스트
    public StageTheme CurrentTheme { get; private set; }  // 진행 테마 저장용
    public int SavedEnemySetIndex { get; set; }           // 진행 에너미 세트 저장용
    public int SavedRandomEvent { get; set; }             // 저장용 랜던이밴트 인덱스
    public int CurrentExp { get; set; }                 //현재까지 얻은 Exp;
    public bool IsNewCamp { get; set; }                 // 첫 야영지 확인용 (첫 캠프에만 튜토리얼)
    public bool IsSecondGame { get; set; }                  // 새로하기 확인용 (완전 처음 일때 false / 이후 새로하기 일때 true)
    public bool IsEndingClear { get; set; }                 // 앤딩봤을 경우
    // 설정 데이터
    public Vector2Int[] resolutions = new Vector2Int[1];

    protected override void Awake()
    {
        base.Awake();
        LoadResolution();// 해상도 불러오기
    }

    private void Start()
    {
        InitializePlayerData();
        LoadProgress();

        DataManager.Instance.InitCardUnlockStatus();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            UnlockStyle(8);
        }
    }
    // 나중에 저장을 세부적으로 쪼개기
    public void SaveProgress(bool safe)
    {
        ProgressSaveData data = new ProgressSaveData();

        data.GameStartType = (int)GameStartType;
        data.stageIndex = StageIndex;
        data.minStageIndex = MinStageIndex;
        data.isNewStage = IsNewStage;
        data.retryFromStart = RetryFromStart;
        data.stageCleared = StageCleared;
        data.isStageScene = IsStageScene;
        data.savedEnemySetIndex = SavedEnemySetIndex;
        data.isNewCamp = IsNewCamp;
        data.isSecondGame = IsSecondGame;
        data.resolutions = resolutions;
        data.isEndingClear = IsEndingClear;

        if (SavedStageData != null && VisitedNodes != null)
        {
            var dto = StageDataSaveHelper.ConvertToDTO(SavedStageData, VisitedNodes, CurrentBattleNode);
            data.stageDataJson = JsonUtility.ToJson(dto);
        }


        data.currentTheme = (int)CurrentTheme;

        data.untilNextCombatEffects = untillNextCombat.Select(e => e.index).ToList();
        data.untilNextStageEffects = untillNextStage.Select(e => e.index).ToList();
        data.untilEndAdventureEffects = untillEndAdventure.Select(e => e.index).ToList();

        data.progressTutorial = ProgressTutorial.ToList();
        data.usedRandomEventIds = usedRandomEvent.ToList();
        data.chainedTriggeredEvents = TriggeredRandomEvent.ToList();
        data.savedRandomEvent = SavedRandomEvent;
        data.stageThemePairs = stageThemes
            .Select(pair => new StageThemePair { stageIndex = pair.Key, theme = (int)pair.Value })
            .ToList();
        data.eliteClearThemes = eliteClearThemes.Select(e => (int)e).ToList();

        SaveStyleData(data); // 문체 데이터 저장

        data.unlockedCardIndexes = unlockedCards.ToList();
        data.itemCounts = itemCounts.ToArray();
        if(safe)
            data.playerSaves = PlayerDatas.Select(p => new PlayerSaveData
            {
                id = p.IDNum,
                maxHP = p.MaxHP,
                currentHP = p.currentHP,
                currentDeckIndexes = new List<int>(p.currentDeckIndexes)
            }).ToList();

        data.unlockedCharacterIDs = unlockedCharacterIDs.ToList();

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString("ProgressSaveData", json);
        PlayerPrefs.Save();
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

        GameStartType = (GameStartType)data.GameStartType;
        StageIndex = data.stageIndex;
        MinStageIndex = data.minStageIndex;
        IsNewStage = data.isNewStage;
        RetryFromStart = data.retryFromStart;
        StageCleared = data.stageCleared;
        IsStageScene = data.isStageScene;
        SavedEnemySetIndex = data.savedEnemySetIndex;
        IsNewCamp = data.isNewCamp;
        IsSecondGame = data.isSecondGame;
        IsEndingClear = data.isEndingClear;

        //stageThemes = data.stageThemes.ToDictionary(pair => pair.Key, pair => (StageTheme)pair.Value);

        stageThemes = data.stageThemePairs.ToDictionary(pair => pair.stageIndex, pair => (StageTheme)pair.theme);

        CurrentTheme = (StageTheme)data.currentTheme;

        if (!string.IsNullOrEmpty(data.stageDataJson))
        {
            var dto = JsonUtility.FromJson<StageDataDTO>(data.stageDataJson);
            SavedStageData = StageDataSaveHelper.ConvertFromDTO(dto, out var visited, out var current);
            VisitedNodes = visited;
            CurrentBattleNode = current;
        }

        untillNextCombat = data.untilNextCombatEffects
            .Select(index => EventEffectManager.Instance.eventEffectDict[index].Clone())
            .ToList();

        untillNextStage = data.untilNextStageEffects
            .Select(index => EventEffectManager.Instance.eventEffectDict[index].Clone())
            .ToList();

        untillEndAdventure = data.untilEndAdventureEffects
            .Select(index => EventEffectManager.Instance.eventEffectDict[index].Clone())
            .ToList();

        SavedRandomEvent = data.savedRandomEvent;
        usedRandomEvent = data.usedRandomEventIds.ToHashSet();
        TriggeredRandomEvent = data.chainedTriggeredEvents.ToHashSet();
        ProgressTutorial = data.progressTutorial.ToHashSet();
        eliteClearThemes = data.eliteClearThemes.Select(i => (StageTheme)i).ToHashSet();


        EventEffectManager.Instance.LoadEventEffectsData(untillNextCombat, untillNextStage, untillEndAdventure);

        unlockedCards = data.unlockedCardIndexes.ToHashSet();

        for (int i = 0; i < Mathf.Min(itemCounts.Length, data.itemCounts.Length); i++)
            itemCounts[i] = data.itemCounts[i];

        ApplySaveToPlayerDatas(data.playerSaves);
        InitializePlayerManagerWithLoadedData(DataManager.Instance.AllCards);

        LoadStyleData(data); //문체 데이터 로드
        unlockedCharacterIDs = data.unlockedCharacterIDs.ToHashSet();

        // 모든 플레이어 초기화
        PlayerManager.Instance.RegisterAndSetupPlayers(PlayerDatas, DataManager.Instance.AllCards);

        // 해금된 캐릭터만 activePlayers에 추가
        foreach (var characterId in unlockedCharacterIDs)
        {
            var character = PlayerDatas.FirstOrDefault(p => p.IDNum == characterId);
            if (character != null)
            {
                PlayerManager.Instance.AddPlayerDuringGame(character, DataManager.Instance.AllCards);
            }
        }
    }

    public void ApplySaveToPlayerDatas(List<PlayerSaveData> saves)
    {
        foreach (var save in saves)
        {
            var match = PlayerDatas.FirstOrDefault(p => p.IDNum == save.id);
            if (match != null)
            {
                match.MaxHP = save.maxHP;
                match.currentHP = save.currentHP;
                match.currentDeckIndexes = new List<int>(save.currentDeckIndexes);
            }
            else
            {
                Debug.LogWarning($"[ProgressDataManager] 저장된 플레이어 ID {save.id}를 찾을 수 없습니다.");
            }
        }
    }

    public void FullResetProgress() // 완전 초기화 (게임을 처음 시작하는 상태로 초기화)
    {
        GameStartType = GameStartType.New;
        BattleLogManager.Instance.ResetGameLog();
        untillNextCombat.Clear();
        untillNextStage.Clear();
        untillEndAdventure.Clear();

        usedRandomEvent.Clear();
        TriggeredRandomEvent.Clear();
        stageThemes.Clear();
        eliteClearThemes.Clear();

        //이걸로 설정 예정(유저테스트 이후) 변경
        //StageIndex = Mathf.Max(1, StageIndex);
        //MinStageIndex = Mathf.Max(1, MinStageIndex);
        AssignThemesToStages();

        int tempstageind = StageIndex;
        StageIndex = 1;
        MinStageIndex = 1;

        CurrentExp = 0;
        SavedEnemySetIndex = -1;
        SavedRandomEvent = -1;
        IsNewCamp = true;
        IsNewStage = true;
        RetryFromStart = true;
        StageCleared = false;
        IsStageScene = true;
        IsSecondGame = false;
        CurrentBattleNode = null;
        SavedStageData = null;
        VisitedNodes.Clear();
        CurrentTheme = default;

        foreach (StyleDefinition st in StyleManager.Instance.StyleDic.Values)
        {
            st.ResetProgress(); // 진행도 1로 초기화
        }
        if (StyleManager.Instance.StyleDic.TryGetValue(0, out var chaos))
        {
            // 혼돈 문체 초기화
            chaos.plusTiers.Clear();
            chaos.minusTiers.Clear();
            chaos.isUnlocked = false;
            //
            if (tempstageind >= 3)
            {
                // 스테이지 진행도가 2번째 스테이지 진입 상태 시 새로운 혼돈 문체로 설정
                int styleCount = DataManager.Instance.styleDefs.Count;
                int rnd = UnityEngine.Random.Range(1, styleCount);

                // 랜덤하게 정해진 plus 효과와 동일한 등급의 minus 효과로 재설정
                StyleDefinition.StyleRank rank = DataManager.Instance.styleDefs[rnd].rank;
                var plus = DataManager.Instance.styleDefs[rnd].plusTiers;
                var sameRankIndexes = Enumerable.Range(1, styleCount - 1)
                                    .Where(i => i != rnd && DataManager.Instance.styleDefs[i].rank == rank)
                                    .ToList();
                rnd = sameRankIndexes[UnityEngine.Random.Range(0, sameRankIndexes.Count)];
                var minus = DataManager.Instance.styleDefs[rnd].minusTiers;

                DataManager.Instance.styleDefs[0].plusTiers = plus;
                DataManager.Instance.styleDefs[0].minusTiers = minus;
                chaos.isUnlocked = true;
            }
        }
        currentDefID = 1;
        inkAmount = 0;
        // 카드 해금, 문체 해금, 캐릭터 해금 초기화
        unlockedCards.Clear();
        unlockedCharacterIDs.Clear();
        InitializeDefaultStyleUnlock();

        PlayerPrefs.DeleteKey("ProgressSaveData");

        SaveProgress(true);
        // 저장된 데이터 다시 로드하여 메모리에 반영
        LoadProgress();
    }
    public void ResetProgress() // 튜토리얼을 끝낸 이후 new game 시 호출 및 저장 (카드 해금, 문체 해금의 경우 보존)
    {   
        GameStartType = GameStartType.New;
        BattleLogManager.Instance.ResetGameLog();
        untillNextCombat.Clear();
        untillNextStage.Clear();
        untillEndAdventure.Clear();

        usedRandomEvent.Clear();
        TriggeredRandomEvent.Clear();
        stageThemes.Clear();
        eliteClearThemes.Clear();

        AssignThemesToStages();

        int tempstageind = StageIndex;
        StageIndex = 2;
        MinStageIndex = 2;

        CurrentExp = 0;
        SavedEnemySetIndex = -1;
        SavedRandomEvent = -1;
        IsNewCamp = true;
        IsNewStage = false;
        RetryFromStart = true;
        StageCleared = false;
        IsStageScene = true;
        CurrentBattleNode = null;
        SavedStageData = null;
        VisitedNodes.Clear();
        CurrentTheme = default;

        foreach (StyleDefinition st in StyleManager.Instance.StyleDic.Values)
        {
            st.ResetProgress(); // 진행도 1로 초기화
        }
        if (StyleManager.Instance.StyleDic.TryGetValue(0, out var chaos))
        {
            // 혼돈 문체 초기화
            chaos.plusTiers.Clear();
            chaos.minusTiers.Clear();
            chaos.isUnlocked = false;
            
            if (tempstageind >= 3)
            {
                int styleCount = DataManager.Instance.styleDefs.Count;
                int rnd = UnityEngine.Random.Range(1, styleCount);

                StyleDefinition.StyleRank rank = DataManager.Instance.styleDefs[rnd].rank;
                var plus = DataManager.Instance.styleDefs[rnd].plusTiers;
                var sameRankIndexes = Enumerable.Range(1, styleCount - 1)
                                    .Where(i => i != rnd && DataManager.Instance.styleDefs[i].rank == rank)
                                    .ToList();
                rnd = sameRankIndexes[UnityEngine.Random.Range(0, sameRankIndexes.Count)];
                var minus = DataManager.Instance.styleDefs[rnd].minusTiers;

                DataManager.Instance.styleDefs[0].plusTiers = plus;
                DataManager.Instance.styleDefs[0].minusTiers = minus;
                chaos.isUnlocked = true;
            }
        }
        currentDefID = 1;
        inkAmount = 0;

        PlayerPrefs.DeleteKey("ProgressSaveData");
        SaveProgress(true);   
        // 저장된 데이터 다시 로드하여 메모리에 반영
        LoadProgress();
    }

    public void InitializePlayerData()      //아예 초기 데이터로 완전 초기화
    {
        PlayerDatas = new List<PlayerData>(defaultPlayerParty.allPlayers);
    }
    public void InitializePlayerHPByGameType()
    {
        switch (GameStartType)
        {
            case GameStartType.New:
                foreach (var data in PlayerDatas)
                {
                    data.ResetHPToMax();
                    data.ResetDeckIndexesToDefault();
                }
                GameStartType = GameStartType.Respawn;
                break;

            case GameStartType.Respawn:
                foreach (var data in PlayerDatas)
                {
                    if (data.currentHP <= 0)
                        data.currentHP = 1;
                    // 살아있다면 유지
                }
                break;
        }
    }
    // 진행한 튜토리얼 추가 시키기
    public void AddProgressTutorial(int index)
    {
        ProgressTutorial.Add(index);
    }
    public void UpdateEventEffectsData(List<EventEffects> com, List<EventEffects> stage, List<EventEffects> adv)
    {
        untillNextCombat = com;
        untillNextStage = stage;
        untillEndAdventure = adv;

        SaveProgress(true);
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
        usedRandomEvent.Clear();
    }
    /// <summary>
    /// 현재 전투 노드 설정
    /// </summary>
    public void SetCurrentBattleNode(GraphNode node)
    {
        CurrentBattleNode = node;
    }
    public void AssignThemesToStages()
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
    public void EliteClear(StageTheme theme)
    {
        eliteClearThemes.Add(theme);
    }
    public bool IsEliteClear(StageTheme theme)
    {
        return eliteClearThemes.Contains(theme);
    }
    public RandomEventData GetRandomEvent(StageTheme theme)
    {
        var available = DataManager.Instance.allRandomEvents
        // 현재 테마와 일치하거나 공통 이벤트중, 등장하지 않은 이벤트 선정
        // 천의 자리수가 0인 이벤트만 선택 (x1xxx는 연속 이벤트이기 때문에 제외)
            .Where(x => (x.theme == theme || x.theme == 0) && !usedRandomEvent.Contains(x.index) && (x.index / 1000) % 10 == 0)
            .ToList();

        if (available.Count == 0) return null;

        var selected = available[UnityEngine.Random.Range(0, available.Count)];
        usedRandomEvent.Add(selected.index);
        return selected;
    }
    public void SaveEnemySetIndex(int index)
    {
        SavedEnemySetIndex = index;
    }
    public void InitializePlayerManagerWithLoadedData(List<CardModel> allCards)
    {
        PlayerManager.Instance.RegisterAndSetupPlayers(PlayerDatas, allCards);
    }
    /// <summary>
    /// 인과형 랜덤 이벤트의 활성화 여부
    /// </summary>
    public bool IsChainEventTriggered(int eventIndex)
    {
        return TriggeredRandomEvent.Contains(eventIndex);
    }
    public void SetChainEventTriggered(int eventIndex)
    {
        // 유효한 이벤트 인덱스일 경우 플레그 저장
        if(DataManager.Instance.allRandomEvents.Any(e => e.index == eventIndex))
            TriggeredRandomEvent.Add(eventIndex);
    }
    public void LoadResolution()
    {
        string json = PlayerPrefs.GetString("ProgressSaveData");
        ProgressSaveData data = JsonUtility.FromJson<ProgressSaveData>(json);

        if (data.resolutions != null && data.resolutions.Length > 0)
        {
            resolutions = data.resolutions;
            Screen.SetResolution(resolutions[0].x, resolutions[0].y, false);
        }
        else
        {
            // 저장된 해상도 데이터가 없을경우 FHD 적용
            resolutions[0] = new Vector2Int(1920, 1080);
            Screen.SetResolution(resolutions[0].x, resolutions[0].y, false);
        }
    }
    // 각 시스템 별 세이브 로드 분리

    /// <summary>
    /// 문체 시스템의 정보를 ProgressData 매니저 쪽에 저장
    /// </summary>
    public void SetStyleData(int CurrentID)
    {
        currentDefID = CurrentID;
    }
    private void SaveStyleData(ProgressSaveData data)
    {
        data.currentdefid = currentDefID;
        data.inkamount = inkAmount;
        data.unlockedStyleIds = unlockedStyles.ToList();
    }

    private void LoadStyleData(ProgressSaveData data)
    {
        currentDefID = data.currentdefid;
        inkAmount = data.inkamount;
        unlockedStyles = data.unlockedStyleIds.ToHashSet();

        // 로드된 해금 상태를 StyleManager에 반영
        if (StyleManager.Instance != null)
        {
            foreach (var styleId in unlockedStyles)
            {
                if (StyleManager.Instance.StyleDic.TryGetValue(styleId, out var style))
                {
                    style.isUnlocked = true;
                }
            }
        }
    }

    /// <summary>
    /// 기본 해금 문체 초기화 (ID 1, 2, 3, 4는 기본 해금)
    /// 게임을 최초 플레이 시 에만 호출 할 메서드
    /// </summary>
    private void InitializeDefaultStyleUnlock()
    {
        unlockedStyles.Clear();
        
        var styles = DataManager.Instance.styleDefs;
        foreach (var style in styles) style.isUnlocked = false; // 일단 모두 잠금
        // 1,2,3,4,6 문체는 기본 해금
        unlockedStyles.Add(1);
        unlockedStyles.Add(2);
        unlockedStyles.Add(3);
        unlockedStyles.Add(4);
        unlockedStyles.Add(6); 
        
        // StyleManager가 준비되었다면 반영
        if (StyleManager.Instance != null)
        {
            foreach (var styleId in unlockedStyles)
            {
                if (StyleManager.Instance.StyleDic.TryGetValue(styleId, out var style))
                {
                    style.isUnlocked = true;
                }
            }
        }
    }

    /// <summary>
    /// '특정 문체 ID'를 해금 처리
    /// </summary>
    /// <param name="styleId">해금할 문체 ID </param>
    public void UnlockStyle(int styleId)
    {
        if (!unlockedStyles.Contains(styleId))
        {
            unlockedStyles.Add(styleId);

            // StyleManager 반영
            if (StyleManager.Instance != null && StyleManager.Instance.StyleDic.TryGetValue(styleId, out var style))
            {
                style.isUnlocked = true;
            }
        }
    }
    /// <summary>
    /// 게임 클리어 시 잠금 상태의 '랜덤한 문체 하나'를 잠금 해제
    /// </summary>
    public void UnlockRandomStyle()
    {
        List<int> lockstyles = StyleManager.Instance.StyleDic.Keys
            .Where(id => !unlockedStyles.Contains(id) && id != 0) // 0은 혼돈 문체이므로 제외
            .ToList();

        if (lockstyles.Count == 0)
        {
            Debug.Log("[ProgressDataManager] 해금 가능한 문체가 없습니다.");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, lockstyles.Count);
        int styleId = lockstyles[randomIndex];
        UnlockStyle(styleId);
    }

    private void OnApplicationQuit()
    {
        try
        {
            SaveProgress(false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save on quit failed: {ex}");
        }
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            try
            {
                SaveProgress(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save on pause failed: {ex}");
            }
        }
    }
}

[System.Serializable]
public class ProgressSaveData
{
    public int GameStartType;
    public int stageIndex;
    public int minStageIndex;
    public bool isNewStage;
    public bool retryFromStart;
    public bool stageCleared;
    public bool isStageScene;
    public int savedEnemySetIndex;
    public bool isNewCamp;
    public bool isSecondGame;
    public bool isEndingClear;

    public string stageDataJson;
    public int currentTheme;

    public List<int> progressTutorial = new();
    public List<int> untilNextCombatEffects = new();
    public List<int> untilNextStageEffects = new();
    public List<int> untilEndAdventureEffects = new();


    // 문체 시스템
    public int currentdefid;
    public int inkamount;
    public List<int> unlockedStyleIds = new();

    // 랜덤 이벤트
    public int savedRandomEvent;
    public List<int> usedRandomEventIds = new();
    public List<int> chainedTriggeredEvents = new();
    public List<StageThemePair> stageThemePairs = new();
    public List<int> eliteClearThemes = new();

    public List<PlayerSaveData> playerSaves= new();
    public List<int> unlockedCardIndexes = new();
    public int[] itemCounts = new int[ProgressDataManager.MAX_ITEM_COUNT];
    public List<int> unlockedCharacterIDs = new(); // 저장용 필드 (HashSet -> List 직렬화)

    public Vector2Int[] resolutions;

    [System.Serializable] public class IdealCounterEntry { public string key; public int value; }

    public List<int> unlockedIdealIds = new();
    public List<IdealCounterEntry> idealCounters = new();
}

//
[System.Serializable]
public class StageDataDTO
{
    public int columnCount;
    public List<GraphNodeDTOList> columns = new();
    public List<int> visitedNodeIds = new();
    public int currentNodeId = -1;
}

[System.Serializable]
public class GraphNodeDTOList
{
    public List<GraphNodeDTO> nodes = new();
}

[System.Serializable]
public class GraphNodeDTO
{
    public int id;
    public NodeType type;
    public int columnIndex;
    public float posX;
    public float posY;

    public List<int> nextNodeIds = new();
}

[System.Serializable]
public class StageThemePair
{
    public int stageIndex;
    public int theme;
}

[System.Serializable]
public class PlayerSaveData
{
    public int id;
    public float maxHP;
    public float currentHP;
    public List<int> currentDeckIndexes = new();
}

public static class StageDataSaveHelper
{
    // 저장용 DTO로 변환
    public static StageDataDTO ConvertToDTO(StageData stage, List<GraphNode> visitedNodes, GraphNode currentNode)
    {
        var dto = new StageDataDTO
        {
            columnCount = stage.columnCount,
            visitedNodeIds = visitedNodes.Select(n => n.id).ToList(),
            currentNodeId = currentNode?.id ?? -1
        };

        foreach (var column in stage.columns)
        {
            var columnDTO = new GraphNodeDTOList();

            foreach (var node in column)
            {
                columnDTO.nodes.Add(new GraphNodeDTO
                {
                    id = node.id,
                    type = node.type,
                    columnIndex = node.columnIndex,
                    posX = node.position.x,
                    posY = node.position.y,
                    nextNodeIds = node.nextNodes.Select(n => n.id).ToList()
                });
            }

            dto.columns.Add(columnDTO);
        }

        return dto;
    }

    // DTO를 실제 StageData로 복원
    public static StageData ConvertFromDTO(StageDataDTO dto, out List<GraphNode> visitedNodes, out GraphNode currentNode)
    {
        var stage = new StageData
        {
            columnCount = dto.columnCount,
            columns = new List<List<GraphNode>>()
        };

        Dictionary<int, GraphNode> nodeMap = new();

        foreach (var columnDTO in dto.columns)
        {
            var column = new List<GraphNode>();

            foreach (var nodeDTO in columnDTO.nodes)
            {
                var node = new GraphNode
                {
                    id = nodeDTO.id,
                    type = nodeDTO.type,
                    columnIndex = nodeDTO.columnIndex,
                    position = new Vector2(nodeDTO.posX, nodeDTO.posY)
                };
                column.Add(node);
                nodeMap[node.id] = node;
            }

            stage.columns.Add(column);
        }

        // 여기가 연결 복원 구간
        foreach (var columnDTO in dto.columns)
        {
            foreach (var nodeDTO in columnDTO.nodes)
            {
                var node = nodeMap[nodeDTO.id];
                node.nextNodes = nodeDTO.nextNodeIds
                    .Where(id => nodeMap.ContainsKey(id))
                    .Select(id => nodeMap[id])
                    .ToList();
            }
        }

        visitedNodes = dto.visitedNodeIds
            .Where(nodeMap.ContainsKey)
            .Select(id => nodeMap[id])
            .ToList();

        currentNode = nodeMap.ContainsKey(dto.currentNodeId) ? nodeMap[dto.currentNodeId] : null;

        return stage;
    }
}



