using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using static CardEffectVisualizer;

// 핸드에 소지하고 있는 개별 카드의 스크립트.
public class CardInHand : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler
{
    public CardModel cardData; // 카드 정보
    public CardDisplay cardDisplay; // 핸드내의 모든 카드들을 관리하는 중앙 스크립트. ( + UI LineRenderer가 이곳에 존재.) 
    public CardEffectVisualizer effectVisualizer; // 카드 효과 비주얼을 위한 스크립트 
    RectTransform rect; // RectTransform 컴포넌트
    public Vector2 originalPos; // 원래 위치
    public Vector2 targetPos; // 목표 위치
    [SerializeField] CardState cardState;
    [Header("UI Info")]
    [SerializeField] Image cardFrame;
    [SerializeField] Image cardImage; // 카드 이미지
    [SerializeField] Image cardCostImage; // 카드 코스트 이미지
    [SerializeField] Image cardTypeImage; // 카드 타입 이미지
    [SerializeField] Image cardCharImage; // 카드 캐릭터 이미지
    [SerializeField] Image shade1;
    [SerializeField] Image shade2; 
    [SerializeField] TextMeshProUGUI cardCost; // 카드 코스트
    [SerializeField] TextMeshProUGUI cardName; // 카드 이름
    [SerializeField] TextMeshProUGUI cardDescription; // 카드 설명
    [Header("UI For FX")]
    [SerializeField] Image frameCover;
    [SerializeField] Image illustCover;

    public bool isPointerOver = false; // 마우스 포인터가 카드 위에 있는지 여부
    bool cancelDrag = false; // 포인터 이벤트의 OnDrag를 조건에 맞춰 직접 끊어주기 위한 변수.

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
        
