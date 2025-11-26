using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    StyleDefinition styOnButton;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(styOnButton !=null)
            transform.DOScale(1.08f, 0.15f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (styOnButton != null)
            transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
    }
    public void SetStyle(StyleDefinition sty)
    {
        styOnButton = sty;
    }
}
