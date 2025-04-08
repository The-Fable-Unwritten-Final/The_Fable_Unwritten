using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public void Render(StageData stage, System.Action<GraphNode> onClick)
    {
        nodeUIMap.Clear();
        lineInfos.Clear();

        foreach (var column in stage.columns)
        {
            foreach (var node in column)
            {
                var go = Instantiate(nodePrefab, nodesContainer);
                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = node.position;
                nodeUIMap[node] = rt;

                var ui = go.GetComponent<StageNode>();
                ui.Setup(node.type, GetIcon(node.type));

                go.GetComponent<Button>().onClick.AddListener(() => onClick(node));
            }
        }

        foreach (var node in nodeUIMap.Keys)
        {
            foreach (var next in node.nextNodes)
            {
                var fromRT = nodeUIMap[node];
                var toRT = nodeUIMap[next];

                GameObject line = UILineDrawer.DrawLine(fromRT, toRT, linesContainer, linePrefab);
                lineInfos.Add(new LineInfo { from = node, to = next, lineObj = line });
            }
        }
    }

    /// <summary>
    /// 현재 노드 기준으로 갈 수 있는 노드만 활성화하고 라인 강조
    /// </summary>
    public void UpdateInteractables(GraphNode current, List<GraphNode> visited)
    {
        foreach (var node in nodeUIMap.Keys)
        {
            var rt = nodeUIMap[node];
            var btn = rt.GetComponent<Button>();

            if (visited.Contains(node))
            {
                btn.interactable = false;
                btn.enabled = false;
                btn.image.color = Color.white;
            }
            else if (current.nextNodes.Contains(node))
            {
                btn.enabled = true;
                btn.interactable = true;
                btn.image.color = Color.white;
            }
            else
            {
                btn.enabled = false;
                btn.interactable = false;
                btn.image.color = new Color(1, 1, 1, 0.4f);
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

            bool isVisitedFrom = visited.Contains(line.from);
            bool isVisitedTo = visited.Contains(line.to);
            bool isCurrentPath = line.from == current && current.nextNodes.Contains(line.to);

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

        foreach (var rt in nodeUIMap.Values)
        {
            Vector2 pos = rt.anchoredPosition;
            min = Vector2.Min(min, pos);
            max = Vector2.Max(max, pos);
        }

        Vector2 center = (min + max) / 2f;
        nodesContainer.anchoredPosition = -center;
    }

    /// <summary>
    /// 노드 타입에 따라 아이콘 변환
    /// </summary>
    private Sprite GetIcon(NodeType type) => type switch
    {
        NodeType.Start => startIcon,
        NodeType.NormalBattle => normalIcon,
        NodeType.EliteBattle => eliteIcon,
        NodeType.RandomEvent => randomIcon,
        NodeType.Camp => campIcon,
        NodeType.Boss => bossIcon,
        _ => null
    };
}