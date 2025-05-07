using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public List<PlayerData> playerDatas = new();  //  보유중인 케릭터 데이터

    public GameStartType gameStartType = GameStartType.New;


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

#if UNITY_EDITOR
        EnemySkillInitializer.ImportAndGenerate();
        EnemyInitializer.ImportAndGenerate();
#endif

        LoadPlayerPartyIfNull(); // <- 플레이어 데이터 가져오기
        playerDatas = new List<PlayerData>(playerParty.allPlayers);
    }

    public void InitializePlayerHPByGameType()
    {
        switch (gameStartType)
        {
            case GameStartType.New:
                foreach (var data in playerDatas)
                    data.ResetHPToMax();
                gameStartType = GameStartType.Respawn;
                break;

            case GameStartType.Respawn:
                foreach (var data in playerDatas)
                {
                    if (data.currentHP <= 0)
                        data.currentHP = 1;
                    // 살아있다면 유지
                }
                break;
        }
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
