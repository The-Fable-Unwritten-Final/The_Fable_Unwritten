using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Node : MonoBehaviour
{
    [SerializeField] Image iconImage;

    private bool isClear = false;
    public bool IsClear => isClear;
    public StageNode NodeData;
    

    public void SetUp(StageNode data)
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
    }

    private void OnClickNode()
    {
        Debug.Log($"Clicked node: {NodeData.type}");
    }
}