        // UIParticle 정리
        if (effectVisualizer != null)
        {
            if (effectVisualizer.useCardFXParticle != null)
            {
                effectVisualizer.useCardFXParticle.Clear();
                effectVisualizer.useCardFXParticle.Stop();
            }
            
            if (effectVisualizer.useEnhancedCardFXParticle != null)
            {
                effectVisualizer.useEnhancedCardFXParticle.Clear();
                effectVisualizer.useEnhancedCardFXParticle.Stop();
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        // 카드 버리기 관련 클릭 상호작용
        CardDiscardController con = GameManager.Instance.cardDiscardController;

        if(cardState == CardState.CanDiscard)
        {
            if(con.isAll )// 전체 카드 버리기인 경우, 카드 버리기 카운트가 0 이하인 경우 리턴
            {
                if(con.ReqTotalCount <= 0) return;
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
        // cardDisplay가 없으면 (정비 UI 등) 상호작용 불가능
        if (cardDisplay == null) return;

        if(!cardDisplay.deckInitComplete) return; // 덱 이닛이 완료되지 않았을 경우 상호작용 불가능
        
        // 드래그 중인 다른 카드가 있으면 이 카드 상호작용 불가능
        if (cardDisplay.isOnDrag && cardDisplay.currentCard != this) return;
        
        isPointerOver = true; // 마우스 포인터가 카드 위에 있는 상태로 설정
        
        if (GameManager.Instance == null || GameManager.Instance.turnController == null) return;
        if (GameManager.Instance.turnController.onAction) return; // 행동 중일 경우 상호작용 불가능
        if(cardState == CardState.None) return; // 상태가 None인 경우 상호작용 불가능

        // 드래그 중이 아닐 때만 currentCard 업데이트
        if (!cardDisplay.isOnDrag)
        {
            cardDisplay.currentCard = this;// 현재 카드 설정.
        }

        transform.SetAsLastSibling();// 카드가 가장 위에 오도록 설정

        rect.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutSine);

        // SFX 출력
        SoundManager.Instance.PlaySFX(SoundCategory.SFX, 104);
        
        // 카드의 사용 가능 타겟 표시
        cardDisplay.TargetArrowDisplay();
        
        // 연계 가능한 카드들을 canchain으로
        cardDisplay.CheckCanChain();

        if (effectVisualizer != null)
        {
            if (effectVisualizer.currentState == CardVisualState.Chain)// 카드의 상태가 Chain인 경우 이펙트 변경 취소 (빨간색 유지)
            {
                effectVisualizer.ApplyVisualState(CardVisualState.ReadyOnChain); // 카드의 상태를 ReadyOnChain으로 변경 (노란색 테두리 + 연계 가능 상태 >> ReadyOnChain상태)
                return;
            }
            effectVisualizer.ApplyVisualState(CardVisualState.Ready); // 카드의 상태를 Ready로 변경 (노란색 테두리)
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false; // 마우스 포인터가 카드 위에 있지 않은 상태로 설정

        // cardDisplay가 없으면 (정비 UI 등) 상호작용 불가능
        if (cardDisplay == null) return;

        // 드래그 중인 경우 처리하지 않음 (드래그 중 포인터가 나갔을 때는 계속 드래그 유지)
        if (cardState == CardState.OnDrag) 
        {
            if (!cardDisplay.isOnDrag) cardDisplay.TargetArrowReset();
            return;
        }

        // 카드의 시각적 효과 (이펙트 제외)
        if (!cardDisplay.isOnDrag) cardDisplay.TargetArrowReset(); // 카드 드래그 중이 아닐 때 타겟 화살표 초기화
        if (GameManager.Instance == null || GameManager.Instance.turnController == null) return;
        if (GameManager.Instance.turnController.onAction) return; // 행동 중일 경우 상호작용 불가능

        // currentCard가 이 카드가 맞을 때만 해제 (다른 카드가 currentCard일 수 있음)
        if (cardDisplay.currentCard == this)
        {
            cardDisplay.currentCard = null;// 현재 카드 설정 해제.
        }
        
        ResetSiblingIndex();// List의 순서에 맞게 원래 위치로 돌아가기.
        rect.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutSine);

        // 예외처리 + 이펙트 초기화
        if (effectVisualizer != null)
        {
            effectVisualizer.SetStateToNone(); // 카드의 상태를 None으로 변경
        }

        // canchain 상태의 카드들을 원상태로(enhanced에 따라서 다르게 설정)
        cardDisplay.ResetCanChain();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 공통 상호작용 검증
        if (!cardDisplay.CanInteractCard()) return;
        
        // 카드 상태 검증
        if (cardState != CardState.CanDrag) return;

        // 드래그 상태 설정 (이제 currentCard도 함께 설정)
        cardDisplay.SetCurrentCard(this);
        transform.SetAsLastSibling();// 카드가 가장 위에 오도록 설정

        cardDisplay.lineRenderer.gameObject.SetActive(true); // 드래그 중일 때 라인 렌더러 활성화
        cardDisplay.arrowImage.gameObject.SetActive(true); // 드래그 중일 때 화살표 이미지 활성화
        cardState = CardState.OnDrag; // 카드 상태를 OnDrag로 변경
    }
    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 취소 상태이거나 현재 카드가 이 카드가 아닌 경우 드래그하지 않음
        if (cardDisplay.currentCard != this) return;
        if (cancelDrag || cardState != CardState.OnDrag) return;
        
        if (GameManager.Instance != null && GameManager.Instance.turnController != null)
        {
            if (GameManager.Instance.turnController.onAction) return;
        }

        // 드래그 할때 카드가 위로 안올라올 경우의 후처리
        if (this.rect.anchoredPosition == originalPos)
            rect.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutSine);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        // 게임 상태 검증 (행동 중이면 처리하지 않음)
        if (GameManager.Instance != null && GameManager.Instance.turnController != null)
        {
            if (GameManager.Instance.turnController.onAction) return;
        }

        // 드래그 상태가 아니면 처리하지 않음
        if (!cardDisplay.isOnDrag) return;
        
        // 현재 카드가 이 카드가 아니면 처리하지 않음 (다른 카드가 드래그 중)
        if (cardDisplay.currentCard != this) return;

        // 카드 상태가 OnDrag가 아니면 처리하지 않음
        if (cardState != CardState.OnDrag) return;

        cardDisplay.TargetArrowReset(); // 드래그 종료 시 타겟 화살표 초기화
        cardDisplay.ResetCanChain(); // 드래그 종료시, 체인 가능 이펙트 리셋

        // 드래그 UI 비활성화
        cardDisplay.lineRenderer.gameObject.SetActive(false);
        cardDisplay.arrowImage.gameObject.SetActive(false);

        // 드래그 위치 종료의 정보를 통해 사용 성공시 상태 OnUse로 변경
        cardDisplay.OnMousepoint(eventData);

        // 카드 사용 성공 (FXOnUse에서 카드 제거)
        if (cardState == CardState.OnUse)
        {
            cardDisplay.ClearCurrentCard(); // 드래그 상태 해제
            return;
        }

        // 카드 사용 실패 - 원래 위치로 돌아가기
        cardState = CardState.None; // 임시로 None 상태로 설정
        rect.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutSine);

