using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 스테이지 흐름을 제어하는 메인 매니저 클래스
/// </summary>
public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")]  
    [SerializeField] Vector2 spacing = new(300, 200);      // 노드 간격

    [Header("References")]
    public StageMapRenderer mapRenderer;                   // StageMapRederer 연결

    private int stageIndex = 1;
    private StageData stageData;                           // 현재 스테이지 데이터
    private readonly List<GraphNode> visitedNodes = new(); // 방문한 노드 목록

    /// <summary>
    /// 시작 시 스테이지 복원 또는 새로 시작
    /// </summary>
    private void Start()
    {
        stageIndex = GameManager.Instance.stageIndex;

        if (!TryRestoreStage())
        {
            LoadStage(stageIndex);
        }
    }

    /// <summary>
    /// 저장된 상태가 있다면 복원 시도
    /// </summary>
    /// <returns>복원 성공 여부</returns>
    private bool TryRestoreStage()
    {
        var gm = GameManager.Instance;

        if (gm.savedStageData != null && !gm.retryFromStart)
        {
            if (gm.stageCleared)
            {
                var lastVisited = gm.savedVisitedNodes.Last();
                bool wasLastColumnNode = gm.savedStageData.columns[^1].Contains(lastVisited);

                if (wasLastColumnNode)
                {
                    gm.stageIndex++;
                    gm.stageCleared = false;
                    stageIndex = gm.stageIndex;
                    LoadStage(stageIndex);
                    return true;
                }
            }

            // 복원 처리
            stageData = gm.savedStageData;
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

    /// <summary>
    /// 노드 클릭 시 스테이지 호출 및 저장
    /// </summary>
    private void OnNodeClicked(GraphNode clicked)
    {
        visitedNodes.Add(clicked);
        GameManager.Instance.SaveStageState(stageData, visitedNodes, stageIndex);

        switch (clicked.type)
        {
            case NodeType.NormalBattle:
            case NodeType.EliteBattle:
            case NodeType.Boss:
                SceneManager.LoadScene("InGameScene");
                return;

            case NodeType.Camp:
                SceneManager.LoadScene("CampScene");
                return;

            case NodeType.RandomEvent:
                SceneManager.LoadScene("RandomEventScene");
                return;
        }

        if (IsLastColumnNode(clicked))
        {
            stageIndex++;
            GameManager.Instance.stageIndex = stageIndex;
            LoadStage(stageIndex);
        }
        else
        {
            mapRenderer.UpdateInteractables(clicked, visitedNodes);
        }
    }

    /// <summary>
    /// 스테이지를 새로 불러오고 맵 초기화
    /// </summary>
    private void LoadStage(int index)
    {
        mapRenderer.ClearMap();
        visitedNodes.Clear();

        stageData = RebuildStage(index);
        mapRenderer.Render(stageData, OnNodeClicked);
        mapRenderer.CenterMap();

        DisableAllNodeButtons();
        ActivateStartNode();
    }

    /// <summary>
    /// 스테이지 생성 및 spacing 보정
    /// </summary>
    private StageData RebuildStage(int index)
    {
        var stage = StageGraphGenerator.Generate(index, spacing);
        AdjustSpacing(stage.columnCount);
        return StageGraphGenerator.Generate(index, spacing);
    }

    /// <summary>
    /// 열 개수에 따라 spacing 조절
    /// </summary>
    private void AdjustSpacing(int columnCount)
    {
        float targetWidth = 1400f;
        spacing.x = targetWidth / Mathf.Max(1, columnCount - 1);
    }

    /// <summary>
    /// 모든 노드 비활성화
    /// </summary>
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

    /// <summary>
    /// 시작 노드를 활성화
    /// </summary>
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

    /// <summary>
    /// 클릭된 노드가 마지막 열에 있는지 확인
    /// </summary>
    private bool IsLastColumnNode(GraphNode clicked)
        => stageData.columns[^1].Contains(clicked);
}
