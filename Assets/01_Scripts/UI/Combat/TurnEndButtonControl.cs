using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TurnEndButtonControl : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnEndButtonText;
    [SerializeField] Button button;
    [SerializeField] Sprite activeSprite;
    [SerializeField] Sprite inactiveSprite;

    Vector2 targetPosition = new Vector2(90, 130); // 버튼의 원래 위치
    Vector2 originalPosition = new Vector2(-140, 130); // 이동할 위치 (예시로 오른쪽으로 400 픽셀 이동)
    Vector2 hiddenPosition = new Vector2(180, 130); // 숨길 위치 (예시로 오른쪽으로 400 픽셀 이동)


    void Start()
    {
        ((RectTransform)transform).anchoredPosition = hiddenPosition; // 시작 시 버튼을 숨긴 위치로 설정

        // 1.8초뒤 originalPosition으로 이동
        ((RectTransform)transform).DOAnchorPos(originalPosition, 1.8f).SetEase(Ease.InOutSine);            
    }
    public void OnTurnEnd()
    {
        // 버튼을 Dotween을 사용해 현재 위치에서 PosX 좌표 400 으로 이동
        ((RectTransform)transform).anchoredPosition = originalPosition; // 원래 위치로 리셋
        ((RectTransform)transform).DOAnchorPosX(originalPosition.x + 240, 0.8f).SetEase(Ease.InOutSine).OnStart(() =>
        {
            if (button != null)
                button.interactable = false;
        })
        .OnComplete(() =>
        {
            GetComponent<Image>().sprite = inactiveSprite; // 버튼 이미지 비활성화 스프라이트로 변경
            if (turnEndButtonText != null)
                turnEndButtonText.gameObject.SetActive(false);
        }); 
    }
    public void OnStartTurn()
    {
        // 버튼을 Dotween을 사용해 현재 위치에서 PosX 좌표 400 으로 이동
        ((RectTransform)transform).anchoredPosition = new Vector2(originalPosition.x + 240, originalPosition.y); // 시작 위치 설정
        ((RectTransform)transform).DOAnchorPosX(originalPosition.x, 0.8f).SetEase(Ease.InOutSine).OnStart(() =>
         {
             GetComponent<Image>().sprite = activeSprite; // 버튼 이미지 활성화 스프라이트로 변경
             if (turnEndButtonText != null)
                 turnEndButtonText.gameObject.SetActive(true);
         })
        .OnComplete(() =>
        {
            if (button != null)
                button.interactable = true;
        });
    }
    public void TurnOffText()
    {
        if (turnEndButtonText != null)
            turnEndButtonText.gameObject.SetActive(false);
    }
    public void TurnOnText()
    {
        if (turnEndButtonText != null)
            turnEndButtonText.gameObject.SetActive(true);
    }

    public void InteractableOff()
    {
        if (button != null)
            button.interactable = false;
    }

    public void InteractableOn()
    {
        if (button != null)
            button.interactable = true;
            
    }
}
