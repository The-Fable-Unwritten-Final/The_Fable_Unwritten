using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // 적 턴이 끝나면 이 콜백이 실행되도록
        battleFlow.ExecuteEnemyTurn(() => isDone = true);

        // 기다림
        yield return new WaitUntil(() => isDone);

        yield return new WaitForSeconds(0.5f); // 텀 살짝 주고
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
        cardDisplay.CardArrange(); // 카드 배치 초기화
        EventEffectManager.Instance.PlayNextCombat();
        EventEffectManager.Instance.PlayNextStage();
        EventEffectManager.Instance.PlayEndAdventure();
        yield return new WaitForSeconds(0.2f);

        battleFlow.StartBattle();
        SetTurnState(TurnState.StartPlayerTurn); // 게임 시작 후 플레이어 턴으로
    }
    IEnumerator AtStartPlayerTurn()
    {
        yield return new WaitForSeconds(0.4f);
        SetTurnState(TurnState.PlayerTurn); // 플레이어 턴으로
    }
    public void AtPlayerTurn()// 턴 종료 버튼을 눌러서 EndPlayerTurn으로 진입
    {
        //playerturn일 떄만 다음 턴 진행 가능
        if (turnState != TurnState.PlayerTurn) return;
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
        // 카드 초과 체크하고 초과시 카드 버리기 요청.
        if (!GameManager.Instance.cardDiscardController.CheckCountOk()) return; // 만약 카드수량이 초과시 return.
        SetTurnState(TurnState.EndPlayerTurn); // 플레이어 턴 종료
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
    public void ToGameEnd()// 아군, 적군 중 한쪽의 체력이 전부 0 이되면 호출. (플레이어 or 몬스터가 행동을 할때마다 전투 종료 체크, 해당 메서드 호출)
    {
        // 결과창 팝업을 띄우기 (승패 결과는 battleflowCon 에서 가져올 수 있음 win <<)
        UIManager.Instance.PopupRewardUI();
        // 데이터 처리
        EventEffectManager.Instance.EndNextCombat();
        SetTurnState(TurnState.GameEnd); // 전투 종료
    }

    public void Onaction() { if (!onAction) onAction = true; }
    public void OffAction() { if (onAction) onAction = false; }
}
