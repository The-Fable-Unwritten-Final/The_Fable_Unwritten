using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupUI_ReturnTitle : BasePopupUI
{
    public void OnClickToTitle()
    {
        CloseAll();
        SceneManager.LoadScene(SceneNameData.SubTitleScene);
    }

    void CloseAll()
    {
        foreach (BasePopupUI popup in UIManager.Instance.popupStack.ToList()) // List로 변환해 Close로 pop을 해서 예외처리
        {
            popup.Close();
        }
    }
}
