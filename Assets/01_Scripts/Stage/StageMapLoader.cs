using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMapLoader : MonoBehaviour
{
    [SerializeField] StageMapData currentStage;
    [SerializeField] Transform nodesBG;
    [SerializeField] GameObject nodePrefap;
    [SerializeField] GameObject linePrefab;

    int currentStageRow;
    int currentStageCol;

    UI_Node[,] stageNodes;
    List<GameObject> lines = new List<GameObject>();

    public void Start()
    {
        SetNode();
    }

    // 스테이지씬 진입시 노드 정렬 매서드
    private void SetNode()
    {
        currentStageRow = currentStage.Row;
        currentStageCol = currentStage.Col;

        stageNodes = new UI_Node[currentStage.Row, currentStage.Col];


        
        for (int row = 1; row <= currentStageRow; row++)
        {
            for (int col = 1; col <= currentStageCol; col++)
            {
                GameObject nodeGO = Instantiate(nodePrefap, nodesBG);
                UI_Node uiNode = nodeGO.GetComponent<UI_Node>();

                StageNode data = new StageNode();                

                data.row = row;
                data.col = col;

                if (col == 1)
                {
                    data.type = NodeTpye.Start;
                }
                else if(col == currentStageCol)
                {
                    data.type = NodeTpye.Boss;
                }
                else
                {
                    data.type = StageNode.GetRandomType();
                }
                
                uiNode.SetUp(data);                

                //내부배열은 0부터라 -1
                stageNodes[row - 1, col - 1] = uiNode;
            }
        }
        if (currentStage.Type == StageType.Middle)
        {
            ClearNode();
        }

        ConnectNodes();
    }

    //불필요한 노드 비활성화
    private void ClearNode()
    {
        List<UI_Node> clearNodes = new List<UI_Node>();

        clearNodes.Add(GetNode(1, 1));
        clearNodes.Add(GetNode(currentStageRow, 1));
        clearNodes.Add(GetNode(1, currentStageCol));
        clearNodes.Add(GetNode(currentStageRow, currentStageCol));

        //중복된 랜덤값 확인으로 HashSet 사용
        HashSet<UI_Node> randomNodes = new HashSet<UI_Node>();

        while (randomNodes.Count < 3)
        {
            // 시작, 끝 열 제외
            int row = Random.Range(1, currentStageRow + 1);
            int col = Random.Range(2, currentStageCol);

            UI_Node node = GetNode(row, col);

            // 고정된 노드와 중복되지 않게 추가
            if (!clearNodes.Contains(node))
            {
                randomNodes.Add(node);
            }
        }

        clearNodes.AddRange(randomNodes);

        // Clear 처리
        foreach (var node in clearNodes)
        {
            node.SetClear();
        }
    }

    private void ConnectNodes()
    {
        for (int col = 0; col < currentStageCol - 1; col++)
        {
            for (int row = 0; row < currentStageRow; row++)
            {
                UI_Node current = stageNodes[row, col];
                if (current.IsClear) continue;

                for (int nextRow = 0; nextRow < currentStageRow; nextRow++)
                {
                    UI_Node next = stageNodes[nextRow, col + 1];
                    if (next.IsClear) continue;

                    if (Mathf.Abs(nextRow - row) <= 1)
                    {
                        DrawLine(current.transform, next.transform);
                    }
                }
            }
        }
    }

    private void DrawLine(Transform from, Transform to)
    {
        GameObject line = Instantiate(linePrefab, nodesBG);
        lines.Add(line);

        RectTransform rt = line.GetComponent<RectTransform>();
        Vector3 start = from.position;
        Vector3 end = to.position;

        Vector3 dir = end - start;
        float length = dir.magnitude;

        rt.sizeDelta = new Vector2(length, 5f); // 5f: 선의 두께
        rt.position = start + dir * 0.5f;
        rt.rotation = Quaternion.FromToRotation(Vector3.right, dir.normalized);
    }

    public UI_Node GetNode(int row, int col)
    {
        return stageNodes[row - 1, col - 1];
    }
}
