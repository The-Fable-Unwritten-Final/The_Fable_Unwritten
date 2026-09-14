using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasePopupUI : MonoBehaviour
{
    public Button backGround;
    private void Awake()
    {
        if (backGround != null)
        {
            // 별도의 설정을 하지 않은 모든 팝업 UI는 뒷 배경을 클릭하면 닫히도록 설정
            backGround.onClick.AddListener(Close);
        }
    }

    public virtual void Open()
    {
        Stack<BasePopupUI> popStack = UIManager.Instance.popupStack;

        if (popStack.Count > 0)
        {
            if(popStack.Peek() == this)
            {
                gameObject.SetActive(false);
                UIManager.Instance.popupStack.Pop();
                return;
            }
        }

        UIManager.Instance.popupStack.Push(this);
        gameObject.SetActive(true);
    }
    /// <summary>
    /// Stack 에서 Pop 해주며 비활성화
    /// </summary>
    public virtual void Close()
    {
        var popStack = UIManager.Instance.popupStack;

        if (popStack.Count > 0 && popStack.Peek() == this)
        {
            popStack.Pop();
        }

        gameObject.SetActive(false);
    }
}
