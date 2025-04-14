using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePopupUI : MonoBehaviour
{
    public virtual void Open()
    {
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
