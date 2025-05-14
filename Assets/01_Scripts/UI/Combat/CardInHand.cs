using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

// 핸드에 소지하고 있는 개별 카드의 스크립트.
public class CardInHand : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler
{
    public CardModel cardData; // 카드 정보
    public CardDisplay cardDisplay; // 핸드내의 모든 카드들을 관리하는 중앙 스크립트. ( + UI LineRenderer가 이곳에 존재.) 
    RectTransform rect; // RectTransform 컴포넌트
    public Vector2 originalPos; // 원래 위치
    Vector3 targetPos; // 목표 위치
    [SerializeField] CardState cardState;
    [Header("UI Info")]
    [SerializeField] Image cardFrame;
    [SerializeField] Image cardImage; // 카드 이미지
    [SerializeField] Image cardTypeImage; // 카드 타입 이미지
    [SerializeField] Image cardCharImage; // 카드 캐릭터 이미지
    [SerializeField] TextMeshProUGUI cardCost; // 카드 코스트
    [SerializeField] TextMeshProUGUI cardName; // 카드 이름
    [SerializeField] TextMeshProUGUI cardDescription; // 카드 설명

    public enum CardState// 추후 턴 상태와 연계해서 카드의 상태관리. (카드의 상태에 따른 상호작용 가능 여부 설정.)
    {
        None,// 아무런 상호 작용이 불가능한 상태. (각종 상태들의 중간 거쳐가는 단계)
        CanMouseOver,// 마우스 오버를 통해 정보 확인 까지만 가능한 상태. (코스트가 부족하거나, 플레이어 턴이 아닐때 모든 카드 상태) (플레이어 턴 == turnState.PlayerTurn)
        CanDrag,// 드래그를 통한 사용까지 가능한 상태. (코스트도 충분하고, 플레이어의 턴일때) 
        OnDrag,// 드래그 중인 상태.
        OnUse,// 카드 사용에 성공
        CanDiscard,// 카드 버리기 선택 가능 상태. (UI에서 카드 버리기 선택 시 true로 설정됨)
        OnDiscard,// 카드 버리기 선택된 상태.
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        SetOriginalPos();// 덱 세팅기능이 추가되면 그쪽으로 이동.
    }
    private void OnDestroy()
    {
        StopAllCoroutines(); // 카드가 파괴될 때 모든 코루틴 정지
    }

