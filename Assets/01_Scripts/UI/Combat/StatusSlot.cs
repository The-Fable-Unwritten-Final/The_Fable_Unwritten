using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;

    public void bind(Sprite icon, int value, bool rotateForNegative = false, bool hideNumber = false)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.rectTransform.localRotation = rotateForNegative
                ? Quaternion.Euler(0f, 0f, 180f)
                : Quaternion.identity;
        }

        if (valueText != null)
        {
            valueText.text = hideNumber ? string.Empty : Mathf.Abs(value).ToString();
        }

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.rectTransform.localRotation = Quaternion.identity;
        }

        if (valueText != null)
        {
            valueText.text = string.Empty;
        }

        gameObject.SetActive(false);
    }

}
