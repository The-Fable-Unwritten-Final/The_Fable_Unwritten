using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")]
    public int stageIndex = 1;
    public Vector2 spacing = new(300, 200);

    [Header("References")]
    public StageMapRenderer mapRenderer;

    private StageData stageData;
    private readonly List<GraphNode> visitedNodes = new();

    void Start() => LoadStage(stageIndex);

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
        visitedNodes.Clear();

        // 스테이지 데이터 생성
        stageData = StageGraphGenerator.Generate(index, spacing);

        // 🔧 spacing.x를 열 수에 맞게 자동 조정
        float targetWidth = 1400f;
        spacing.x = targetWidth / (stageData.columnCount - 1);

        // 다시 생성 (spacing 조정 후)
        stageData = StageGraphGenerator.Generate(index, spacing);

        mapRenderer.Render(stageData, OnNodeClicked);
        mapRenderer.CenterMap();

        DisableAllNodeButtons();
        ActivateStartNode();
    }

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

    private bool IsLastColumnNode(GraphNode clicked)
        => stageData.columns[^1].Contains(clicked);
}