        // 이펙트 초기화
        if (effectVisualizer != null)
        {
            effectVisualizer.SetStateToNone();
        }

        // 최종적으로 드래그 상태 해제 및 상태 정규화
        // OnDrag 종료 후 0.4초 애니메이션 동안 상태 유지, 이후 상태 업데이트
        DOVirtual.DelayedCall(0.4f, () =>
        {
            // 다시 한번 검증 (카드가 삭제되었을 수도 있음)
            if (this == null || cardDisplay == null) return;

            // 현재 카드가 이 카드인 경우만 해제
            if (cardDisplay.currentCard == this)
            {
                cardDisplay.ClearCurrentCard();
            }

            // 마나 상태에 따라 상태 업데이트
            if (cardData.IsUsable(GameManager.Instance.turnController.battleFlow.currentMana))
            {
                cardState = CardState.CanDrag;
            }
            else
            {
                cardState = CardState.CanMouseOver;
            }
        });
    }

    public void FXOnUse()
    {
        DG.Tweening.Sequence seq = DOTween.Sequence();

        // 첫 단계: frameCover, illustCover 반투명하게
        //seq.Append(frameCover.DOFade(0.5f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(illustCover.DOFade(0.5f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardCostImage.DOFade(0f, 0.3f).SetEase(Ease.OutSine));


        // 두 번째 단계: 전체 fade out + FX 적용
        seq.AppendCallback(() =>
        {
            if (effectVisualizer.currentState == CardVisualState.ReadyOnChain || effectVisualizer.currentState == CardVisualState.Chain)
                effectVisualizer.enhanceTriggered = true; // 강화 사용 효과 트리거

            effectVisualizer.ApplyVisualState(CardVisualState.Use);
        });

        //seq.Append(frameCover.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(illustCover.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardCostImage.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardFrame.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardImage.DOFade(0f, 0.4f).SetEase(Ease.OutSine));
        seq.Join(cardTypeImage.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardCharImage.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(shade1.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(shade2.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardName.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardCost.DOFade(0f, 0.3f).SetEase(Ease.OutSine));
        seq.Join(cardDescription.DOFade(0f, 0.3f).SetEase(Ease.OutSine));

        // 파티클이 재생될 시간 (약 1초 정도 대기)
        seq.AppendInterval(1f);

        // 마지막 단계: 오브젝트 제거 및 정렬
        seq.OnComplete(() =>
        {
            // UIParticle 정리 (재생 종료 후)
            if (effectVisualizer != null)
            {
                if (effectVisualizer.useCardFXParticle != null)
                {
                    effectVisualizer.useCardFXParticle.Clear();
                    effectVisualizer.useCardFXParticle.Stop();
                }

                if (effectVisualizer.useEnhancedCardFXParticle != null)
                {
                    effectVisualizer.useEnhancedCardFXParticle.Clear();
                    effectVisualizer.useEnhancedCardFXParticle.Stop();
                }
            }

            // 카드 제거 전에 드래그 상태 정리
            if (cardDisplay != null && cardDisplay.currentCard == this)
            {
                cardDisplay.ClearCurrentCard();
            }

            Destroy(this.gameObject);
            
            if (cardDisplay != null)
            {
                cardDisplay.CardArrange();
            }
        });
    }
    public void SetCardData(CardModel card)// 카드 데이터 설정
    {
        cardData = card;
        UpdateCardImage();// 카드 이미지 업데이트
        UpdatCardInfo();// 카드 정보 업데이트
    }
    public void UpdateCardImage()
    {
        if (cardData.illustration == null)
        {
            cardImage.sprite = null;
            cardImage.gameObject.SetActive(false);

            illustCover.sprite = null;
            illustCover.gameObject.SetActive(false);
        }
        else
        {
            cardImage.gameObject.SetActive(true);
            illustCover.gameObject.SetActive(true);

            cardImage.sprite = cardData.illustration;
            illustCover.sprite = cardData.illustration;
        }

        cardTypeImage.sprite = cardData.cardType;
        cardCharImage.sprite = cardData.chClass;
    }

    /// <summary>
    /// 카드 description, cost, name 설명 업데이트.
    /// </summary>
    public void UpdatCardInfo()// 외부에서 해당 메서드 for문 으로 묶어서 action에 구독하여 사용하기에, 기능 분리.
    {
        cardName.text = cardData.cardName; // 카드 이름 설정
        cardCost.text = cardData.GetEffectiveCost().ToString(); // 카드 코스트 설정

        IStatusReceiver caster = GameManager.Instance.turnController.battleFlow.playerParty.Find(p => p.ChClass == cardData.characterClass);
        cardDescription.text = cardData.GetFormattedCardText(caster);// 카드 설명 설정
    }

    /// <summary>
    /// UI 전용 카드 정보 업데이트 (캠프 씬에서 사용)
    /// </summary>
    public void UpdateCardInfoOnlyUI()
    {
        cardName.text = cardData.cardName; // 카드 이름 설정
        cardCost.text = cardData.GetEffectiveCost().ToString(); // 카드 코스트 설정
        
        // ---캠프 씬에서는 caster 정보가 없으므로 플레이스홀더 {숫자}를 ?로 치환---
        // null을 대입 시 기본값 출력으로 변경됨
        string description = cardData.cardText;
        //description = Regex.Replace(description, @"\{\d+\}", "?");
        cardDescription.text = cardData.GetFormattedCardText(null);
    }
    public void SetOriginalPos()// 덱 최초 세팅 시점, 카드 추가 혹은 감소시 위치 초기화.
    {
        // 원래 위치 설정
        originalPos = rect.anchoredPosition;

        // 카드의 회전에 따른 위로 올라갔을때의 위치 설정.
        float distance = 135f;// 135 만큼 위로 올라감.
        float angle = rect.localEulerAngles.z * Mathf.Deg2Rad;
        Vector2 localUpDir = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));// 로컬 좌표계에서의 각도 변환을 위해 라디안을 통해 삼각함수 사용. (tranform.up 등의 방식은 월드 좌표계 기준의 단위 벡터이기 때문에 로컬 기준의 값을 계산해야 함.)

        targetPos = rect.anchoredPosition + localUpDir * distance;
    }
    public void SetTargetPos(Vector2 tPos ,float tAngle)// 핸드의 카드에 변동사항이 생겨 움직이는 도중, 핸드의 카드에 접근시 정상적인 동작을 위해 필요한 메서드.
    {
        float distance = 135f;// 135 만큼 위로 올라감.
        float angle = tAngle * Mathf.Deg2Rad;
        Vector2 localUpDir = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));

        targetPos = tPos + localUpDir * distance;
    }
    // 바로 위와 같은 값 설정이 아닌, 값 리턴 형식의 메서드
    public Vector2 GetElevatedPos(Vector2 basePos)
    {
        float elevateAmount = 40f;
        return new Vector2(basePos.x, basePos.y + elevateAmount);
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
    public GameObject GetCoverIllust()
    {
        return illustCover.gameObject; // 카드 일러스트 커버 오브젝트 가져오기
    }
    public void UpdateStateMoveEnd()
    {
        if (cardData.IsUsable(GameManager.Instance.turnController.battleFlow.currentMana))
        {
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
        //SoundManager.Instance.PlaySFX(SoundCategory.SFX, 104);
    }

}


/*
 * if (isPointerOver||CardState.OnDrag == cardState)
        {
            if(cardDisplay.currentCard != this)
            {
                if (GameManager.Instance.turnController.onAction) return; // 행동 중일 경우 상호작용 불가능
                if (cardState == CardState.None) return; // 상태가 None인 경우 상호작용 불가능

                cardDisplay.currentCard = this;// 현재 카드 설정.
                transform.SetAsLastSibling();// 카드가 가장 위에 오도록 설정

                rect.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutSine);

                // 카드의 이펙트 시각 효과
                if (effectVisualizer.currentState == CardVisualState.Chain) return; // 카드의 상태가 Chain인 경우 이펙트 변경 취소 (빨간색 유지)

                effectVisualizer.ApplyVisualState(CardVisualState.Ready); // 카드의 상태를 Ready로 변경 (노란색 테두리)
            }
        }
*/ // update에서 일정 주기마다 초기화 해주던 방식 제거.
