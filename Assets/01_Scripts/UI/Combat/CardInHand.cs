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

    // @@@@ 추후 덱 초기화 시점 및 카드 드로우마다 cardDisplay 추가 및 list에 추가 하기.
    public CardDisplay cardDisplay; // 핸드내의 모든 카드들을 관리하는 중앙 스크립트. (UI LineRenderer가 이곳에 존재.) 
    RectTransform rect; // RectTransform 컴포넌트
    Vector3 originalPos; // 원래 위치
    Vector3 targetPos; // 목표 위치
    [SerializeField] CardState cardState;

    enum CardState// 추후 턴 상태와 연계해서 카드의 상태관리. (카드의 상태에 따른 상호작용 가능 여부 설정.)
    {
        None,// 아무런 상호 작용이 불가능한 상태.
        CanMouseOver,// 마우스 오버를 통해 정보 확인 까지만 가능한 상태.
        CanDrag,// 드래그를 통한 사용까지 가능한 상태.
        OnDrag,// 드래그 중인 상태.
        OnUse,// 카드 사용에 성공
    }

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
        if(cardState == CardState.OnDrag) return; // 카드 상태가 OnDrag인 경우에는 원래 위치로 돌아가지 않음.
        rect.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutSine);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        cardDisplay.isOnDrag = true; // 드래그 시작 시 카드 드래그 상태를 true로 설정
        cardDisplay.currentCard = this; // 현재 드래그 중인 카드 설정
        cardDisplay.lineRenderer.gameObject.SetActive(true); // 드래그 중일 때 라인 렌더러 활성화
        cardDisplay.arrowImage.gameObject.SetActive(true); // 드래그 중일 때 화살표 이미지 활성화
        cardState = CardState.OnDrag; // 카드 상태를 OnDrag로 변경
    }

    public void OnDrag(PointerEventData eventData)
    {
        //this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cardDisplay.isOnDrag = false;
        cardState = CardState.None; // 카드 상태를 None으로 초기화
        cardDisplay.lineRenderer.gameObject.SetActive(false); // 드래그 종료 시 라인 렌더러 비활성화
        cardDisplay.arrowImage.gameObject.SetActive(false); // 드래그 종료 시 화살표 이미지 비활성화
        // 드래그 위치 종료의 정보을 통해 사용 성공시 상태 OnUse로 변경
        if(cardDisplay.OnMousepoint(eventData) != null)// 캐릭터 or 몬스터 카드에 드롭을 한 경우 << 일단은 카드 오브젝트의 캐릭터 or 몬스터 판정만 가능. (null이 아니면 캐릭터 or 몬스터)
        {
            cardState = CardState.OnUse; // 카드 상태를 OnUse로 변경
            //카드 사용 로직...
            rect.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutSine);// @@ 임시로 원래 위치로 돌아가기.
        }

        cardDisplay.currentCard = null; // 드래그 종료 시 현재 카드 설정 해제
        rect.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutSine); // 사용 실패시 원래 위치로 돌아가기.
    }

    public void SetOriginalPos()// 덱 최초 세팅 시점, 카드 추가 혹은 감소시 위치 초기화.
    {
        // 원래 위치 설정
        originalPos = rect.anchoredPosition;

        // 카드의 회전에 따른 위로 올라갔을때의 위치 설정.
        float distance = 100f;// 100 만큼 위로 올라감.
        float angle = rect.localEulerAngles.z * Mathf.Deg2Rad;
        Vector2 localUpDir = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));// 로컬 좌표계에서의 각도 변환을 위해 라디안을 통해 삼각함수 사용. (tranform.up 등의 방식은 월드 좌표계 기준의 단위 벡터이기 때문에 로컬 기준의 값을 계산해야 함.)

        targetPos = rect.anchoredPosition + localUpDir * distance;
    }

    // update 에서 매번 조건을 확인해 카드의 상태 변경 메서드 추가하기
    // method1
}

