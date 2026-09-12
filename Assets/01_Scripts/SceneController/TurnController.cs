using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    public enum TurnState
    {
        GameStart,// 전투의 시작 상태
        StartPlayerTurn,// 플레이어 턴 시작
        PlayerTurn,// 플레이어 턴 진행중
        EndPlayerTurn,// 플레이어 턴 종료
        EnemyTurn,// 적 턴
        GameEnd,// 전투 종료
    }
    public TurnState turnState = TurnState.GameStart; // 현재 턴 상태
    [SerializeField] CardDisplay cardDisplay; // 카드 디스플레이
    public BattleFlowController battleFlow;
    [SerializeField] TurnEndButtonControl TurnButton; 

    // 턴의 각 상태 진입시 호출되는 이벤트
    /// <summary>
    /// 카드 3장씩 드로우 + 배치, 마나 회복
    /// </summary>
    public event Action OnStartPlayerTurn;// 자동 진행
    /// <summary>
    /// 플레이어 카드 사용 가능
    /// </summary>
    public event Action OnPlayerTurn;// 수동으로 행동 후 다음 상태 진행
    /// <summary>
    /// 턴 종료 버튼을 누르면 호출, 캐릭터별 카드 3장 제한을 체크하고 초과하면 해당 캐릭터 카드 버릴것 선택하게 하기
    /// </summary>
    public event Action OnEndPlayerTurn;// 턴 종료 버튼을 누르면 호출 되어, 카드 3장 제한여부에 따라 자동 진행 or 버리는 수동 진행 후 다음 상태 (일단은 자동으로 3장 맞추기 + 자동진행으로)
    /// <summary>
    /// 적 행동 진행
    /// </summary>
    public event Action OnEnemyTurn;// 코루틴으로 몬스터의 행동 진행 + 끝나면 다음 상태로 자동 진행
    /// <summary>
    /// 전투 종료
    /// </summary>
    public event Action OnGameEnd;

    public bool onAction = false;

    private void Awake()
    {
        GameManager.Instance.RegisterTurnController(this);
    }
    private void Start()
    {
        OnStartPlayerTurn += battleFlow.ExecutePlayerTurn;
        //OnStartPlayerTurn += cardDisplay.CardArrange; // 카드 배치 초기화
        OnEndPlayerTurn += battleFlow.EndPlayerTurn;

        StartCoroutine(AtStartGame()); // 게임 시작 후 1초 후에 플레이어 턴으로
    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.F1))
            AtEnemyTurn(); // 적 턴 종료 테스트 코드.
