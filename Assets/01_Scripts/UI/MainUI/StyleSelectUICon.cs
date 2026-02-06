using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StyleSelectUICon : MonoBehaviour
{
    [SerializeField] RectTransform mainUIRect;
    [SerializeField] RectTransform styleEditUIRect;
    [SerializeField] RectTransform fixedUIRect;


    public void OnClickStyleEditButton(float transitionTime)
    {
        mainUIRect.DOAnchorPosY(1080, transitionTime).SetEase(Ease.OutCubic);
        styleEditUIRect.DOAnchorPosY(0, transitionTime).SetEase(Ease.OutCubic);
        fixedUIRect.DOAnchorPosY(1080, transitionTime).SetEase(Ease.OutCubic);
    }

    public void OnClickStyleCloseButton(float transitionTime)
    {
        mainUIRect.DOAnchorPosY(0, transitionTime).SetEase(Ease.OutCubic);
        styleEditUIRect.DOAnchorPosY(-1080, transitionTime).SetEase(Ease.OutCubic);
        fixedUIRect.DOAnchorPosY(0, transitionTime).SetEase(Ease.OutCubic);
    }
}
