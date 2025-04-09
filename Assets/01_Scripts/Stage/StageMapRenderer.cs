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
    [SerializeField] GameObject linePrefab;        // 라인 UI 프리팹
    [SerializeField] RectTransform nodesContainer; // 노드 부모 오브젝트
    [SerializeField] RectTransform linesContainer; // 라인 부모 오브젝트

    [Header("Node Icons")]
    [SerializeField] Sprite startIcon, normalIcon, eliteIcon, randomIcon, campIcon, bossIcon; // 노드아이콘 설정

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

                GameObject line = LineDrawer.DrawLine(fromRT, toRT, linesContainer, linePrefab);
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

            // 지나온 노드 버튼 비활성화, 색상 초기화
            if (visited.Contains(node))
            {
                btn.interactable = false;
                btn.enabled = false;
                btn.image.color = Color.white;
            }
            // 진행가능 노드 버튼 활성화, 색상 초기화
            else if (current.nextNodes.Contains(node))
            {
                btn.enabled = true;
                btn.interactable = true;
                btn.image.color = Color.white;
            }
            // 그 외 노드 버튼 비활성화, 색상 흐리게
            else
            {
                btn.enabled = false;
                btn.interactable = false;
                btn.image.color = new Color(1, 1, 1, 0.6f);
            }
        }

        HighlightLines(current, visited);
    }

    /// <summary>
    /// 지나온 라인과 진행 못한 라인 표시해주는 함수
    /// </summary>
    private void HighlightLines(GraphNode current, List<GraphNode> visited)
    {
        foreach (var line in lineInfos)
        {
            var img = line.lineObj.GetComponent<Image>();

            bool isVisitedFrom = visited.Contains(line.from);  // 시작 노드 방문 여부
            bool isVisitedTo = visited.Contains(line.to);      // 도착 노드 방문 여부
            bool isCurrentPath = line.from == current && current.nextNodes.Contains(line.to); //현재 노드에서 진행 할 수 있는 노드 확인 여부

            img.color = (isVisitedFrom && isVisitedTo) || isCurrentPath
                ? new Color(1f, 1f, 1f, 1f)
                : new Color(1f, 1f, 1f, 0.2f);
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

    /// <summary>
    /// 노드 타입에 따라 아이콘 변환
    /// </summary>
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
                return bossIcon;
            default: return null;
        }     
    }
}