using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float targetScale = 1.0f;
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(targetScale, 0.15f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
    }
}
