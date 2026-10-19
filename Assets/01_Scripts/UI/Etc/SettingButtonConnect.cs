using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingButtonConnect : MonoBehaviour
{
    public void SettingUIManagerOpen()
    {
        if(UIManager.Instance != null)
            UIManager.Instance.ShowPopupByName("PopupUI_Setting");
    }
}
