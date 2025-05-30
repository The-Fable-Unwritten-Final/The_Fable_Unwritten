using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class CursorHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private StanceOptionButton stanceButton;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.customMouseCursor.SetCursorState(CursorState.Hover);
        if (stanceButton != null)
        {
            string desc = stanceButton.GetDescription();
            ToolTip.Show(desc, Input.mousePosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.customMouseCursor.SetCursorState(CursorState.Idle);
        ToolTip.Hide();
    }
}
