using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class StatusSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private List<StatusTooltipTrigger> triggerText;

    public void Bind(string keywordType, Sprite icon, int value, bool rotateForNegative = false, bool hideNumber = false)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.rectTransform.localRotation = rotateForNegative
                ? Quaternion.Euler(0f, 0f, 180f)
                : Quaternion.identity;
        }

        if (valueText != null)
        {
            valueText.text = hideNumber ? string.Empty : Mathf.Abs(value).ToString();
        }

        if (triggerText != null)
        {
            foreach (var trigger in triggerText)
            {
                if (trigger != null)
                    trigger.SetKeyword(keywordType);
            }
        }

        gameObject.SetActive(true);
    }
    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            iconImage.rectTransform.localRotation = Quaternion.identity;
        }

        if (valueText != null)
            valueText.text = string.Empty;

        if (triggerText != null)
        {
            foreach (var trigger in triggerText)
            {
                if (trigger != null)
                    trigger.SetKeyword(null);
            }
        }

        gameObject.SetActive(false);
    }

}
