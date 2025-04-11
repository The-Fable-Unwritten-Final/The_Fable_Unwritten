using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 스테이지 흐름을 제어하는 메인 매니저 클래스
/// </summary>
public class StageMapController : MonoBehaviour
{
    [Header("Stage Settings")]  
    [SerializeField] Vector2 spacing = new(300, 200);      // 노드 간격
    [SerializeField] int mapTargetWidth;                   // 지도 가로 크기

    [Header("References")]
    public StageMapRenderer mapRenderer;                   // StageMapRederer 연결

    private int stageIndex = 1;

    private StageData stageData;                           // 현재 스테이지 데이터
    private readonly List<GraphNode> visitedNodes = new(); // 방문한 노드 목록


    private void Start()
    {
        // 시작 시 스테이지 복원 또는 새로 시작
        stageIndex = GameManager.Instance.stageIndex;

        if (!TryRestoreStage())
        {
            LoadStage(stageIndex);
        }
    }

    // 저장된 상태가 있다면 복원 시도
    private bool TryRestoreStage()
    {
        var gm = GameManager.Instance;

        if (gm.savedStageData != null && !gm.retryFromStart)
        {
            if (gm.stageCleared)
            {
                var lastVisited = gm.savedVisitedNodes.Last();
                bool wasLastColumnNode = gm.savedStageData.columns[^1].Contains(lastVisited); // 보스 노드 방문 여부

                // 보스 노드 클리어면 다음 스테이지
                if (wasLastColumnNode)
                {
                    gm.stageIndex++;
                    gm.stageCleared = false;
                    stageIndex = gm.stageIndex;
                    LoadStage(stageIndex);
                    return true;
                }
            }

            // 스테이지 데이터 복구 (야영지, 이벤트, 인게임에서 돌아 왔을 경우)
            stageData = gm.savedStageData;  // 게임 매니저에서 데이터 불러오기
            visitedNodes.Clear();           
            visitedNodes.AddRange(gm.savedVisitedNodes);
            stageIndex = gm.stageIndex;
            gm.stageCleared = false;

            mapRenderer.Render(stageData, OnNodeClicked);
            mapRenderer.CenterMap();
            mapRenderer.UpdateInteractables(visitedNodes.Last(), visitedNodes);
            return true;
        }

        return false;
    }

    // 노드 클릭 시 스테이지 호출 및 저장
    private void OnNodeClicked(GraphNode clicked)
    {
        visitedNodes.Add(clicked);
        GameManager.Instance.SaveStageState(stageData, visitedNodes, stageIndex);

        // 노드 타입별 씬 전환
        switch (clicked.type)
        {
            case NodeType.NormalBattle:
            case NodeType.EliteBattle:
            case NodeType.Boss:
                GameManager.Instance.currentBattleNode = clicked;
                SceneManager.LoadScene(SceneNameData.InGameScene);
                return;

            case NodeType.Camp:
                SceneManager.LoadScene(SceneNameData.CampScene);
                return;

            case NodeType.RandomEvent:
                SceneManager.LoadScene(SceneNameData.RandomEventScene);
                return;
        }
   
        if (IsLastColumnNode(clicked))
        {
            // 마지막 열(보스노드)의 경우 스테이지 증가
            stageIndex++;
            GameManager.Instance.stageIndex = stageIndex;
        }
        else
        {
            // 중간 노드의 경우 다음 스테이지 연결
            mapRenderer.UpdateInteractables(clicked, visitedNodes);
        }
    }

    // 클릭된 노드가 마지막 열에 있는지 확인
    private bool IsLastColumnNode(GraphNode clicked)
        => stageData.columns[^1].Contains(clicked);


    // 스테이지를 불러오고 맵 초기화
    private void LoadStage(int index)
    {
        // 맵 정보 초기화
        mapRenderer.ClearMap();
        visitedNodes.Clear();

        stageData = RebuildStage(index);
        mapRenderer.Render(stageData, OnNodeClicked);
        mapRenderer.CenterMap();

        DisableAllNodeButtons();
        ActivateStartNode();
    }

    // 스테이지 생성 및 spacing 보정
    private StageData RebuildStage(int index)
    {
        var stage = StageGraphGenerator.Generate(index, spacing);
        AdjustSpacing(stage.columnCount);
        return StageGraphGenerator.Generate(index, spacing);
    }


    // 열 개수에 따라 spacing 조절
    private void AdjustSpacing(int columnCount)
    {
        spacing.x = mapTargetWidth / Mathf.Max(1, columnCount - 1);
    }

    // 모든 노드 비활성화
    private void DisableAllNodeButtons()
    {
        foreach (var column in stageData.columns)
        {
            foreach (var node in column)
            {
                var ui = mapRenderer.nodeUIMap[node];
                var btn = ui.GetComponent<Button>();

                ui.gameObject.SetActive(true);
                btn.enabled = false;
                btn.interactable = false;
                btn.image.color = new Color(1, 1, 1, 0.4f);
            }
        }
    }

    // 시작 지점 노드 활성화
    private void ActivateStartNode()
    {
        var startNode = stageData.columns[0].First();
        visitedNodes.Add(startNode);

        var startUI = mapRenderer.nodeUIMap[startNode];
        var startBtn = startUI.GetComponent<Button>();
        startBtn.enabled = true;
        startBtn.interactable = true;
        startBtn.image.color = Color.white;

        mapRenderer.UpdateInteractables(startNode, visitedNodes);
    }
}
