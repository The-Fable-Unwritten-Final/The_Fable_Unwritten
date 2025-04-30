using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public List<PlayerData> playerDatas = new();  //  보유중인 케릭터 데이터

    public StageSetttingController StageSetting { get; private set; }
    private PlayerPartySO playerParty;

    [Header("Controller")]
    public CombatUIController combatUIController; // 전투 UI 컨트롤러
    public CombatCameraController combatCameraController; // 전투 카메라 컨트롤러
    public CombatLightingController CombatLightingController; // 전투 조명 컨트롤러
    public TurnController turnController; // 턴 컨트롤러
    public CardDiscardController cardDiscardController; // 카드 버리기 컨트롤러
   

    protected override void Awake()
    {
        base.Awake();

        StageSetting = new StageSetttingController();
        StageSetting.Initialize();

        // 추후 데이터매니저? 이동 가능성 있음
        CardSystemInitializer.Instance.LoadCardDatabase();
        EnemySkillInitializer.ImportAndGenerate();
        EnemyInitializer.ImportAndGenerate();
        LoadPlayerPartyIfNull(); // <- 플레이어 데이터 가져오기
        playerDatas = new List<PlayerData>(playerParty.allPlayers);
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

    public void RegisterCombatUI(CombatUIController cont)
    {
        combatUIController = cont;
    }
    public void UnregisterCombatUI()
    {
        combatUIController = null;
    }
    public void RegisterCombatCamera(CombatCameraController cont)
    {
        combatCameraController = cont;
    }
    public void UnregisterCombatCamera()
    {
        combatCameraController = null;
    }
    public void RegisterTurnController(TurnController cont)
    {
        turnController = cont;
    }
    public void UnregisterTurnController()
    {
        turnController = null;
    }
    public void RegisterCardDiscardController(CardDiscardController cont)
    {
        cardDiscardController = cont;
    }
    public void UnregisterCardDiscardController()
    {
        cardDiscardController = null;
    }
    public void RegisterCombatLightingController(CombatLightingController cont)
    {
        CombatLightingController = cont;
    }
    public void UnregisterCombatLightingController()
    {
        CombatLightingController = null;
    }


    public Sprite GetBackgroundForStage(int stageIndex)
    {
        return StageSetting.GetBackground(stageIndex);
    }

    public RandomEventData GetRandomEvent()
    {
        var theme = StageSetting.CurrentTheme;
        return StageSetting.GetRandomEvent(theme);
    }

    public List<EnemyStageSpawnData> GetSpawnData(StageTheme theme, NodeType type)
    {
        return StageSetting.GetEnemySpawnData(theme, type);
    }
}
