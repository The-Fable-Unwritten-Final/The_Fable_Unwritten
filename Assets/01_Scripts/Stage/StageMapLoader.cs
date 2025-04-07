using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageMapLoader : MonoBehaviour
{
    [SerializeField] StageMapData currentStage;
    [SerializeField] Transform nodesBG;
    [SerializeField] GameObject nodePrefap;

    int currentStageRow;
    int currentStageCol;

    UI_Node[,] stageNodes;

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

        List<UI_Node> availableNodes = new();

        for (int row = 1; row <= currentStageRow; row++)
        {
            for (int col = 1; col <= currentStageCol; col++)
            {
                GameObject nodeGO = Instantiate(nodePrefap, nodesBG);
                UI_Node uiNode = nodeGO.GetComponent<UI_Node>();

                StageNodeData data = new StageNodeData();                

                data.row = row;
                data.col = col;

                //1번열 - 시작 포인트, 마지막열 - 보스 포인트, 나머지 랜덤
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
                    data.type = NodeTpye.NormalBattle; // 임시, 나중에 덮어씌움
                    availableNodes.Add(uiNode);
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

        ApplyFixedRandomTypes(availableNodes);

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

    private void ApplyFixedRandomTypes(List<UI_Node> candidates)
    {
        // Clear된 노드는 제외
        List<UI_Node> validNodes = new();
        foreach (var node in candidates)
        {
            if (!node.IsClear)
                validNodes.Add(node);
        }

        // 총 12개가 되어야 함
        if (validNodes.Count < 12)
        {
            Debug.LogError("타입을 지정할 수 있는 노드가 부족합니다!");
            return;
        }

        List<NodeTpye> nodeTypes = new();

        nodeTypes.AddRange(Enumerable.Repeat(NodeTpye.NormalBattle, 6));
        nodeTypes.AddRange(Enumerable.Repeat(NodeTpye.EliteBattle, 1));
        nodeTypes.AddRange(Enumerable.Repeat(NodeTpye.RandomEvent, 3));
        nodeTypes.AddRange(Enumerable.Repeat(NodeTpye.Camp, 2));

        // 셔플
        for (int i = 0; i < nodeTypes.Count; i++)
        {
            NodeTpye temp = nodeTypes[i];
            int randIndex = Random.Range(i, nodeTypes.Count);
            nodeTypes[i] = nodeTypes[randIndex];
            nodeTypes[randIndex] = temp;
        }

        // 타입 배정
        for (int i = 0; i < nodeTypes.Count; i++)
        {
            validNodes[i].NodeData.type = nodeTypes[i];
        }
    }

    private UI_Node GetNode(int row, int col)
    {
        return stageNodes[row - 1, col - 1];
    }
}
