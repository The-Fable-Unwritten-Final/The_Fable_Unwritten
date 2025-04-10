using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupUI_Setting : BasePopupUI
{
    [SerializeField] Button exitBtn;

    private void Awake()
    {
        exitBtn.onClick.AddListener(Close);
    }

}
