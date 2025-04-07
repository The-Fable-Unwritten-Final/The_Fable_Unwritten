using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

// 핸드에 소지하고 있는 개별 카드의 스크립트.
public class CardInHand : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    //public CardData card; // 카드 정보
    public GameObject cardPrefab; // 카드 프리팹
    public GameObject cardInHandPrefab; // 핸드 카드 프리팹
    RectTransform rect; // RectTransform 컴포넌트
    Vector3 originalPos; // 원래 위치
    Vector3 targetPos; // 목표 위치

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        SetOriginalPos();// 덱 세팅기능이 추가되면 그쪽으로 이동.
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        rect.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutSine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutSine);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 카드의 위치를 변경합니다.
        this.transform.SetParent(transform.parent);
        this.transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중 카드의 위치를 업데이트합니다.
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료 시 카드의 위치를 원래대로 되돌립니다.
        this.transform.SetParent(transform);
        this.transform.localPosition = Vector3.zero;
    }

    public void SetOriginalPos()// 덱 최초 세팅 시점, 카드 추가 혹은 감소시 위치 초기화.
    {
        // 원래 위치 설정
        originalPos = rect.anchoredPosition;

        // 카드의 회전에 따른 위로 올라갔을때의 위치 설정.
        float distance = 100f;
        float angle = rect.localEulerAngles.z * Mathf.Deg2Rad;
        Vector2 localUpDir = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));// 로컬 좌표계에서의 각도 변환을 위해 라디안을 통해 삼각함수 사용. (tranform.up 등의 방식은 월드 좌표계 기준의 방향 벡터이기 때문에 로컬 기준의 값을 계산해야 함.)

        targetPos = rect.anchoredPosition + localUpDir * distance;
    }
}

