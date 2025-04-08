using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")]
    private int stageIndex = 1;
    [SerializeField] Vector2 spacing = new(300, 200); // 노드 간격

    [Header("References")]
    public StageMapRenderer mapRenderer;              // StageMapRederer 연결

    private StageData stageData;                           // 현재 스테이지 데이터
    private readonly List<GraphNode> visitedNodes = new(); // 방문한 노드 목록

    /// <summary>
    /// 시작 시 현재 스테이지 로드
    /// </summary>
    void Start() => LoadStage(stageIndex);

    /// <summary>
    /// 노드 클릭 시 호출
    /// </summary>
    void OnNodeClicked(GraphNode clicked)
    {
        visitedNodes.Add(clicked);

        if (IsLastColumnNode(clicked))
        {
            // 마지막 열이면 다음 스테이지
            stageIndex++;
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

        // 초기 스테이지 생성
        stageData = StageGraphGenerator.Generate(index, spacing);

        // 열 수에 맞게 간격 조정
        float targetWidth = 1400f;
        spacing.x = targetWidth / (stageData.columnCount - 1);

        // 조정 후 재생성
        stageData = StageGraphGenerator.Generate(index, spacing);

        mapRenderer.Render(stageData, OnNodeClicked);
        mapRenderer.CenterMap();

        DisableAllNodeButtons();
        ActivateStartNode();
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
