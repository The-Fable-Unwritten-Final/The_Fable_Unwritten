using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemPrefab : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemText;

    public void SetItem(Sprite icon, string name, int count)
    {
        itemIcon.sprite = icon;
        itemText.text = $"{name} x{count}";
    }
}
