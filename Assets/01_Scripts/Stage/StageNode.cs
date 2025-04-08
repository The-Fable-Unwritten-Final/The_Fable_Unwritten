using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageNode : MonoBehaviour
{
    public Image iconImage;
    
    /// <summary>
    /// 타입 확인 후 노드 이미지 설정
    /// </summary>
    public void Setup(NodeType type, Sprite icon)
    {
        iconImage.sprite = icon;
    }
}