    public void OnPointerClick(PointerEventData eventData)// 카드 버리기 관련 상호작용 클릭
    {
        CardDiscardController con = GameManager.Instance.cardDiscardController;

        if(cardState == CardState.CanDiscard)
        {
            if(con.isAll )// 전체 카드 버리기인 경우, 카드 버리기 카운트가 0 이하인 경우 리턴
            {
                if(con.ReqTotalCount <= 0) return;
                else
                {

                }
            }
            else
            {
                switch (cardData.characterClass)
                {
                    case CharacterClass.Sophia:
                        if (con.ReqSophiaCount <= 0) return;
                        break;

                    case CharacterClass.Kayla:
                        if (con.ReqKylaCount <= 0) return;
                        break;

                    case CharacterClass.Leon:
                        if (con.ReqLeonCount <= 0) return;
                        break;

                    default:
                        Debug.LogError($"[CardInHand] 카드 클릭 시 오류 발생. 카드 이름: {cardData.cardName}");
                        break;
                }
            }

            con.AddDiscardCard(this);// 카드 버리기 UI에 카드 정보 전달.
            cardFrame.color = Color.yellow; // 임시 색상 변경          
        }
        else if(cardState == CardState.OnDiscard)
        {
            con.RemoveDiscardCard(this);// 카드 버리기 UI에서 카드 정보 제거.
            cardFrame.color = Color.white; // 임시 색상 변경
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(cardState == CardState.None) return; // 상태가 None인 경우 상호작용 불가능

        cardDisplay.currentCard = this;// 현재 카드 설정.
        transform.SetAsLastSibling();// 카드가 가장 위에 오도록 설정

        rect.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutSine);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if(cardState == CardState.OnDrag) return; // 카드 상태가 OnDrag인 경우에는 원래 위치로 돌아가지 않음.
        cardDisplay.currentCard = null;// 현재 카드 설정 해제.
        ResetSiblingIndex();// List의 순서에 맞게 원래 위치로 돌아가기.
        rect.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutSine);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardState != CardState.CanDrag) return; // 카드 상태가 CanDrag가 아닌 경우 드래그 불가능

        cardDisplay.isOnDrag = true; // 드래그 시작 시 카드 드래그 상태를 true로 설정

        cardDisplay.currentCard = this; // 현재 드래그 중인 카드 설정
        transform.SetAsLastSibling();// 카드가 가장 위에 오도록 설정

        cardDisplay.lineRenderer.gameObject.SetActive(true); // 드래그 중일 때 라인 렌더러 활성화
        cardDisplay.arrowImage.gameObject.SetActive(true); // 드래그 중일 때 화살표 이미지 활성화
        cardState = CardState.OnDrag; // 카드 상태를 OnDrag로 변경
    }
    public void OnDrag(PointerEventData eventData)
    {
        //this.transform.position = eventData.position;
        if(this.rect.anchoredPosition == originalPos)
            rect.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutSine);// 드래그 할때 카드가 위로 안올라올 경우의 후처리.
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (cardState != CardState.OnDrag) return; // 카드 상태가 OnDrag가 아닌 경우 해당 메서드 실행하지 않음

        cardDisplay.isOnDrag = false;
        cardState = CardState.None; // 카드 상태를 None으로 초기화
        cardDisplay.lineRenderer.gameObject.SetActive(false); // 드래그 종료 시 라인 렌더러 비활성화
        cardDisplay.arrowImage.gameObject.SetActive(false); // 드래그 종료 시 화살표 이미지 비활성화
        // 드래그 위치 종료의 정보을 통해 사용 성공시 상태 OnUse로 변경
        cardDisplay.OnMousepoint(eventData); // 드래그 종료 시의 해당 위치를 확인해 상호작용 여부 확인
        rect.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutSine); // 원래 위치로 돌아가기.
    }

    public void SetCardData(CardModel card)// 카드 데이터 설정
    {
        cardData = card;
        UpdateCardImage();// 카드 이미지 업데이트
        UpdatCardInfo();// 카드 정보 업데이트
    }
    public void UpdateCardImage()//카드의 일러스트,사용캐릭터,카드타입
    {
        if(cardData.illustration == null)
            Debug.LogError($"[CardInHand] 카드 일러스트가 설정되지 않았습니다. 카드 이름: {cardData.cardName}");// 카드 데이터 SO쪽에서 resources 파일을 통해 이미지 업데이트를 하지 못했을때.

        cardImage.sprite = cardData.illustration; // 카드 이미지 설정
        cardTypeImage.sprite = cardData.cardType; // 카드 타입 이미지 설정
        cardCharImage.sprite = cardData.chClass; // 카드 캐릭터 이미지 설정
    }
    /// <summary>
    /// 카드(본인의) description, cost, name 설명 업데이트.
    /// </summary>
    public void UpdatCardInfo()// 외부에서 해당 메서드 for문 으로 묶어서 action에 구독하여 사용하기에, 기능 분리.
    {
        cardName.text = cardData.cardName; // 카드 이름 설정
        cardCost.text = cardData.GetEffectiveCost().ToString(); // 카드 코스트 설정


        var battleFlow = GameManager.Instance.turnController.battleFlow;
        IStatusReceiver caster = battleFlow.playerParty.Find(p => p.ChClass == cardData.characterClass);

        cardDescription.text = cardData.GetFormattedCardText(caster);// 카드 설명 설정
    }
    public void SetOriginalPos()// 덱 최초 세팅 시점, 카드 추가 혹은 감소시 위치 초기화.
    {
        // 원래 위치 설정
        originalPos = rect.anchoredPosition;

        // 카드의 회전에 따른 위로 올라갔을때의 위치 설정.
        float distance = 150f;// 150 만큼 위로 올라감.
        float angle = rect.localEulerAngles.z * Mathf.Deg2Rad;
        Vector2 localUpDir = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));// 로컬 좌표계에서의 각도 변환을 위해 라디안을 통해 삼각함수 사용. (tranform.up 등의 방식은 월드 좌표계 기준의 단위 벡터이기 때문에 로컬 기준의 값을 계산해야 함.)

        targetPos = rect.anchoredPosition + localUpDir * distance;
    }
    public void SetTargetPos(Vector2 tPos ,float tAngle)// 핸드의 카드에 변동사항이 생겨 움직이는 도중, 핸드의 카드에 접근시 정상적인 동작을 위해 필요한 메서드.
    {
        float distance = 150f;// 150 만큼 위로 올라감.
        float angle = tAngle * Mathf.Deg2Rad;
        Vector2 localUpDir = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));

        targetPos = tPos + localUpDir * distance;
    }
    public void ResetSiblingIndex()// 카드 사용이 실패 했을때 등 원래 위치로 돌아가야 할때 호출.
    {
        transform.SetSiblingIndex(cardDisplay.GetCardIndex(this));
    }
    public void SetCardState(CardState state)
    {
        cardState = state; // 카드 상태 설정
    }
    public CardState GetCardState()
    {
        return cardState; // 카드 상태 가져오기
    }
    public void OnCardMoveCouroutine()
    {
        StartCoroutine(OnCardMove());// 카드 이동 애니메이션을 위한 코루틴 시작.
    }
    IEnumerator OnCardMove()// 카드 위치 재설정
    {
        // 카드 이동 애니메이션을 위한 상호작용 제한 코루틴
        if(cardState == CardState.CanDiscard) yield break; // 카드 상태가 OnDrag인 경우 코루틴 종료

        SetCardState(CardInHand.CardState.None);// 카드가 움직이는 도중에는 상호작용 제한.
        yield return new WaitForSeconds(0.2f);

        if (cardData.IsUsable(GameManager.Instance.turnController.battleFlow.currentMana))
        {
            Debug.Log($"[CardInHand] 카드 사용 가능 상태로 변경. 카드 이름: {cardData.cardName}");
            SetCardState(CardInHand.CardState.CanDrag);
        }
        else
        {
            SetCardState(CardInHand.CardState.CanMouseOver);
        }
    }

    // 핸드 카드 선택 시 출력 사운드
    public void OnPointerDown(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(SoundCategory.UI, 4);
    }
}

