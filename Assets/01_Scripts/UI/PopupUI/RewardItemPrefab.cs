using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemPrefab : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemText;

    public void SetItem(Sprite icon, int count)
    {
        itemIcon.sprite = icon;
        itemText.text = $" x{count}";
    }
}
