using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;

public class StageMapLoader : MonoBehaviour
{
    [SerializeField] int rows;
    [SerializeField] int cols;
    UI_Node[,] stageNodes;

    private void Awake()
    {
        
    }

    public void Start()
    {
        stageNodes = new UI_Node[rows, cols];

        int index = 0;
        for (int row = 1; row <= rows; row++)
        {
            for (int col = 1; col <= cols; col++)
            {
                Transform child = transform.GetChild(index);
                UI_Node stageNode = child.GetComponent<UI_Node>();
                stageNode.NodeData.row = row;
                stageNode.NodeData.col = col;

                if (stageNode.NodeData.col == 1)
                {
                    stageNode.NodeData.type = NodeTpye.Start;
                }
                else if (stageNode.NodeData.col== 7)
                {
                    stageNode.NodeData.type = NodeTpye.Boss;
                }
                else
                {
                    stageNode.NodeData.type = StageNode.GetRandomType();
                }

                //내부배열은 0부터라 -1
                stageNodes[row - 1, col - 1] = stageNode;
                
                index++;
            }
        }
    }


    private void SetNode()
    {
        if (true)
        {
            
        }
    }


}
