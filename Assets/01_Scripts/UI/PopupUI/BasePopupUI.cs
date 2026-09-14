using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasePopupUI : MonoBehaviour
{
    public Button backGround;
    private bool isListenerRegistered = false;

    private void OnEnable()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.SFX, 102); // 팝업 활성화 시 사운드
    }
    private void OnDisable()
    {
        // 팝업 비활성화 시 사운드
        SoundManager.Instance.PlaySFX(SoundCategory.SFX, 103);
    }

    private void RegisterBackgroundListener()
    {
        if (backGround != null && !isListenerRegistered)
        {
            // 별도의 설정을 하지 않은 모든 팝업 UI는 뒷 배경을 클릭하면 닫히도록 설정
            backGround.onClick.AddListener(Close);
            isListenerRegistered = true;
        }
    }

    public virtual void Open()
    {
        RegisterBackgroundListener();
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
