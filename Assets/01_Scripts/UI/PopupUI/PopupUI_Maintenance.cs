using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 캠프 정비 팝업 UI
/// 캐릭터의 덱에서 카드 1개를 선택하여 제거할 수 있는 인터페이스
/// 5x2 그리드로 카드 표시, 스크롤 가능
/// </summary>
public class PopupUI_Maintenance : BasePopupUI
{
    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GridLayoutGroup cardGrid;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardBasePrefab; // CardBase 프리팹
    [SerializeField] private Button removeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform cardToHere; // 카드 제거 시 이동할 위치 (애니메이션용)
    [SerializeField] private Animator burnAnimation; // 카드 제거 시 불타는 애니메이션

    private CampCharSelection currentCampCharSelection;
    private CardModel selectedCard;
    private Button selectedCardButton;
    private GameObject cardCopyObject; // 선택된 카드의 복사본
    private Coroutine burnCoroutine; // Burn 애니메이션 코루틴
    private bool isRemoving = false; // 카드 제거 중 플래그
    [SerializeField] private Sprite noBurnSprite; // 불타는 애니메이션이 끝난 후 표시할 스프라이트 (카드가 다 타기 전에 팝업이 닫혀버린 경우, 애니메이션 이미지 초기화용.)

    private List<Button> cardButtons = new List<Button>();
    private void Start()
    {
        // 버튼 이벤트 등록
        if (removeButton != null)
            removeButton.onClick.AddListener(OnRemoveButtonClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    /// <summary>
    /// 정비 팝업 열기 (CampCharSelection만 지정)
    /// </summary>
    public void ShowMaintenance(CampCharSelection campCharSelection)
    {
        if (campCharSelection == null)
        {
            Debug.LogError("[PopupUI_Maintenance] CampCharSelection이 null입니다.");
            return;
        }

        if (campCharSelection.character == null)
        {
            Debug.LogError("[PopupUI_Maintenance] 캐릭터가 null입니다.");
            return;
        }

        currentCampCharSelection = campCharSelection;
        selectedCard = null;
        selectedCardButton = null;
        isRemoving = false;
        // 카드 리스트 표시
        DisplayCharacterDeck();

        if (burnAnimation != null && noBurnSprite != null)
        {
            Image burnImage = burnAnimation.GetComponent<Image>();
            if (burnImage != null)
            {
                burnImage.sprite = noBurnSprite;
            }
        }
    }

    /// <summary>
    /// 캐릭터의 덱 카드를 UI에 표시
    /// </summary>
    private void DisplayCharacterDeck()
    {
        if (currentCampCharSelection == null || currentCampCharSelection.character == null || currentCampCharSelection.character.currentDeck == null)
        {
            Debug.LogWarning("[PopupUI_Maintenance] 표시할 덱이 없습니다.");
            return;
        }

        PlayerData character = currentCampCharSelection.character;

        // 기존 카드 버튼 정리 (cardContainer의 모든 자식 제거)
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        cardButtons.Clear();

        // 각 카드마다 버튼 생성
        foreach (var card in character.currentDeck)
        {
            if (card == null) continue;

            GameObject cardButtonObj = Instantiate(cardBasePrefab, cardContainer);
            
            // CardInHand를 사용해서 UI 요소 업데이트만 수행
            CardInHand cardInHand = cardButtonObj.GetComponent<CardInHand>();
            if (cardInHand != null)
            {
                // 카드 데이터 설정 (이미지, 이름, 비용, 설명)
                cardInHand.cardData = card;
                cardInHand.UpdateCardImage();
                // 캠프 씬에서는 battleFlow가 없으므로 플레이스홀더를 공백으로 표시
                cardInHand.UpdateCardInfoOnlyUI();
            }
            
            // Button 컴포넌트 확인, 없으면 추가 (정비 UI 클릭은 Button으로만 처리)
            Button cardBtn = cardButtonObj.GetComponent<Button>();
            if (cardBtn == null)
            {
                cardBtn = cardButtonObj.AddComponent<Button>();
            }

            // CardInHand의 GetCoverIllust()로 IllustCover를 가져와서 targetGraphic 설정
            if (cardInHand != null)
            {
                GameObject illustCoverObj = cardInHand.GetCoverIllust();
                if (illustCoverObj != null)
                {
                    Image illustCover = illustCoverObj.GetComponent<Image>();
                    if (illustCover != null)
                    {
                        cardBtn.targetGraphic = illustCover;
                        
                        // 초기 상태: IllustCover의 alpha를 0으로 설정 (선택되지 않은 상태)
                        Color color = illustCover.color;
                        color.a = 0f;
                        illustCover.color = color;
                    }
                }
            }

            // UIButtonHoverScaleCustom 컴포넌트 추가 (마우스 올라가면 스케일 변경)
            UIButtonHoverScaleCustom hoverScale = cardButtonObj.GetComponent<UIButtonHoverScaleCustom>();
            if (hoverScale == null)
            {
                hoverScale = cardButtonObj.AddComponent<UIButtonHoverScaleCustom>();
            }
            hoverScale.originalScale = 2.35f;  // 초기 스케일
            hoverScale.targetScale = 2.6f;     // 호버 시 스케일
            
            // CardInHand 컴포넌트 제거 (이벤트 핸들러 비활성화, UI 업데이트는 이미 완료됨)
            if (cardInHand != null)
            {
                DestroyImmediate(cardInHand);
            }

            // 클로저 문제 방지 위해 로컬 변수 사용
            CardModel cardData = card;
            cardBtn.onClick.AddListener(() => OnCardSelected(cardData, cardBtn));

            cardButtons.Add(cardBtn);
        }

        // 레이아웃 재계산 (스크롤이 작동하도록)
        if (scrollRect != null && cardContainer is RectTransform contentRect)
        {
            // Content의 Anchor: 좌우 스트래치, 상단 고정
            contentRect.anchorMin = new Vector2(0, 1);   // Left-Top
            contentRect.anchorMax = new Vector2(1, 1);   // Right-Top
            contentRect.pivot = new Vector2(0.5f, 1);    // Top-Center
            
            // ViewPort 높이 가져오기
            RectTransform viewportRect = scrollRect.viewport;
            float viewportHeight = viewportRect != null ? viewportRect.rect.height : 700f;
            
            // 카드 개수에 따라 줄(rows) 계산
            int cardCount = cardButtons.Count;
            int columns = cardGrid.constraintCount > 0 ? cardGrid.constraintCount : 5;
            int rows = Mathf.CeilToInt((float)cardCount / columns);
            
            // 높이 계산: 기본 viewportHeight, 추가 줄마다 viewportHeight * 0.5 추가
            float contentHeight = viewportHeight;
            if (rows > 2)
            {
                contentHeight += viewportHeight * 0.5f * (rows - 2);
            }
            
            // Top-Left에 고정되도록 offset 설정
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            
            // LayoutRebuilder 후 높이 재설정 (SizeDelta로 높이만 설정, 너비는 Stretch)
            contentRect.sizeDelta = new Vector2(0, contentHeight);
            
            // 스크롤 초기 위치를 맨 위로 설정
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    /// <summary>
    /// 카드 선택
    /// </summary>
    private void OnCardSelected(CardModel card, Button cardButton)
    {
        // 카드 제거 중이면 선택 불가
        if (isRemoving)
        {
            return;
        }

        selectedCard = card;
        selectedCardButton = cardButton;

        // 기존 복사본 제거
        if (cardCopyObject != null)
        {
            Destroy(cardCopyObject);
        }

        // 복사본 카드 생성 (cardToHere의 하위 오브젝트로)
        cardCopyObject = Instantiate(cardBasePrefab, cardToHere);
        CardInHand cardInHand = cardCopyObject.GetComponent<CardInHand>();
        if (cardInHand != null)
        {
            cardInHand.cardData = card;
            cardInHand.UpdateCardImage();
            cardInHand.UpdateCardInfoOnlyUI();
        }

        // CardInHand 컴포넌트 제거 (이벤트 핸들러 비활성화)
        if (cardInHand != null)
        {
            DestroyImmediate(cardInHand);
        }

        // 복사본의 초기 위치 및 스케일 설정
        RectTransform copyRect = cardCopyObject.GetComponent<RectTransform>();
        if (copyRect != null)
        {
            // 초기 위치: y축은 최종 위치에서 약 150 위, x축은 0 (부모 기준)
            copyRect.anchoredPosition = new Vector2(0f, 150f);
            
            // 스케일 설정
            copyRect.localScale = Vector3.one * 2.66f;
            
            // DOTween으로 최종 위치로 이동 (0, 0) - 1초에 걸쳐 이동
            copyRect.DOAnchorPos(Vector2.zero, 1f).SetEase(Ease.OutCubic);
        }

        // #313114를 Color로 변환
        if (!ColorUtility.TryParseHtmlString("#313114", out Color selectedColor))
        {
            selectedColor = new Color(0x31 / 255f, 0x31 / 255f, 0x14 / 255f, 1f);
        }
        selectedColor.a = 1f;

        // 모든 카드의 targetGraphic (IllustCover) 색상 업데이트
        for (int i = 0; i < cardButtons.Count; i++)
        {
            Button btn = cardButtons[i];
            if (btn.targetGraphic != null)
            {
                if (btn == cardButton)
                {
                    // 선택된 카드: #313114 (alpha 1)
                    btn.targetGraphic.color = selectedColor;
                }
                else
                {
                    // 선택되지 않은 카드: alpha 0
                    Color deselectedColor = btn.targetGraphic.color;
                    deselectedColor.a = 0f;
                    btn.targetGraphic.color = deselectedColor;
                }
            }
        }
    }

    /// <summary>
    /// 제거 버튼 클릭 (즉시 제거 대신 임시 저장)
    /// </summary>
    private void OnRemoveButtonClicked()
    {
        // 이미 제거 중이면 return
        if (isRemoving)
        {
            return;
        }

        if (selectedCard == null)
        {
            Debug.LogWarning("[PopupUI_Maintenance] 선택된 카드가 없습니다.");
            return;
        }

        if (currentCampCharSelection == null || currentCampCharSelection.character == null)
        {
            Debug.LogError("[PopupUI_Maintenance] 캐릭터가 null입니다.");
            return;
        }

        // 제거 상태 플래그 설정
        isRemoving = true;

        // 선택된 카드 버튼 제거
        if (selectedCardButton != null)
        {
            Destroy(selectedCardButton.gameObject);
        }

        burnAnimation.SetTrigger("Burn");

        // 기존 코루틴이 있으면 중지
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }

        // 애니메이션 완료 후 콜백 실행
        burnCoroutine = StartCoroutine(WaitForBurnAnimationComplete());

        // 카드를 제거 대기 목록에 등록 (캠프 나갈 때 일괄 제거)
        currentCampCharSelection.AddCardToRemoval(selectedCard);

        //Debug.Log($"[PopupUI_Maintenance] {selectedCard.cardName}를 {currentCampCharSelection.character.CharacterName}의 제거 목록에 등록 (캠프 나갈 때 삭제됨)");

        // 선택 초기화
        selectedCard = null;
        selectedCardButton = null;

        // 정비 완료 콜백
        NotifyMaintenanceComplete();
    }

    /// <summary>
    /// 정비 완료 알림 (CampCharSelection 직접 호출)
    /// </summary>
    private void NotifyMaintenanceComplete()
    {
        if (currentCampCharSelection != null)
        {
            currentCampCharSelection.OnMaintenanceComplete();
        }
        else
        {
            Debug.LogError("[PopupUI_Maintenance] CampCharSelection 참조가 없습니다.");
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public override void Close()
    {
        // 코루틴 정리
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
            burnCoroutine = null;
        }

        // 선택 및 참조 초기화
        selectedCard = null;
        selectedCardButton = null;
        currentCampCharSelection = null;
        isRemoving = false;

        // 복사본 카드 정리
        if (cardCopyObject != null)
        {
            Destroy(cardCopyObject);
            cardCopyObject = null;
        }

        base.Close();
    }
    public void OnBurnAnimationComplete()
    {
        // 불타는 애니메이션이 끝난 후, 복사본 카드 제거 및 1초 후 팝업 닫기
        if (cardCopyObject != null)
        {
            Destroy(cardCopyObject);
            cardCopyObject = null;
        }

        // 1초 대기 후 팝업 닫기
        StartCoroutine(CloseAfterDelay(1f));
    }

    /// <summary>
    /// 일정 시간 후 팝업 닫기
    /// </summary>
    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }

    /// <summary>
    /// Burn 애니메이션 완료 대기 코루틴
    /// </summary>
    private IEnumerator WaitForBurnAnimationComplete()
    {
        // 애니메이션이 시작될 때까지 대기
        yield return null;

        // 현재 재생 중인 애니메이션 상태 가져오기
        AnimatorStateInfo stateInfo = burnAnimation.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        // Burn 애니메이션이 완료될 때까지 대기
        yield return new WaitForSeconds(animationLength);

        // 애니메이션 완료 콜백
        OnBurnAnimationComplete();
    }
}
