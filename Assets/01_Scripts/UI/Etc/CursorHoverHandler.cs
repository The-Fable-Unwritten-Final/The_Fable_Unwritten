using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.customMouseCursor.SetCursorState(CursorState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.customMouseCursor.SetCursorState(CursorState.Idle);
    }
}
