using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Main : MonoBehaviour
{
    [SerializeField] Button closeBtn;

    private void Awake()
    {
        closeBtn.onClick.AddListener(ShowTest);
    }

    private void ShowTest()
    {
        UIManager.Instance.ShowPopup<UI_PopupTest>();
    }
}
