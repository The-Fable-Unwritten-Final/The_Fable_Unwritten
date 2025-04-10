using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public PlayerPrefabData playerPrefabData;
    public List<PlayerData> playerDatas = new();

    public StageData savedStageData;    // 현재 진행 중인 스테이지 데이터
    public List<GraphNode> savedVisitedNodes = new(); // 플레이어가 진행한 노드 리스트
    public int stageIndex;              //현재 스테이지 인덱스
    public int minimumStageIndex;       // 재시작 스테이지 (2스테이지 클리어시 2)
    public bool retryFromStart = false; // 스테이지 실패여서 재시작여부
    public bool stageCleared = false;   // 전투 승리 여부

    [Header("Controller")]
    public CombatUIController combatUIController; // 전투 UI 컨트롤러
    public TurnController turnController; // 턴 컨트롤러

    protected override void Awake()
    {
        base.Awake();

        // 1스테이지 부터 시작
        if (stageIndex <= 0)
            stageIndex = 1;

        if (minimumStageIndex <= 0)
            minimumStageIndex = 1;
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
