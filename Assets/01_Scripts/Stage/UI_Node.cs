using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Node : MonoBehaviour
{
    private bool isClear = false;
    public bool IsClear => isClear;
    public StageNodeData NodeData;
    

    public void SetUp(StageNodeData data)
    {
        gameObject.SetActive(true);
        isClear = false;
        NodeData = data;        
        GetComponent<Button>().onClick.AddListener(OnClickNode);
    }

    public void SetClear()
    {
        isClear = true;
        gameObject.GetComponent<Button>().enabled = false;
        gameObject.GetComponent<Image>().enabled = false;
        NodeData.row = 0;
        NodeData.col = 0;
    }

    private void OnClickNode()
    {
        Debug.Log($"Clicked node: {NodeData.type}");
    }
}