#endif
    }

    private void SetTurnState(TurnState newState)
    {
        turnState = newState;

        switch (turnState)
        {
            case TurnState.GameStart:
                break;

            case TurnState.StartPlayerTurn:
                OnStartPlayerTurn?.Invoke();
                StartCoroutine(AtStartPlayerTurn()); // 대기 후 다음 상태로 넘기기
                break;

            case TurnState.PlayerTurn:
                OnPlayerTurn?.Invoke(); // 플레이어가 수동으로 EndTurn 하기 전까지 대기
                cardDisplay.StartPlayerTurn(); // 플레이어 턴 카드 디스플레이 업데이트
                break;

            case TurnState.EndPlayerTurn: // 플레이어 턴 종료시 필요한 데이터 처리 상태
                OnEndPlayerTurn?.Invoke();
                StartCoroutine(AtEndPlayerTurn()); // 프레임 대기 후 다음 상태로 넘기기
                break;

            case TurnState.EnemyTurn:
                OnEnemyTurn?.Invoke();
                cardDisplay.EndPlayerTurn(); // 카드 디스플레이 업데이트
                StartCoroutine(WaitForEnemyTurn());
                break;

            case TurnState.GameEnd:
                OnGameEnd?.Invoke();
                break;
        }
    }

    private IEnumerator WaitForEnemyTurn()
    {
        bool isDone = false;

        // 적 턴이 끝나면 이 콜백이 실행되도록 + 이곳에서 전투 종료 체크도 진행.
        battleFlow.ExecuteEnemyTurn(() => isDone = true);

        // 기다림
        yield return new WaitUntil(() => isDone);

        yield return new WaitForSeconds(0.5f); // 텀 살짝 주고

        if (turnState == TurnState.GameEnd)
            yield break;

        SetTurnState(TurnState.StartPlayerTurn); // 다음 턴
    }

    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.Instance.UnregisterTurnController();// 씬 나갈 때 턴 컨트롤러 해제
    }
    IEnumerator AtStartGame()
    {
        yield return new WaitForSeconds(0.5f);
        // 데이터 처리
        ProgressDataManager.Instance.turnCount = 0; // 전투 시작 시 턴 수 초기화
        cardDisplay.CardArrange(); // 카드 배치 초기화
        EventEffectManager.Instance.PlayNextCombat();
        EventEffectManager.Instance.PlayNextStage();
        EventEffectManager.Instance.PlayEndAdventure();
        yield return new WaitForSeconds(0.2f);

        cardDisplay.deckInitComplete = true; // 덱 이닛 완료
        battleFlow.StartBattle();
        StyleManager.Instance.isFirstTurnCard = true; // 전투 시작 후 첫 턴 플래그 설정
        // 전투 시작 직후 관련 문체 효과 호출 //
        var randomAlly = battleFlow.GetRandomAliveParty();
        if (randomAlly != null) StyleManager.Instance.ApplyRandomDebuffToSingleAlly(randomAlly); // 살아있는 랜덤한 팀원 한명에게 랜덤 디버프 부여 문체
        // 문체 효과 호출 종료 //

        SetTurnState(TurnState.StartPlayerTurn); // 게임 시작 후 플레이어 턴으로
    }
    IEnumerator AtStartPlayerTurn() // 턴 시작시 제일 먼저 호출
    {
        yield return new WaitForSeconds(0.4f);
        ProgressDataManager.Instance.turnCount++; // 턴 수 증가
        SetTurnState(TurnState.PlayerTurn); // 플레이어 턴으로
        GameManager.Instance.combatUIController.ShowPlayerTurnUI(); // 플레이어 턴 UI 표시
        StyleManager.Instance.isStartOfTurnCard = true; // 턴 시작후 첫 행동 플래그 설정
        // 매 턴 시작 시 호출될 문체 효과들
        StyleManager.Instance.ApplyStunToAllAllies(battleFlow.playerParty);
        // 문체 효과 호출 종료 //

        // 턴 종료 버튼 활성화
        if (TurnButton != null)
            TurnButton.OnStartTurn(); // 버튼 UI 업데이트 (텍스트 보이게 하고 인터렉션 켜기)
    }
    public void AtPlayerTurn()// 턴 종료 버튼을 눌러서 EndPlayerTurn으로 진입
    {
        //playerturn일 떄만 다음 턴 진행 가능
        if (turnState != TurnState.PlayerTurn) return;
        if (onAction) return;

        SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드

        // 카드 초과 체크하고 초과시 카드 버리기 요청.
        if (!GameManager.Instance.cardDiscardController.CheckCountOk()) return; // 만약 카드수량이 초과시 return.
        SetTurnState(TurnState.EndPlayerTurn); // 플레이어 턴 종료
        GameManager.Instance.combatUIController.ShowEnemyTurnUI(); // 적 턴 UI 표시
        // 턴 종료 버튼 비활성화
        if (TurnButton != null)
            TurnButton.OnTurnEnd(); // 버튼 UI 업데이트 (텍스트 숨기고 인터렉션 끄기)
    }
    IEnumerator AtEndPlayerTurn()
    {
        yield return new WaitForEndOfFrame(); // 프레임 대기
        SetTurnState(TurnState.EnemyTurn); // 적 턴으로
    }
    public void AtEnemyTurn()// 몬스터의 행동을 진행하고(몬스터 클라스 쪽에서), 이후 행동이 끝나면 호출. 
    {
        SetTurnState(TurnState.StartPlayerTurn); // 플레이어 턴으로
    }
    public void ToGameEnd(bool isWin)// 아군, 적군 중 한쪽의 체력이 전부 0 이되면 호출. (플레이어 or 몬스터가 행동을 할때마다 전투 종료 체크, 해당 메서드 호출)
    {
        ProgressDataManager.Instance.battleCount++;
        ProgressDataManager pdm = ProgressDataManager.Instance;
        // 턴 종료 버튼 비활성화
        if (TurnButton != null)
            TurnButton.OnBattleEnd(); // 버튼 UI 업데이트 (텍스트 숨기고 인터렉션 끄기)
        // 결과창 팝업을 띄우기 (승패 결과는 battleflowCon 에서 가져올 수 있음 win <<)
        StartCoroutine(WaitAndPopupReward());
        // 데이터 처리
        if (isWin) // 승리 시 처리
        {
            StyleManager.Instance.GetInk(2);
            GameManager.Instance.analyticsLogger.LogRunEndInfo(0, pdm.StageIndex, pdm.battleCount);
            GameManager.Instance.analyticsLogger.LogBattleEndInfo(pdm.SavedEnemySetIndex, 0, pdm.turnCount, BattleLogManager.Instance.UsedCardsForGame.Count);
        }
        else       // 패배 시 처리
        {
            // 캐릭터 maxHP 정보 수집 및 분석 기록
            int sophiaMaxHP = 0; int kylaMaxHP = 0; int leonMaxHP = 0;
            foreach (var player in ProgressDataManager.Instance.PlayerDatas)
            {
                if (player.IDNum == 0) sophiaMaxHP = (int)player.MaxHP;      // 소피아
                else if (player.IDNum == 1) kylaMaxHP = (int)player.MaxHP;  // 카일라
                else if (player.IDNum == 2) leonMaxHP = (int)player.MaxHP;   // 레온
            }

            GameManager.Instance.analyticsLogger.LogCharMaxHPInfo(sophiaMaxHP, kylaMaxHP, leonMaxHP);
            GameManager.Instance.analyticsLogger.LogRunEndInfo(1, pdm.StageIndex, pdm.battleCount);
            GameManager.Instance.analyticsLogger.LogBattleEndInfo(pdm.SavedEnemySetIndex, 1, pdm.turnCount, BattleLogManager.Instance.UsedCardsForGame.Count);
        }
        EventEffectManager.Instance.EndNextCombat();
        SetTurnState(TurnState.GameEnd); // 전투 종료
    }

    private IEnumerator WaitAndPopupReward()
    {
        yield return new WaitForSeconds(2f);
        UIManager.Instance.PopupRewardUI();
    }

    public void Onaction() { if (!onAction) onAction = true; }
    public void OffAction() { if (onAction) onAction = false; }
}

