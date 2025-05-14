using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DG.Tweening;
using System.Linq;
using static CardInHand;

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

    [Header("Drag with Arrow")]
    [SerializeField] private GraphicRaycaster uiRaycaster; // Canvas에 있는 GraphicRaycaster
    [SerializeField] private EventSystem eventSystem;
    public RectTransform arrowImage; // 드래그 중에 보일 화살표 이미지.
    public UILineRenderer lineRenderer;
    public bool isOnDrag = false; // 드래그 중인지 여부
    public CardInHand currentCard; // 현재 드래그 중인 카드

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
    void CardArrange()// 카드 정렬 (수량에 따른 각도, 거리)
    {
        if(cardsInHand.Count == 0) return;

        int oddeven = cardsInHand.Count / 2;

        if(oddeven !=0)// 핸드의 카드 개수가 홀수인 경우
        {
            for(int i = 0; i < cardsInHand.Count; i++)
            {
                int cash = i;// 카드의 인덱스를 캐싱해서 OnComplete에서 사용
                // 각도 설정
                float angle = 0f;
                angle = (oddeven * 2.5f) + (i * - 2.5f);
                cardsInHand[i].GetComponent<RectTransform>().DORotate(new Vector3(0, 0, angle), 0.5f).SetEase(Ease.OutSine);

                // 카드의 좌표 설정
                Vector2 targetPos = new Vector2(0, 0);
                int distance = Mathf.Abs(i - oddeven);
                targetPos.x =(oddeven * -100) + (i * 100);
                targetPos.y = (- yOffset * distance * (distance + 1) / 2f) - cardMidPosY;
                cardsInHand[i].GetComponent<RectTransform>()
                    .DOAnchorPos(targetPos, 0.5f)
                    .OnStart(() =>
                    {
                        cardsInHand[cash].originalPos = targetPos;
                        cardsInHand[cash].SetTargetPos(targetPos, angle);// 카드의 원래 위치 설정.
                    })
                    .SetEase(Ease.OutSine);

                cardsInHand[cash].OnCardMoveCouroutine();// 카드 이동중에는 상호작용 불가능 설정.
            }
        }
        else
        {
            for(int i = 0; i < cardsInHand.Count; i++)
            {
                int cash = i;// 카드의 인덱스를 캐싱해서 OnComplete에서 사용
                // 각도 설정
                float angle = 0f;
                angle = (oddeven * 2.5f - 1.25f) + (i * - 2.5f);
                cardsInHand[i].GetComponent<RectTransform>().DORotate(new Vector3(0, 0, angle), 0.5f).SetEase(Ease.OutSine);

                // 카드의 좌표 설정
                Vector2 targetPos = new Vector2(0, 0);
                int distance = Mathf.Abs(i - oddeven);
                targetPos.x = (oddeven * -100) + (i * 100);
                targetPos.y = (- yOffset * distance * (distance + 1) / 2f) - cardMidPosY;
                cardsInHand[i].GetComponent<RectTransform>()
                    .DOAnchorPos(targetPos, 0.1f)
                    .OnStart(() => 
                    {
                        cardsInHand[cash].originalPos = targetPos;
                        cardsInHand[cash].SetTargetPos(targetPos, angle);// 카드의 원래 위치 설정.
                    })
                    .SetEase(Ease.OutSine);

                cardsInHand[cash].OnCardMoveCouroutine();// 카드 이동중에는 상호작용 불가능 설정.
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
        Destroy(currentCard.gameObject);// 카드 삭제.
        CardArrange();
    }
    public void ThrowAwayCard(CardInHand card)// 카드 버리기
    {
        if (card == null) return;
        GameManager.Instance.combatUIController.ThrowCard(card.cardData);// 카드 버리기.
        cardsInHand.Remove(card);// 카드 리스트에서 제거.
        Destroy(card.gameObject);// 카드 삭제.
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
            if (cardsInHand[i].cardData.IsUsable(GameManager.Instance.turnController.battleFlow.currentMana))// 현재 보유 마나를 가져와, 사용 가능한지 확인. (사용 가능시 CanDrag, 불가능시 CanMouseOver)
            {
                cardsInHand[i].SetCardState(CardInHand.CardState.CanDrag);// 카드 상태를 CanDrag로 변경
            }
            else
            {
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
    public void OnMousepoint(PointerEventData eventData)
    {
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
                cardsInHand.Remove(card);
                Destroy(card.gameObject);
            }
        }
        CardArrange();
    }
}
