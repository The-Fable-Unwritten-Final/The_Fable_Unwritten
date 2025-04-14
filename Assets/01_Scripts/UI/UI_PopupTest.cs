using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PopupTest : BasePopupUI
{
    [SerializeField] Button closeBtn;

    private void Awake()
    {
        closeBtn.onClick.AddListener(Close);// 이 방식 말고 버튼의 onClick에 Close 메서드를 추가하는 방식으로 변경 해주세요.
    }
}
