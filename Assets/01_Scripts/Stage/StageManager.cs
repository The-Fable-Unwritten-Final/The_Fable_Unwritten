using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public int stageIndex = 1;
    public Vector2 spacing = new Vector2(300, 200);

    public StageMapRenderer mapRenderer;

    private StageData stageData;

    private List<GraphNode> visitedNodes = new List<GraphNode>();

    void Start()
    {
        LoadStage(stageIndex);
    }

    void OnNodeClicked(GraphNode clicked)
    {
        visitedNodes.Add(clicked);

        if (IsLastColumnNode(clicked))
        {
            stageIndex++;
            LoadStage(stageIndex);
        }
        else
        {
            mapRenderer.UpdateInteractables(clicked, visitedNodes);
        }
    }

    private void LoadStage(int index)
    {
        mapRenderer.ClearMap();
        visitedNodes.Clear(); // 이전 방문 기록 초기화

        stageData = StageGraphGenerator.Generate(index, spacing);
        mapRenderer.Render(stageData, OnNodeClicked);
        mapRenderer.CenterMap();

        // 모든 노드 보이기 + 버튼 비활성화
        foreach (var column in stageData.columns)
            foreach (var node in column)
            {
                var ui = mapRenderer.nodeUIMap[node];
                ui.gameObject.SetActive(true);

                var btn = ui.GetComponent<Button>();
                btn.interactable = false;
                btn.enabled = false;
                btn.image.color = new Color(1, 1, 1, 0.4f); // 흐리게
            }

        // Start 노드만 버튼 활성화
        var startNode = stageData.columns[0].First();
        visitedNodes.Add(startNode);

        var startUI = mapRenderer.nodeUIMap[startNode];
        var startBtn = startUI.GetComponent<Button>();
        startBtn.enabled = true;
        startBtn.interactable = true;
        startBtn.image.color = Color.white;

        // 다음 노드 표시 처리
        mapRenderer.UpdateInteractables(startNode, visitedNodes);
    }

    bool IsLastColumnNode(GraphNode clicked)
    {
        return stageData.columns[stageData.columns.Count - 1].Contains(clicked);
    }


}
