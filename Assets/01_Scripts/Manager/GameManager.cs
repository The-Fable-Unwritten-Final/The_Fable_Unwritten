using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public StageData savedStageData;
    public List<GraphNode> savedVisitedNodes = new();
    public int stageIndex;

    public int minimumStageIndex = 1;    // 튜토리얼 스테이지
    public bool retryFromStart = false;
    public bool stageCleared = false;

    protected override void Awake()
    {
        base.Awake();
        if (stageIndex <= 0)
            stageIndex = 1;

        if (minimumStageIndex <= 0)
            minimumStageIndex = 1;
    }

    public void SaveStageState(StageData data, List<GraphNode> visited, int index)
    {
        savedStageData = data;
        savedVisitedNodes = new List<GraphNode>(visited);
        stageIndex = index;
    }

    public void ClearStageState()
    {
        savedStageData = null;
        savedVisitedNodes.Clear();
    }
}
