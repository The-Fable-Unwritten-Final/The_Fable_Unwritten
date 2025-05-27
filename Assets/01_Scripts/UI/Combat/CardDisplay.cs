using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DG.Tweening;
using System.Linq;
using static CardInHand;
using static CardEffectVisualizer;

// 핸드에 소지하고 있는 카드들 전체를 관리하는 스크립트.
public class CardDisplay : MonoBehaviour
{
    [SerializeField] RectTransform canvasRect;
    // cardsInHand를 SerializeField로 할 경우 에디터 상에서만 경고가 발생하나, 이후 카드 시스템이 완성될때는 cardsInHand를 실제 카드 데이터를 가져와서 하기 때문에 SerializeField를 지울 예정.
    public List<CardInHand> cardsInHand = new List<CardInHand>(); // 핸드에 소지하고 있는 카드들.
    [SerializeField] GameObject cardPrefab; // 카드 프리팹
    int layerCharacter;
    int layerMonster;
    int layerTrash;

    [Header("CardsInHand")]
    [SerializeField] float cardMidPosY = 470f; // 중간에 위치할 카드의 Recttransform Y 좌표.
    [SerializeField] float yOffset = 5f; // 카드의 Y 좌표 오프셋. (카드가 겹치지 않도록 하기 위함.)
    [SerializeField] float cardSpacing = 120f; // 카드 간의 x 폭.
    [SerializeField] float cardAngle = 1.2f; // 카드 각도 (카드가 겹치지 않도록 하기 위함.)

    [Header("Drag with Arrow")]
    [SerializeField] private GraphicRaycaster uiRaycaster; // Canvas에 있는 GraphicRaycaster
    [SerializeField] private EventSystem eventSystem;
    public RectTransform arrowImage; // 드래그 중에 보일 화살표 이미지.
    public UILineRenderer lineRenderer;
    public bool isOnDrag = false; // 드래그 중인지 여부
    public CardInHand currentCard; // 현재 드래그 중인 카드


    Coroutine cardDragCoroutine;

