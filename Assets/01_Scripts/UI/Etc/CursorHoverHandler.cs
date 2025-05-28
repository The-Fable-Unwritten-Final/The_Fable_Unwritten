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
        //Tooltip.Instance.Show(stanceButton.GetDescription());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.customMouseCursor.SetCursorState(CursorState.Idle);
        //Tooltip.Instance.Hide();
    }
}
