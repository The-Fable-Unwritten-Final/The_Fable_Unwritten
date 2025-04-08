using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMapRenderer : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject linePrefab;
    public RectTransform mapParent;

    public Sprite startIcon, normalIcon, eliteIcon, randomIcon, campIcon, bossIcon;

    public Dictionary<GraphNode, RectTransform> nodeUIMap = new();

    public void Render(StageData stage, System.Action<GraphNode> onClick)
    {
        nodeUIMap.Clear();

        foreach (var column in stage.columns)
        {
            foreach (var node in column)
            {
                var go = Instantiate(nodePrefab, mapParent);
                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = node.position;
                nodeUIMap[node] = rt;

                var ui = go.GetComponent<StageNodeUI>();
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
                UILineDrawer.DrawLine(fromRT, toRT, mapParent, linePrefab);
            }
        }
    }

    public void ShowColumn(int columnIndex, StageData stage)
    {
        foreach (var column in stage.columns)
        {
            foreach (var node in column)
            {
                bool active = node.columnIndex == columnIndex;
                if (nodeUIMap.ContainsKey(node))
                    nodeUIMap[node].gameObject.SetActive(active);
            }
        }
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
    public void SetInteractableNodes(GraphNode current, StageData stage)
    {
        // 전체 노드 비활성화
        foreach (var col in stage.columns)
        {
            foreach (var node in col)
            {
                if (nodeUIMap.TryGetValue(node, out RectTransform rt))
                {
                    var button = rt.GetComponent<Button>();
                    button.interactable = false;

                    // (선택) 흐리게 보이게 하고 싶으면:
                    button.image.color = new Color(1, 1, 1, 0.4f);
                }
            }
        }

        // 다음 열 노드만 다시 활성화
        foreach (var next in current.nextNodes)
        {
            if (nodeUIMap.TryGetValue(next, out RectTransform rt))
            {
                var button = rt.GetComponent<Button>();
                button.interactable = true;

                // (선택) 밝게 복원
                button.image.color = Color.white;
            }
        }
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

        // 여기서 this.mapParent 사용!
        mapParent.anchoredPosition = -center;
    }

    public void ShowNextOnly(GraphNode current)
    {
        foreach (var node in nodeUIMap.Keys)
        {
            var rt = nodeUIMap[node];
            var button = rt.GetComponent<Button>();

            if (node == current)
            {
                // 현재 클릭한 노드는 비활성화
                button.interactable = false;
            }
            else if (current.nextNodes.Contains(node))
            {
                // 다음 연결 노드만 활성화
                rt.gameObject.SetActive(true);
                button.interactable = true;
                button.image.color = Color.white;
            }
            else
            {
                // 나머지 전부 비활성화
                button.interactable = false;
            }
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
                // ✅ 지나온 경로는 버튼 꺼버리기
                btn.interactable = false;
                btn.enabled = false;
                btn.image.color = Color.white; // 흐림 제거
            }
            else if (current.nextNodes.Contains(node))
            {
                // ✅ 다음 선택 가능한 노드만 켜기
                btn.enabled = true;
                btn.interactable = true;
                btn.image.color = Color.white;
            }
            else
            {
                // ✅ 나머지 노드는 클릭 불가
                btn.enabled = false;
                btn.interactable = false;
                btn.image.color = new Color(1, 1, 1, 0.4f); // 흐리게
            }
        }
    }
    public void ClearMap()
    {
        foreach (var ui in nodeUIMap.Values)
        {
            Destroy(ui.gameObject);
        }
        nodeUIMap.Clear();
    }
}