    private void Awake()
    {
        for(int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].SetCardState(CardInHand.CardState.CanDrag);// 임시로 카드 상태 설정.
        }
        // 대상 레이어 캐싱
        layerCharacter = LayerMask.NameToLayer("Character");
        layerMonster = LayerMask.NameToLayer("Monster");
        layerTrash = LayerMask.NameToLayer("Trash");
    }
    private void Start()
    {
        GameManager.Instance.turnController.OnStartPlayerTurn += SetAllCardCanMouseOver;// 플레이어 턴 시작 시 카드 상태를 CanMouseOver로 변경
    }
    private void Update()
    {
        if(isOnDrag)
        {
            UpdateLineRenderer();
            UpdateArrowHead();
        }
    }

    void UpdateLineRenderer()// 화살표 선
    {
        if (cardsInHand.Count == 0 || currentCard == null) return;

        RectTransform cardRect = currentCard.GetComponent<RectTransform>();
        Vector2 localMousePos;

        // 마우스 위치를 UI 캔버스의 로컬 좌표로 변환.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localMousePos);// 추후 카메라 분리시 null에 UI용 카메라 넣어주기

        // lineRenderer를 곡선의 형태로 표시.
        Vector2 cardHeadOffset = new Vector2(0, cardRect.rect.height * (1 - cardRect.pivot.y) * cardRect.lossyScale.y);
        lineRenderer.Points[0] = cardRect.anchoredPosition + cardHeadOffset;// 카드의 머리 부분
        lineRenderer.Points[1] = new Vector2(localMousePos.x * 0.33f, localMousePos.y * 0.85f);
        lineRenderer.Points[2] = new Vector3(localMousePos.x * 0.66f, localMousePos.y * 0.95f);
        lineRenderer.Points[3] = localMousePos;

        lineRenderer.SetAllDirty();// 라인 렌더러 업데이트.
    }
    void UpdateArrowHead()// 화살표 머리
    {
        if(lineRenderer.Points.Length < 2) return;

        // 화살표의 위치와 각도를 업데이트
        Vector2 p1 = CalculateBezierPoint(0.98f, lineRenderer.Points[0], lineRenderer.Points[1], lineRenderer.Points[2], lineRenderer.Points[3]);// 마지막 포인트에 근접한 점을 구하기 위해 t를 0.98f로 설정.
        Vector2 p2 = lineRenderer.Points[lineRenderer.Points.Length - 1];// 마지막 포인트

        Vector2 direction = (p2 - p1).normalized;

        // 화살표의 위치 설정
        arrowImage.anchoredPosition = p2;

        // 화살표의 각도 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;// UI에서 사용하기 위해 방향 벡터를 회전 각도로 변환.
        arrowImage.localRotation = Quaternion.Euler(0, 0, angle);
    }
    public void CardArrange()// 카드 정렬 (수량에 따른 각도, 거리)
    {
        if(cardsInHand.Count == 0) return;

        isOnDrag = false;
        int oddeven = cardsInHand.Count / 2;

        if(oddeven !=0)// 핸드의 카드 개수가 홀수인 경우
        {
            for(int i = 0; i < cardsInHand.Count; i++)
            {
                int cash = i;// 카드의 인덱스를 캐싱해서 OnComplete에서 사용
                // 각도 설정(cardAngle)
                float angle = 0f;
                angle = (oddeven * cardAngle) + (i * -cardAngle);
                cardsInHand[i].GetComponent<RectTransform>().DORotate(new Vector3(0, 0, angle), 0.5f).SetEase(Ease.OutSine);

                // 카드의 좌표 설정
                Vector2 targetPos = new Vector2(0, 0);
                int distance = Mathf.Abs(i - oddeven);
                targetPos.x =(oddeven * -cardSpacing) + (i * cardSpacing); // 카드간의 폭 (cardSpacing)
                targetPos.y = (- yOffset * distance * (distance + 1) / 2f) - cardMidPosY;
                if (cardsInHand[cash].isPointerOver)
                {
                    Vector2 elevatedPos = cardsInHand[cash].GetElevatedPos(targetPos);

                    cardsInHand[i].GetComponent<RectTransform>()
                    .DOAnchorPos(elevatedPos, 0.1f)
                    .OnStart(() =>
                    {
                        cardsInHand[cash].originalPos = targetPos; // 여전히 기준점은 바닥 위치
                        cardsInHand[cash].SetTargetPos(targetPos, angle);
                    })
                    .OnComplete(() =>
                    {
                        // 잘못된 위치에 있는 카드들을 원래 위치로 보내기
                        RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();
                        if (!cardsInHand[i].isPointerOver && cardRect.position != (Vector3)cardsInHand[i].originalPos) // 마우스를 올린 카드가 아님 + 잘못된 위치에 있을경우
                        {
                            cardRect.DOAnchorPos(cardsInHand[i].originalPos, 0.1f); // 원래 위치로 보내기
                        }
                        
                        cardsInHand[cash].UpdateStateMoveEnd();
                    })
                    .SetEase(Ease.OutSine);
                }
                else
                {
                    cardsInHand[i].GetComponent<RectTransform>()
                    .DOAnchorPos(targetPos, 0.1f)
                    .OnStart(() =>
                    {
                        cardsInHand[cash].SetCardState(CardInHand.CardState.None);// 이동하는 동안에는 none
                        cardsInHand[cash].originalPos = targetPos;
                        cardsInHand[cash].SetTargetPos(targetPos, angle);// 카드의 원래 위치 설정.
                    })
                    .OnComplete(() =>
                    {
                        // 잘못된 위치에 있는 카드들을 원래 위치로 보내기
                        RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();
                        if (!cardsInHand[i].isPointerOver && cardRect.position != (Vector3)cardsInHand[i].originalPos) // 마우스를 올린 카드가 아님 + 잘못된 위치에 있을경우
                        {
                            cardRect.DOAnchorPos(cardsInHand[i].originalPos, 0.1f); // 원래 위치로 보내기
                        }

                        cardsInHand[cash].UpdateStateMoveEnd();// 카드 이동 완료 후 상태 업데이트.
                    })
                    .SetEase(Ease.OutSine);
                }
            }
        }
        else
        {
            for(int i = 0; i < cardsInHand.Count; i++)
            {
                int cash = i;// 카드의 인덱스를 캐싱해서 OnComplete에서 사용
                // 각도 설정(cardAngle)
                float angle = 0f;
                angle = (oddeven * cardAngle - 1.25f) + (i * -cardAngle);
                cardsInHand[i].GetComponent<RectTransform>().DORotate(new Vector3(0, 0, angle), 0.5f).SetEase(Ease.OutSine);

                // 카드의 좌표 설정
                Vector2 targetPos = new Vector2(0, 0);
                int distance = Mathf.Abs(i - oddeven);
                targetPos.x = (oddeven * -cardSpacing) + (i * cardSpacing); // 카드간의 폭 (cardSpacing)
                targetPos.y = (- yOffset * distance * (distance + 1) / 2f) - cardMidPosY;
                if (cardsInHand[cash].isPointerOver)
                {
                    Vector2 elevatedPos = cardsInHand[cash].GetElevatedPos(targetPos);

                    cardsInHand[i].GetComponent<RectTransform>()
                    .DOAnchorPos(elevatedPos, 0.1f)
                    .OnStart(() =>
                    {
                        cardsInHand[cash].originalPos = targetPos; // 여전히 기준점은 바닥 위치
                        cardsInHand[cash].SetTargetPos(targetPos, angle);
                    })
                    .OnComplete(() =>
                    {
                        // 잘못된 위치에 있는 카드들을 원래 위치로 보내기
                        RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();
                        if (!cardsInHand[i].isPointerOver && cardRect.position != (Vector3)cardsInHand[i].originalPos) // 마우스를 올린 카드가 아님 + 잘못된 위치에 있을경우
                        {
                            cardRect.DOAnchorPos(cardsInHand[i].originalPos, 0.1f); // 원래 위치로 보내기
                        }

                        cardsInHand[cash].UpdateStateMoveEnd();
                    })
                    .SetEase(Ease.OutSine);
                }
                else
                {
                    cardsInHand[i].GetComponent<RectTransform>()
                    .DOAnchorPos(targetPos, 0.1f)
                    .OnStart(() =>
                    {
                        cardsInHand[cash].SetCardState(CardInHand.CardState.None);// 이동하는 동안에는 none
                        cardsInHand[cash].originalPos = targetPos;
                        cardsInHand[cash].SetTargetPos(targetPos, angle);// 카드의 원래 위치 설정.
                    })
                    .OnComplete(() =>
                    {
                        // 잘못된 위치에 있는 카드들을 원래 위치로 보내기                     
                        RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();
                        if (!cardsInHand[i].isPointerOver && cardRect.position != (Vector3)cardsInHand[i].originalPos) // 마우스를 올린 카드가 아님 + 잘못된 위치에 있을경우
                        {
                            cardRect.DOAnchorPos(cardsInHand[i].originalPos, 0.1f); // 원래 위치로 보내기
                        }

                        cardsInHand[cash].UpdateStateMoveEnd();// 카드 이동 완료 후 상태 업데이트.
                    })
                    .SetEase(Ease.OutSine);
                }
            }
        }
    }

    /// <summary>
    /// AddCard의 플로우 :
    /// 빈카드 이미지 생성 >> 카드 데이터 넣어주기 >> 해당 카드를 핸드에 배치
    /// </summary>
    /// <param name="data"></param>
    public void AddCard(CardModel data)
    {
        GameObject card = Instantiate(cardPrefab,this.transform);
        cardsInHand.Add(card.GetComponent<CardInHand>());// 카드 추가.
        card.GetComponent<CardInHand>().cardDisplay = this;// 카드의 카드디스플레이 설정.
        card.GetComponent<CardInHand>().SetCardData(data);// 카드의 카드데이터 설정 + 카드 정보

        // 카드를 Class 에 맞게 + 같은 클라스 끼리는 Index 순서대로 재배치
        cardsInHand.Sort((x, y) => x.cardData.index.CompareTo(y.cardData.index));// 카드의 List상의 인덱스 순 정렬
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].GetComponent<RectTransform>().SetSiblingIndex(i);// 카드의 자식 순서 설정(카드의 보이는 순서)
        }
        CardArrange();
    }
    public void UseCard(IStatusReceiver target)
    {
        if(currentCard == null) return;// 현재 카드가 없으면 사용 불가.
        // 만약 카드 사용을 못하게 된다면(코스트 부족 등..), SetSiblingIndex을 통해 해당 카드의 순서를 원래대로 돌려주기
        // 카드 사용 가능 조건 체크

        //카드 사용 시도
        if(!GameManager.Instance.combatUIController.UsedCard(currentCard.cardData,target))
        {
            // 만약 카드 사용이 실패 했다면, 상태 복구.
            currentCard.SetCardState(CardInHand.CardState.CanDrag);// 카드 상태를 CanDrag로 복구.
            currentCard.ResetSiblingIndex();// 카드 순서 원래대로 복구.
            return;
        }
        cardsInHand.Remove(currentCard);// 카드 사용.
        currentCard.FXOnUse();// 카드 사용 이펙트 재생.(destroy + cardArrange 보유)
        //Destroy(currentCard.gameObject);// 카드 삭제.
        //CardArrange();
    }
    public void ThrowAwayCard(CardInHand card)// 카드 버리기
    {
        if (card == null) return;
        GameManager.Instance.combatUIController.ThrowCard(card.cardData);// 카드 버리기.
        CardArrange();
    }
    /// <summary>
    /// Hand에 있는 모든 카드의 정보(Cost,Desc) 업데이트.
    /// </summary>
    public void AllCardInfoUpdate()
    {
        for(int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].UpdatCardInfo();// 카드 정보 업데이트.
        }
    }
    public void SetAllCardCanMouseOver()
    {
        // 플레이어 턴 시작 때 CanMouseOver 실행
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].isPointerOver) return; // 마우스를 위에 올리고 있는 상태라면 별도의 리셋 작업 x
            cardsInHand[i].SetCardState(CardInHand.CardState.CanMouseOver);// 카드 상태를 CanMouseOver로 변경
        }
    }
    public void SetCardCanDrag()
    {
        if(cardsInHand.All(card => card.GetCardState() == CardState.CanDiscard)) return;

        if (GameManager.Instance == null || GameManager.Instance.turnController == null || GameManager.Instance.turnController.battleFlow == null)
        {
        Debug.LogWarning("GameManager or its components are not ready.");
        return;
        }
        // 만약 현재 턴이 적 턴이라면 카드의 상태를 CanMouseOver로
        if (GameManager.Instance.turnController.turnState == TurnController.TurnState.EnemyTurn)
        {
            SetAllCardCanMouseOver();
            return;
        }
        // 코스트가 충분한 경우 카드의 상태를 CanDrag로 변경
        for(int i = 0; i < cardsInHand.Count; i++)
        {
            // 만약 update 호출전 새로운 카드를 드래그 하였을 때의 예외처리. + 여기에 실제로 드래그 중이 아닌데 이전 값이 남아있을 경우의 && 예외처리
            if (cardsInHand[i].GetCardState() == CardState.OnDrag && Input.GetMouseButtonDown(0)) return;

            if (cardsInHand[i].cardData.IsUsable(GameManager.Instance.turnController.battleFlow.currentMana))// 현재 보유 마나를 가져와, 사용 가능한지 확인. (사용 가능시 CanDrag, 불가능시 CanMouseOver)
            {
                if (cardsInHand[i].GetCardState() == CardState.CanDiscard) return;
                cardsInHand[i].SetCardState(CardInHand.CardState.CanDrag);// 카드 상태를 CanDrag로 변경
            }
            else
            {
                if (cardsInHand[i].GetCardState() == CardState.CanDiscard) return;
                cardsInHand[i].SetCardState(CardInHand.CardState.CanMouseOver);// 카드 상태를 CanMouseOver로 변경
            }
        }
    }
    /// <summary>
    /// 손안의 카드들의 상태를 버릴 카드 선택이 가능한 상태로 변경.
    /// </summary>
    public void SetCardCanDiscard()
    {
        // 만약 현재 턴이 적 턴이라면 카드의 상태를 CanMouseOver로
        if (GameManager.Instance.turnController.turnState == TurnController.TurnState.EnemyTurn)
        {
            SetAllCardCanMouseOver();
            return;
        }

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].SetCardState(CardInHand.CardState.CanDiscard);// 카드 상태를 CanDiscard로 변경
        }
    }

    // 마우스 포인트의 위치에, 카드의 효과 적용 시도.
    public void OnMousepoint(PointerEventData eventData)
    {
        // 액션 중일때는 카드 사용 불가능.
        if(GameManager.Instance.turnController.onAction) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject target = hit.collider.gameObject;
            int layer = target.layer;

            if (layer == layerCharacter || layer == layerMonster)
            {
                currentCard.SetCardState(CardInHand.CardState.OnUse);
                UseCard(target.GetComponent<IStatusReceiver>());
                currentCard = null;
            }
            else if (layer == layerTrash)
            {
                //ThrowAwayCard();
                currentCard = null;
            }
            else
            {
                ResetCardState();
            }
        }
        else
        {
            ResetCardState();
        }
    }
    private void ResetCardState()
    {
        if(currentCard == null) return;// 현재 카드가 없으면 사용 불가.
        currentCard.SetCardState(CardInHand.CardState.CanDrag);
        currentCard.transform.SetSiblingIndex(cardsInHand.IndexOf(currentCard));
        currentCard = null;
    }
    // 3차 베지어 곡선 수식. (Unity 포럼에서 찾음.)
    Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    // 반환 메서드
    public int GetCardIndex(CardInHand card)
    {
        int index = cardsInHand.IndexOf(card);
        if(index != -1)
        {
            return index; // 카드가 리스트에 있을 경우 인덱스 반환
        }
        return 0; // 카드가 리스트에 없을 경우 0 반환
    }
    /// <summary>
    /// 사망자 카드 삭제 처리
    /// </summary>
    public void RemoveDeadCharacterCards()
    {
        foreach (var card in new List<CardInHand>(cardsInHand))
        {
            var caster = GameManager.Instance.turnController.battleFlow.GetCharacter(card.cardData.characterClass);
            if (caster == null || !caster.IsAlive())
            {
                GameManager.Instance.combatUIController.ThrowCard(card.cardData);
                CardArrange();
            }
        }
    }
    /// <summary>
    /// 카드의 연계 상태 업데이트
    /// </summary>
    public void CheckChainCard()
    {
        foreach (var card in cardsInHand)
        {
            if (card.effectVisualizer.currentState == CardVisualState.Use) return; // 카드의 상태가 Use인 경우 이펙트 변경 취소 (사용되는 효과 재생 유지)

            if (card.cardData.isEnhanced) // 연계된 상태일때
            {
               card.effectVisualizer.ApplyVisualState(CardVisualState.Chain); // 카드의 상태를 Chain으로 변경 (빨간색 테두리)
            }
            else
            {
                card.effectVisualizer.ApplyVisualState(CardVisualState.None); // 카드의 상태를 None으로 변경 (이펙트 제거)
            }
        }
    }
    public void CheckCanChain()
    {
        foreach (var card in cardsInHand)
        {
            if (card.effectVisualizer.currentState == CardVisualState.Use ||
                card.effectVisualizer.currentState == CardVisualState.Ready) return; // 카드의 상태가 Use,Ready인 경우 이펙트 변경 취소 (사용되는 효과 재생 유지)

            if (currentCard == null) return;

            if(BattleLogManager.comboTable.TryGetValue(currentCard.cardData.type, out var cardTypes))
            {
                if( cardTypes.Contains(card.cardData.type)) // 현재 카드 타입이 연계 가능한 카드 타입에 포함되어 있다면
                {
                    card.effectVisualizer.ApplyVisualState(CardVisualState.CanChain); // 카드의 상태를 CanChain으로 변경 (얇은 노란 테두리)
                }
            }
        }
    }
    /// <summary>
    /// CanChain 상태에서, currentCard에서 마우스를 해제한 경우, 원래 상태로 돌리기
    /// </summary>
    public void ResetCanChain()
    {
        foreach (var card in cardsInHand)
        {
            if (card.effectVisualizer.currentState == CardVisualState.Use ||
                               card.effectVisualizer.currentState == CardVisualState.Ready) return; // 카드의 상태가 Use,Ready인 경우 이펙트 변경 취소 (사용되는 효과 재생 유지)

            // 상태 복구.
            if(card.cardData.isEnhanced)
            {
                card.effectVisualizer.ApplyVisualState(CardVisualState.Chain); // 카드의 상태를 Chain으로 변경 (빨간색 테두리)
            }
            else
            {
                card.effectVisualizer.ApplyVisualState(CardVisualState.None); // 카드의 상태를 None으로 변경 (이펙트 제거)
            }
        }
    }
    /// <summary>
    /// cardInHand 중에서 선택중인 카드의 적용 가능 대상에 화살표 표시
    /// </summary>
    public void TargetArrowDisplay()
    {
        if (currentCard == null) return;

        CardModel model = currentCard.cardData;
        BattleFlowController btc = GameManager.Instance.turnController.battleFlow;

        if (model.effects != null && model.effects.Count > 0)
        {
            // model의 target type에 따라 화살표 표시
            switch (model.targetType)
            {
                // 자기 자신에게만 적용되는 효과
                case TargetType.None:
                    var self = btc.playerParty.FirstOrDefault(x => x.ChClass == model.characterClass);
                    if (self != null) self.IsTargetable = true; // 자기 자신 에게만 화살표 표시
                    break;

                // 아군을 대상으로 적용 가능한 효과
                case TargetType.Ally:
                    btc.playerParty.ForEach(x => x.IsTargetable = true); // 모든 캐릭터의 화살표 활성화
                    break;

                // 적을 대상으로 적용 가능한 효과
                case TargetType.Enemy:
                    btc.enemyParty.ForEach(x => { if (x != null) x.IsTargetable = true; }); // 모든 몬스터의 화살표 활성화 (null 예외 처리)
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 대상 화살표 비활성화
    /// </summary>
    public void TargetArrowReset()
    {
        BattleFlowController btc = GameManager.Instance.turnController.battleFlow;
        btc.playerParty.ForEach(x => x.IsTargetable = false); // 모든 캐릭터의 화살표 비활성화
        btc.enemyParty.ForEach(x => { if (x != null) x.IsTargetable = false; }); // 모든 몬스터의 화살표 비활성화 (null 예외 처리)
    }

    // 카드 상태 일정 주기마다 업데이트 로직 + 플레이어 턴이 시작할떄 호출
    public void StartPlayerTurn()
    {
        // 기존 코루틴이 돌고 있다면 멈추고 다시 시작
        if (cardDragCoroutine != null)
            StopCoroutine(cardDragCoroutine);

        cardDragCoroutine = StartCoroutine(RepeatSetCardCanDrag());
    }
    // 플레이어 턴 종료를 누르면 호출
    public void EndPlayerTurn()
    {
        if (cardDragCoroutine != null)
        {
            StopCoroutine(cardDragCoroutine);
            cardDragCoroutine = null;
        }
    }
    private IEnumerator RepeatSetCardCanDrag()
    {
        bool isreset = false;

        while (true)
        {
            // 현재 드래그 중일 경우 continue;
            if(currentCard != null)
            {
                if (currentCard.GetCardState() == CardState.OnDrag)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
            }           

            // 간단한 상호작용 시의 경우, 최초 1회만 상태 업데이트
            if (Input.GetMouseButtonDown(0) || cardsInHand.Any(x => x.isPointerOver))
            {
                isreset = false; // 카드의 상태가 한번 변경되면 다시 리셋 트리거 세팅.
                yield return new WaitForEndOfFrame();
                continue; // 조건에 해당될시 아래 내용 스킵.
            }
            if (!isreset)
            {
                isreset = true;
                SetCardCanDrag();
                //CardArrange();
                yield return new WaitForEndOfFrame();
                for(int i = 0; i < cardsInHand.Count; i++)
                {
                    RectTransform cardRect = cardsInHand[i].GetComponent<RectTransform>();

                    if(DOTween.IsTweening(cardRect)) continue; // 현재 카드가 이동 중이라면 아래의 재정렬 무시.

                    if (!cardsInHand[i].isPointerOver && cardRect.position != (Vector3)cardsInHand[i].originalPos) // 마우스를 올린 카드가 아님 + 잘못된 위치에 있을경우
                    {
                        cardRect.DOAnchorPos(cardsInHand[i].originalPos, 0.1f); // 원래 위치로 보내기
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
