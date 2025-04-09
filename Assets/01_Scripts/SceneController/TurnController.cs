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

    // 턴의 각 상태 진입시 호출되는 이벤트
    /// <summary>
    /// 카드 3장씩 드로우 + 배치, 마나 회복
    /// </summary>
    public Action OnStartPlayerTurn;
    /// <summary>
    /// 플레이어 카드 사용 가능
    /// </summary>
    public Action OnPlayerTurn;
    /// <summary>
    /// 턴 종료 버튼을 누르면 호출, 캐릭터별 카드 3장 제한을 체크하고 초과하면 해당 캐릭터 카드 버릴것 선택하게 하기
    /// </summary>
    public Action OnEndPlayerTurn;
    /// <summary>
    /// 적 행동 진행
    /// </summary>
    public Action OnEnemyTurn;
    /// <summary>
    /// 전투 종료
    /// </summary>
    public Action OnGameEnd;

    private void Awake()
    {
        GameManager.Instance.RegisterTurnController(this);
    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }


    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.Instance.UnregisterTurnController();// 씬 나갈 때 턴 컨트롤러 해제
    }
}
