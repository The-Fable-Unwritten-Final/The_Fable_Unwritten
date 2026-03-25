using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupByName : MonoBehaviour
{
    public void ShowPopupByName(string name)
    {
        UIManager.Instance.ShowPopupByName(name);
    }
}
