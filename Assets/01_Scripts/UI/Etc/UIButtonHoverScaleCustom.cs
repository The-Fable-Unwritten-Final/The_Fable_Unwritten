using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIButtonHoverScaleCustom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float targetScale = 1.0f;
    public float originalScale = 1.0f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(targetScale, 0.15f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);
    }
}
