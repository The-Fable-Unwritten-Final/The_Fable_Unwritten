using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockItemPrefab : MonoBehaviour
{
    [System.Serializable]
    public class ResourceSlot
    {
        public Image icon;
        public TextMeshProUGUI countText;
    }

    [Header("전리품 슬롯들 (0~3번 순서대로)")]
    [SerializeField] private List<ResourceSlot> slots;

    [Header("전리품 아이콘들 (Resources/illust0 ~ illust3)")]
    [SerializeField] private List<Sprite> lootIcons;

    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].icon.sprite = lootIcons[i];
            slots[i].countText.text = ProgressDataManager.Instance.itemCounts[i].ToString();
        }
    }
}
