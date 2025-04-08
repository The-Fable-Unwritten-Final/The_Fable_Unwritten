using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageNodeUI : MonoBehaviour
{
    public Image iconImage;

    public void Setup(NodeType type, Sprite icon)
    {
        iconImage.sprite = icon;
    }
}
