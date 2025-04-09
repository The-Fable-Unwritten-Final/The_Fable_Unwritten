using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PopupTest : BasePopupUI
{
    [SerializeField] Button closeBtn;

    private void Awake()
    {
        closeBtn.onClick.AddListener(Close);
    }
}
