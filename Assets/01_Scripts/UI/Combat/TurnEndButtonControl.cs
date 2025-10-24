using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnEndButtonControl : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnEndButtonText;
    [SerializeField] Button button;

    public void TurnOffText()
    {
        if (turnEndButtonText != null)
            turnEndButtonText.gameObject.SetActive(false);
    }
    public void TurnOnText()
    {
        if (turnEndButtonText != null)
            turnEndButtonText.gameObject.SetActive(true);
    }

    public void InteractableOff()
    {
        if (button != null)
            button.interactable = false;
    }

    public void InteractableOn()
    {
        if (button != null)
            button.interactable = true;
            
    }
}
