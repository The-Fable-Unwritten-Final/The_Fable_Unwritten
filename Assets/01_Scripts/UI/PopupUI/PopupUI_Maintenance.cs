using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject cardButtonPrefab; // 카드 버튼 프리팹
    [SerializeField] private Button removeButton;
    [SerializeField] private Button closeButton;

    private PlayerData currentCharacter;
    private CampCharSelection currentCampCharSelection;
    private CardModel selectedCard;
    private Button selectedCardButton;

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
    /// 정비 팝업 열기 (캐릭터 + CampCharSelection 지정)
    /// </summary>
    public void ShowMaintenance(PlayerData character, CampCharSelection campCharSelection)
    {
        if (character == null)
        {
            Debug.LogError("[PopupUI_Maintenance] 캐릭터가 null입니다.");
            return;
        }

        if (campCharSelection == null)
        {
            Debug.LogError("[PopupUI_Maintenance] CampCharSelection이 null입니다.");
            return;
        }

        currentCharacter = character;
        currentCampCharSelection = campCharSelection;
        selectedCard = null;
        selectedCardButton = null;

        // 카드 리스트 표시
        DisplayCharacterDeck();

        // 부모 클래스의 Open() 호출
        Open();
    }

    /// <summary>
    /// 캐릭터의 덱 카드를 UI에 표시
    /// </summary>
    private void DisplayCharacterDeck()
    {
        if (currentCharacter == null || currentCharacter.currentDeck == null)
        {
            Debug.LogWarning("[PopupUI_Maintenance] 표시할 덱이 없습니다.");
            return;
        }

        // 기존 카드 버튼 정리
        foreach (var button in cardButtons)
        {
            Destroy(button.gameObject);
        }
        cardButtons.Clear();

        // 각 카드마다 버튼 생성
        foreach (var card in currentCharacter.currentDeck)
        {
            if (card == null) continue;

            GameObject cardButtonObj = Instantiate(cardButtonPrefab, cardContainer);
            Button cardBtn = cardButtonObj.GetComponent<Button>();

            if (cardBtn == null)
            {
                Debug.LogError("[PopupUI_Maintenance] 카드 버튼 프리팹에 Button 컴포넌트가 없습니다.");
                Destroy(cardButtonObj);
                continue;
            }

            // 클로저 문제 방지 위해 로컬 변수 사용
            CardModel cardData = card;
            cardBtn.onClick.AddListener(() => OnCardSelected(cardData, cardBtn));

            // 카드 정보 표시 (Image, Text 등은 프리팹에 설정되어 있다고 가정)
            Image cardImage = cardButtonObj.GetComponent<Image>();
            if (cardImage != null && card.illustration != null)
            {
                cardImage.sprite = card.illustration;
            }

            Text cardNameText = cardButtonObj.GetComponentInChildren<Text>();
            if (cardNameText != null)
            {
                cardNameText.text = card.cardName;
            }

            cardButtons.Add(cardBtn);
        }

        Debug.Log($"[PopupUI_Maintenance] {currentCharacter.CharacterName}의 덱 카드 {currentCharacter.currentDeck.Count}개 표시");
    }

    /// <summary>
    /// 카드 선택
    /// </summary>
    private void OnCardSelected(CardModel card, Button cardButton)
    {
        // 이전 선택 취소
        if (selectedCardButton != null)
        {
            ColorBlock colors = selectedCardButton.colors;
            colors.normalColor = Color.white;
            selectedCardButton.colors = colors;
        }

        // 새로운 카드 선택
        selectedCard = card;
        selectedCardButton = cardButton;

        // 선택된 카드 강조 표시
        ColorBlock selectedColors = cardButton.colors;
        selectedColors.normalColor = Color.yellow;
        cardButton.colors = selectedColors;

        Debug.Log($"[PopupUI_Maintenance] 카드 선택: {card.cardName} (Index: {card.index})");
    }

    /// <summary>
    /// 제거 버튼 클릭
    /// </summary>
    private void OnRemoveButtonClicked()
    {
        if (selectedCard == null)
        {
            Debug.LogWarning("[PopupUI_Maintenance] 선택된 카드가 없습니다.");
            return;
        }

        if (currentCharacter == null)
        {
            Debug.LogError("[PopupUI_Maintenance] 캐릭터가 null입니다.");
            return;
        }

        // 덱에서 카드 제거
        currentCharacter.currentDeck.Remove(selectedCard);

        // 덱 인덱스 업데이트 (저장용)
        currentCharacter.UpdateCurrentDeckIndexes();

        Debug.Log($"[PopupUI_Maintenance] {selectedCard.cardName}를 {currentCharacter.CharacterName}의 덱에서 제거 (남은 카드: {currentCharacter.currentDeck.Count}개)");

        // 카드 리스트 새로고침
        DisplayCharacterDeck();

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
        // 선택 및 참조 초기화
        selectedCard = null;
        selectedCardButton = null;
        currentCampCharSelection = null;
        currentCharacter = null;

        base.Close();
    }
}
