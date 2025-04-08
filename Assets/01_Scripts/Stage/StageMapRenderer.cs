using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class StageMapRenderer : MonoBehaviour
{
    [Header("Prefabs & UI")]
    public GameObject nodePrefab;
    public GameObject linePrefab;
    public RectTransform nodesContainer;


    [Header("Node Icons")]
    public Sprite startIcon, normalIcon, eliteIcon, randomIcon, campIcon, bossIcon;

    public Dictionary<GraphNode, RectTransform> nodeUIMap = new();
    private readonly List<LineInfo> lineInfos = new();

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

                GameObject line = UILineDrawer.DrawLine(fromRT, toRT, nodesContainer, linePrefab);
                lineInfos.Add(new LineInfo { from = node, to = next, lineObj = line });
            }
        }
    }

    public void HighlightLinesFrom(GraphNode current)
    {
        foreach (var line in lineInfos)
        {
            bool active = line.from == current && current.nextNodes.Contains(line.to);
            line.lineObj.SetActive(active);
        }
    }

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

        // 🔥 여기서 visited 같이 넘겨줌!
        HighlightLinesFrom(current, visited);
    }

    public void HighlightLinesFrom(GraphNode current, List<GraphNode> visited)
    {
        foreach (var line in lineInfos)
        {
            var img = line.lineObj.GetComponent<Image>();

            bool isVisitedFrom = visited.Contains(line.from);
            bool isVisitedTo = visited.Contains(line.to);

            bool isCurrentPath = line.from == current && current.nextNodes.Contains(line.to);

            if (isVisitedFrom && isVisitedTo)
            {
                // 지나온 길
                img.color = new Color(1f, 1f, 1f, 1f);
            }
            else if (isCurrentPath)
            {
                // 현재 노드에서 갈 수 있는 경로
                img.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // 아직 도달하지 못한 경로
                img.color = new Color(1f, 1f, 1f, 0.2f);
            }
        }
    }

    public void ClearMap()
    {
        foreach (var ui in nodeUIMap.Values)
            Destroy(ui.gameObject);
        nodeUIMap.Clear();

        foreach (var line in lineInfos)
            Destroy(line.lineObj);
        lineInfos.Clear();
    }

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