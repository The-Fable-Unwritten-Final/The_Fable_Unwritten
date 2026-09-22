using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [Header("Analytics")]
    public AnalyticsLogger analyticsLogger; // 애널리틱스 로거

    [Header("Controller")]
    public CombatUIController combatUIController; // 전투 UI 컨트롤러
    public CombatCameraController combatCameraController; // 전투 카메라 컨트롤러
    public CombatLightingController CombatLightingController; // 전투 조명 컨트롤러
    public TurnController turnController; // 턴 컨트롤러
    public CardDiscardController cardDiscardController; // 카드 버리기 컨트롤러
    public TutorialController tutorialController; // 튜토리얼 컨트롤러 

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // Ctrl + 9 누르면 스테이지 클리어 (심의용 빠른 테스트 기능, 텐키리스 대응)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha9))
        {
            OnTestStageClear();
        }
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
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
    public void RegisterTutorialController(TutorialController cont)
    {
        tutorialController = cont;
    }
    public void UnRegisterTutorialController()
    {
        tutorialController = null;
    }

    /// <summary>
    /// 테스트용 스테이지 클리어 (Ctrl + 9)
    /// </summary>
    private void OnTestStageClear()
    {
        var setting = ProgressDataManager.Instance;
        setting.RetryFromStart = false;
        setting.StageCleared = true;

        // 1 스테이지 클리어 후, 실패 시 2스테이지부터 시작하게 설정
        if (setting.StageIndex == 1)
        {
            if (setting.VisitedNodes.Count > 0)
            {
                var lasVisitde = setting.VisitedNodes[setting.VisitedNodes.Count - 1];
                var lasColum = setting.SavedStageData.columns[^1];

                if (lasColum.Contains(lasVisitde))
                {
                    setting.MinStageIndex = 2;
                }
            }
        }

        if (setting.CurrentNode != null)
        {
            if (setting.CurrentNode.type == NodeType.Boss ||
                    (setting.StageIndex == 1 && setting.CurrentNode.columnIndex == 3))
            {
                setting.IsNewStage = true;
            }
            else
            {
                setting.IsNewStage = false;
            }

            if (setting.CurrentNode.type == NodeType.EliteBattle)
            {
                setting.EliteClear(setting.CurrentTheme);
            }
        }

        ProgressDataManager.Instance.SavedEnemySetIndex = -1; // 랜덤 에너미 셋 초기화

        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
    }
}
