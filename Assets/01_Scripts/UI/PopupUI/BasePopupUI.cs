using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePopupUI : MonoBehaviour
{
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
        UIManager.Instance.popupStack.Pop();
        gameObject.SetActive(false);
    }
}
