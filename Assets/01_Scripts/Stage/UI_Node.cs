using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Node : MonoBehaviour
{
    [SerializeField] Image iconImage;
    public StageNode NodeData;

    public void SetUp(StageNode data)
    {
        NodeData = data;
        GetComponent<Button>().onClick.AddListener(OnClickNode);
    }    

    private void OnClickNode()
    {
        Debug.Log($"Clicked node: {NodeData.type}");
    }
}
