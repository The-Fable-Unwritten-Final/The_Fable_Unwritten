using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public PlayerPrefabData playerPrefabData;     // 모든케릭터 데이터
    public List<PlayerData> playerDatas = new();  //  보유중인 케릭터 데이터
    public GraphNode currentBattleNode;

    public StageData savedStageData;    // 현재 진행 중인 스테이지 데이터
    public List<GraphNode> savedVisitedNodes = new(); // 플레이어가 진행한 노드 리스트
    public int stageIndex;              //현재 스테이지 인덱스
    public int minimumStageIndex;       // 재시작 스테이지 (2스테이지 클리어시 2)
    public bool retryFromStart = false; // 스테이지 실패여서 재시작여부
    public bool stageCleared = false;   // 전투 승리 여부

    private PlayerPartySO playerParty;

    [Header("Controller")]
    public CombatUIController combatUIController; // 전투 UI 컨트롤러
    public TurnController turnController; // 턴 컨트롤러

    protected override void Awake()
    {
        base.Awake();

        CardSystemInitializer.Instance.LoadCardDatabase();
        LoadPlayerPartyIfNull(); // <- 플레이어 데이터 가져오기
        
        playerDatas = new List<PlayerData>(playerParty.allPlayers);

        // 1스테이지 부터 시작
        if (stageIndex <= 0)
            stageIndex = 1;

        if (minimumStageIndex <= 0)
            minimumStageIndex = 1;
    }

    private void LoadPlayerPartyIfNull()
    {
        if (playerParty == null)
        {
            playerParty = Resources.Load<PlayerPartySO>("PlayerPartyData");
            if (playerParty == null)
                Debug.LogError("[GameManager] PlayerPartySO 리소스를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 카드 시스템 및 플레이어 매니저 초기화 (게임 시작 시 1회)
    /// </summary>
    public void InitializeGame(List<PlayerController> playerControllers)
    {
        CardSystemInitializer.Instance.LoadCardDatabase();

        PlayerManager.Instance.RegisterAndSetupPlayers(
            playerControllers,
            playerDatas,
            CardSystemInitializer.Instance.loadedCards
        );

        // 전투 초기화 및 턴 컨트롤러 시작
        if (turnController != null && turnController.battleFlow != null)
        {
            turnController.battleFlow.ReceivePlayerParty(PlayerManager.Instance.GetAllPlayers());
            //turnController.StartBattleFlow();
        }
        else
        {
            Debug.LogWarning("[GameManager] TurnController가 연결되지 않았습니다.");
        }
    }

    /// <summary>
    /// 현재 스테이지 상태 저장
    /// </summary>
    public void SaveStageState(StageData data, List<GraphNode> visited, int index)
    {
        savedStageData = data;
        savedVisitedNodes = new List<GraphNode>(visited);
        stageIndex = index;
    }

    /// <summary>
    /// 저장된 스테이지 상태 초기화
    /// </summary>
    public void ClearStageState()
    {
        //새로하기 기능 있으면 재시작 시테이지(minimumStageIndex)도 초기화 시켜줘야함
        savedStageData = null;
        savedVisitedNodes.Clear();
    }

    public GameObject GetPlayerPrefab(int id)
    {
        return playerPrefabData.GetPrefab(id);
    }    

    public void RegisterCombatUI(CombatUIController cont)
    {
        combatUIController = cont;
    }
    public void UnregisterCombatUI()
    {
        combatUIController = null;
    }
    public void RegisterTurnController(TurnController cont)
    {
        turnController = cont;
    }
    public void UnregisterTurnController()
    {
        turnController = null;
    }
}
