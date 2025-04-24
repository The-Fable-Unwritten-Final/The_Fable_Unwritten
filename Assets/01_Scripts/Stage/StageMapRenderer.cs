using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 노드와 연결선을 UI에 표시해주는 클래스
/// </summary>
public class StageMapRenderer : MonoBehaviour
{
    [Header("Prefabs & UI")]
    [SerializeField] GameObject nodePrefab;        // 노드 UI 프리팹
    [SerializeField] GameObject lineBasicPrefab;        // 라인 UI 프리팹
    [SerializeField] private Sprite lineBasicSprite;    // 기본 라인 Sprite
    [SerializeField] private Sprite lineCompleteSprite; // 지나온 라인 Sprite
    [SerializeField] RectTransform nodesContainer; // 노드 부모 오브젝트
    [SerializeField] RectTransform linesContainer; // 라인 부모 오브젝트

    [Header("Node Icons")]
    [SerializeField] Sprite startIcon, normalIcon, eliteIcon, randomIcon, campIcon; // 노드아이콘 설정
    [SerializeField] private Sprite[] bossStageIcons; // Stage2 = 0, Stage3 = 1, Stage4 = 2, Stage5 = 3

    [Header("숨길 UI들")]
    [SerializeField] private GameObject[] hideDuringDialogue;

    public GameObject[] GetUIToHideDuringDialogue() => hideDuringDialogue;

    public Dictionary<GraphNode, RectTransform> nodeUIMap = new();
    private readonly List<LineInfo> lineInfos = new();

    /// <summary>
    /// 스테이지 노드 및 연결선을 UI에 표시
    /// </summary>
    public void Render(StageData stage, Action<GraphNode> onClick)
    {
        nodeUIMap.Clear();
        lineInfos.Clear();

        foreach (var column in stage.columns)
        {
            foreach (var node in column)
            {
                var go = Instantiate(nodePrefab, nodesContainer);  // 해당 노드 UI 생성
                var rt = go.GetComponent<RectTransform>();        
                rt.anchoredPosition = node.position;               // 해당 노드 위치 설정
                nodeUIMap[node] = rt;                              // 해당 노드의 UI 위치 저장 (라인 이어주기 위해)

                var ui = go.GetComponent<StageNode>();
                ui.Setup(node.type, GetIcon(node.type));           // 해당 노드 이미지 설정

                go.GetComponent<Button>().onClick.AddListener(() => onClick(node)); // 해당 노드 버튼 이벤트 연결
            }
        }

        //모든 노드에 대해 연결 된 노드들 정보 확인 후 라인 그려주기
        foreach (var node in nodeUIMap.Keys)
        {
            foreach (var next in node.nextNodes)
            {
                var fromRT = nodeUIMap[node];
                var toRT = nodeUIMap[next];

                GameObject line = LineDrawer.DrawLine(fromRT, toRT, linesContainer, lineBasicPrefab);
                lineInfos.Add(new LineInfo { from = node, to = next, lineObj = line });
            }
        }
    }

    /// <summary>
    /// 게임 진행 중 노드 상태 관리 매서드
    /// </summary>
    public void UpdateInteractables(GraphNode current, List<GraphNode> visited)
    {
        // 
        foreach (var node in nodeUIMap.Keys)
        {
            var rt = nodeUIMap[node];
            var btn = rt.GetComponent<Button>();
            var stageNode = rt.GetComponent<StageNode>();

            // 지나온 노드 버튼 비활성화, 색상 초기화
            if (visited.Contains(node))
            {
                btn.interactable = false;
                btn.enabled = false;
                btn.image.color = Color.white;
                stageNode.StopPulse();
            }
            // 진행가능 노드 버튼 활성화, 색상 초기화
            else if (current.nextNodes.Contains(node))
            {
                btn.enabled = true;
                btn.interactable = true;
                btn.image.color = Color.white;
                stageNode.PlayPulse();
            }
            // 그 외 노드 버튼 비활성화, 색상 흐리게
            else
            {
                btn.enabled = false;
                btn.interactable = false;
                btn.image.color = new Color(1, 1, 1, 0.6f);
                stageNode.StopPulse();
            }
        }

        HighlightLines(current, visited);
    }

    // 지나온 라인과 진행 못한 라인 표시해주는 함수
    private void HighlightLines(GraphNode current, List<GraphNode> visited)
    {
        foreach (var info in lineInfos)
        {
            var img = info.lineObj.GetComponent<Image>();

            bool isVisitedFrom = visited.Contains(info.from);
            bool isVisitedTo = visited.Contains(info.to);
            bool isCompletePath = isVisitedFrom && isVisitedTo;

            img.sprite = isCompletePath ? lineCompleteSprite : lineBasicSprite;
            img.color = Color.white;
        }
    }

    /// <summary>
    /// 기존 맵의 노드 및 라인들 제거 하는 매서드
    /// </summary>
    public void ClearMap()
    {
        foreach (var ui in nodeUIMap.Values)
            Destroy(ui.gameObject);
        nodeUIMap.Clear();

        foreach (var line in lineInfos)
            Destroy(line.lineObj);
        lineInfos.Clear();
    }

    /// <summary>
    /// 맵 중심이 화명 중앙에 오도록 정렬하는 매서드
    /// </summary>
    public void CenterMap()
    {
        if (nodeUIMap.Count == 0) return;

        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;

        // 노드들의 최소값, 최대값 확인
        foreach (var rt in nodeUIMap.Values)
        {
            Vector2 pos = rt.anchoredPosition;
            min = Vector2.Min(min, pos); // 가장 왼쪽 위
            max = Vector2.Max(max, pos); // 가장 오르쪽 아래
        }

        Vector2 center = (min + max) / 2f;
        nodesContainer.anchoredPosition = -center; // 노드 부모 위치 이동 
    }

    // 노드 타입에 따라 아이콘 변환
    private Sprite GetIcon(NodeType type)
    {
        switch (type)
        {
            case NodeType.Start:
                return startIcon;
            case NodeType.NormalBattle:
                return normalIcon;
            case NodeType.EliteBattle: 
                return eliteIcon;
            case NodeType.RandomEvent:
                return randomIcon;
            case NodeType.Camp:
                return campIcon;
            case NodeType.Boss:
                return GetBossIcon(GameManager.Instance.StageSetting.StageIndex);
            default: return null;
        }     
    }

    private Sprite GetBossIcon(int stageIndex)
    {
        if (stageIndex < 2) return null;

        int bossIndex = stageIndex - 2; // Stage 2 -> 0, Stage 5 -> 3
        
        return bossStageIcons[bossIndex];
    }
}