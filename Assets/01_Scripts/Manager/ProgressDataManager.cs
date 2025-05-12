using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static StageDataSaveHelper;



public class ProgressDataManager : MonoSingleton<ProgressDataManager>
{
    public const int MAX_ITEM_COUNT = 4;       //현재 전리품의 최종 개수

    [Header("기본 플레이어 파티 데이터")]
    [SerializeField] private PlayerPartySO defaultPlayerParty;
    [SerializeField]public List<PlayerData> PlayerDatas { get; private set; } = new();  //게임에 적용할 플레이어 데이터들.



    public GameStartType GameStartType { get; set; } = new();           //게임이 새로 시작한 게임인지 계속 진행되는 게임인지를 판별

    public HashSet<int> unlockedCards = new();          //unlock된 카드들의 index가 들어있는 hashset
    public int[] itemCounts = new int[MAX_ITEM_COUNT];  //현재 전리품의 개수가 들어있는 배열

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
    public StageTheme CurrentTheme { get; private set; }  // 진행 테마 저장용
    public int SavedEnemySetIndex { get; set; }           // 진행 에너미 세트 저장용


    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitializePlayerData();
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

        if (SavedStageData != null && VisitedNodes != null)
        {
            var dto = StageDataSaveHelper.ConvertToDTO(SavedStageData, VisitedNodes, CurrentBattleNode);
            data.stageDataJson = JsonUtility.ToJson(dto);
        }


        data.currentTheme = (int)CurrentTheme;

        data.untilNextCombatEffects = untillNextCombat.Select(e => e.index).ToList();
        data.untilNextStageEffects = untillNextStage.Select(e => e.index).ToList();
        data.untilEndAdventureEffects = untillEndAdventure.Select(e => e.index).ToList();

        data.usedRandomEventIds = usedRandomEvnent.ToList();
        data.stageThemePairs = stageThemes
            .Select(pair => new StageThemePair { stageIndex = pair.Key, theme = (int)pair.Value })
            .ToList();
        data.eliteClearThemes = eliteClearThemes.Select(e => (int)e).ToList();

        data.unlockedCardIndexes = unlockedCards.ToList();
        data.itemCounts = itemCounts.ToArray();
        data.playerSaves = PlayerDatas.Select(p => new PlayerSaveData
        {
            id = p.IDNum,
            maxHP = p.MaxHP,
            currentHP = p.currentHP,
            currentDeckIndexes = new List<int>(p.currentDeckIndexes)
        }).ToList();

        data.unlockedCharacterIDs = PlayerDatas
            .Where(p => PlayerManager.Instance.activePlayers.ContainsKey(p.CharacterClass)) // 해금된 캐릭터만 저장
            .Select(p => p.IDNum)
            .ToList();

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

        StageIndex = data.stageIndex;
        MinStageIndex = data.minStageIndex;
        RetryFromStart = data.retryFromStart;
        StageCleared = data.stageCleared;

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

        usedRandomEvnent = data.usedRandomEventIds.ToHashSet();
        
        eliteClearThemes = data.eliteClearThemes.Select(i => (StageTheme)i).ToHashSet();


        EventEffectManager.Instance.LoadEventEffectsData(untillNextCombat, untillNextStage, untillEndAdventure);

        unlockedCards = data.unlockedCardIndexes.ToHashSet();

        for (int i = 0; i < Mathf.Min(itemCounts.Length, data.itemCounts.Length); i++)
            itemCounts[i] = data.itemCounts[i];

        ApplySaveToPlayerDatas(data.playerSaves);
        InitializePlayerManagerWithLoadedData(DataManager.Instance.AllCards);

        // 모든 플레이어 초기화
        PlayerManager.Instance.RegisterAndSetupPlayers(PlayerDatas, DataManager.Instance.AllCards);

        // 해금된 캐릭터만 activePlayers에 추가
        foreach (var save in data.unlockedCharacterIDs)
        {
            var character = PlayerDatas.FirstOrDefault(p => p.IDNum == save);
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

    public void ResetProgress() // 초기화 및 저장
    {
        untillNextCombat.Clear();
        untillNextStage.Clear();
        untillEndAdventure.Clear();

        usedRandomEvnent.Clear();
        stageThemes.Clear();
        eliteClearThemes.Clear();

        //이걸로 설정 예정
        //StageIndex = Mathf.Max(1, StageIndex);
        //MinStageIndex = Mathf.Max(1, MinStageIndex);
        AssignTemesToStages();

        StageIndex = 1;
        MinStageIndex = 1;
        SavedEnemySetIndex = -1;

        RetryFromStart = false;
        StageCleared = false;
        CurrentBattleNode = null;
        SavedStageData = null;
        VisitedNodes.Clear();
        CurrentTheme = default;

        PlayerPrefs.DeleteKey("ProgressSaveData");

        SaveProgress();
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
                    data.ResetHPToMax();
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

    public void AssignTemesToStages()
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
    public void SaveEnemySetIndex(int index)
    {
        SavedEnemySetIndex = index;
    }

    public void InitializePlayerManagerWithLoadedData(List<CardModel> allCards)
    {
        PlayerManager.Instance.RegisterAndSetupPlayers(PlayerDatas, allCards);
    }

}

[System.Serializable]
public class ProgressSaveData
{
    public int stageIndex;
    public int minStageIndex;
    public bool retryFromStart;
    public bool stageCleared;

    public string stageDataJson;
    public int currentTheme;

    public List<int> untilNextCombatEffects = new();
    public List<int> untilNextStageEffects = new();
    public List<int> untilEndAdventureEffects = new();

    public List<int> usedRandomEventIds = new();
    public List<StageThemePair> stageThemePairs = new();
    public List<int> eliteClearThemes = new();


    public List<PlayerSaveData> playerSaves= new();
    public List<int> unlockedCardIndexes = new();
    public int[] itemCounts = new int[ProgressDataManager.MAX_ITEM_COUNT];
    public List<int> unlockedCharacterIDs = new();
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



