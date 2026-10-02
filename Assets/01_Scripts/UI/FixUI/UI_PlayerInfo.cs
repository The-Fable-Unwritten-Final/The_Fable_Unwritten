using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerInfo : MonoBehaviour
{
    [Header("Info")]

    [SerializeField] Image shopiaHpBar;
    [SerializeField] Image shopiaHpBarInner;
    [SerializeField] TextMeshProUGUI sophiaHp;
    [SerializeField] Image kylaHpBar;
    [SerializeField] Image kylaHpBarInner;
    [SerializeField] TextMeshProUGUI kylaHp;
    [SerializeField] Image leonHpBar;
    [SerializeField] Image leonHpBarInner;
    [SerializeField] TextMeshProUGUI leonHp;
    [SerializeField] TextMeshProUGUI currentExp;

    [Header("EliteClearInfo")]
    [SerializeField] GameObject courageBadge;
    [SerializeField] GameObject loveBadge;
    [SerializeField] GameObject wisdomBadge;

    [Header("CardInfo")]
    [SerializeField] GameObject currentDeck;
    [SerializeField] GameObject cardBasePrefab;
    [SerializeField] Transform cardsRoot;
    [SerializeField] ScrollRect cardScrollRect;
    [SerializeField] float mouseWheelScrollSpeed = 30f; // 마우스 휠 스크롤 속도
    [SerializeField] int cardsPerRow = 7; // 한 줄당 카드 개수
    [SerializeField] int maxVisibleRows = 2; // 최대 표시 줄 수

    private Dictionary<CharacterClass, TextMeshProUGUI> charInfoText;
    private Dictionary<CharacterClass, Image> charhpBar;
    private Dictionary<CharacterClass, Image> charhpBarInner;
    private Dictionary<CharacterClass, Action<float, float>> hpChangedHandlers = new();
    private Coroutine changeHpCoroutine_Sho;
    private Coroutine changeHpCoroutine_Ky;
    private Coroutine changeHpCoroutine_Le;
    private bool isCardPanelActive = false;

    private void Start()
    {
        charInfoText = new Dictionary<CharacterClass, TextMeshProUGUI>
        {
            { CharacterClass.Sophia, sophiaHp },
            { CharacterClass.Kayla, kylaHp },
            { CharacterClass.Leon, leonHp }
        };

        charhpBar = new Dictionary<CharacterClass, Image>
        {
            { CharacterClass.Sophia, shopiaHpBar },
            { CharacterClass.Kayla, kylaHpBar },
            { CharacterClass.Leon, leonHpBar }
        };

        charhpBarInner = new Dictionary<CharacterClass, Image>
        {
            { CharacterClass.Sophia, shopiaHpBarInner },
            { CharacterClass.Kayla, kylaHpBarInner },
            { CharacterClass.Leon, leonHpBarInner }
        };

        SetEndingBadge();
        UpdatePlayerInfoUI();

        RegisterHpUpdateEvent();
    }

    private void Update()
    {
        // 카드 패널이 활성화되어 있고 마우스 스크롤 시
    }

    void OnDisable()
    {
        foreach (var kvp in hpChangedHandlers)
        {
            CharacterClass character = kvp.Key;
            Action<float, float> handler = kvp.Value;

            if (PlayerManager.Instance.activePlayers.TryGetValue(character, out var playerData))
            {
                playerData.OnHpChanged -= handler;
            }
        }
        hpChangedHandlers.Clear();
    }
    private void RegisterHpUpdateEvent()
    {
        var players = PlayerManager.Instance.activePlayers;

        foreach (var kvp in players)
        {
            CharacterClass character = kvp.Key;
            PlayerData playerData = kvp.Value;

            if (charInfoText.TryGetValue(character, out var textObj))
            {
                // 초기 체력 설정 (애니메이션 없이 바로 적용)
                if (charhpBar.TryGetValue(character, out var hpBar))
                {
                    ChangeHpBar(hpBar, playerData.currentHP, playerData.MaxHP, character, animate: false);
                }

                // 기존 핸들러 제거
                if (hpChangedHandlers.TryGetValue(character, out var oldHandler))
                    playerData.OnHpChanged -= oldHandler;

                // 새 핸들러 생성 및 저장
                Action<float, float> handler = (currentHp, maxHp) =>
                {
                    textObj.text = $"{currentHp}/{maxHp}";
                    ChangeHpBar(charhpBar[character], currentHp, maxHp, character, animate: true);
                };

                hpChangedHandlers[character] = handler;
                playerData.OnHpChanged += handler;
            }
        }
    }

    public void OnSophiaClicked() => ShowCards(CharacterClass.Sophia);
    public void OnKaylaClicked() => ShowCards(CharacterClass.Kayla);
    public void OnLeonClicked() => ShowCards(CharacterClass.Leon);

    // 카드 외 선택 시 CardPanel 비활성화
    public void OnClickCardExept()
    {
        currentDeck.SetActive(false);
        isCardPanelActive = false;
    }

    /// <summary>
    /// 선택한 캐릭터의 현재 보유중인 카드 보여주기
    /// </summary>
    public void ShowCards(CharacterClass characterClass)
    {
        OnClickButtonSound();
        currentDeck.SetActive(true);
        isCardPanelActive = true;

        ClearCards();

        // GridLayoutGroup 설정
        SetupCardGridLayout();

        var deck = CurrentCharacterDeck(characterClass);

        foreach (var card in deck)
        {
            var go = Instantiate(cardBasePrefab, cardsRoot);
            
            // CardInHand를 사용해서 UI 요소 업데이트만 수행
            CardInHand cardInHand = go.GetComponent<CardInHand>();
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
        }

        // 레이아웃 강제 재계산
        Canvas.ForceUpdateCanvases();

        // LayoutElement 업데이트: Content 높이 재설정
        LayoutElement layoutElement = cardsRoot.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            float newHeight = CalculateContentHeight();
            layoutElement.preferredHeight = newHeight;
        }

        // Content 높이 직접 설정 (중요!)
        RectTransform contentRect = cardsRoot.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            float calculatedHeight = CalculateContentHeight();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, calculatedHeight);
        }

        // 스크롤 위치 초기화 (카드 추가 후)
        if (cardScrollRect != null)
        {
            cardScrollRect.verticalNormalizedPosition = 1f; // 맨 위부터 시작
        }
        else
        {
            Debug.LogError("cardScrollRect is NULL!");
        }

        // 애널리틱스
        GameManager.Instance.analyticsLogger.LogDeckButtonClick((int)characterClass + 1);
    }

    /// <summary>
    /// 카드 그리드 레이아웃 설정 (7열 고정)
    /// </summary>
    private void SetupCardGridLayout()
    {
        // cardsRoot null 체크
        if (cardsRoot == null)
        {
            Debug.LogError("cardsRoot is not assigned");
            return;
        }

        // 1. GridLayoutGroup 설정
        GridLayoutGroup gridLayout = cardsRoot.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = cardsRoot.gameObject.AddComponent<GridLayoutGroup>();
        }

        // 중요: cellSize와 spacing 명시적 설정
        gridLayout.cellSize = new Vector2(75f, 135f);  // 카드 크기 (폭 75, 높이 135)
        gridLayout.spacing = new Vector2(150f, 280f);  // 카드 간 간격
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cardsPerRow; // 7열 고정
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.childAlignment = TextAnchor.UpperLeft; // 자식 정렬: 왼쪽 위

        // 2. Viewport 높이 설정 (2줄 크기)
        RectTransform viewportRect = cardScrollRect.viewport as RectTransform;
        if (viewportRect != null)
        {
            float viewportHeight = 2 * 135f + 280f - 40f;  // 정확히 2줄만 표시되도록 조정 (510px)
            viewportRect.sizeDelta = new Vector2(viewportRect.sizeDelta.x, viewportHeight);
        }

        // 3. Content의 RectTransform 설정
        RectTransform contentRect = cardsRoot.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);  // 왼쪽 위를 기준점으로 (중요!)
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, 0);
            
            // Content 너비를 Viewport 너비와 동일하게 설정
            if (viewportRect != null)
            {
                contentRect.sizeDelta = new Vector2(viewportRect.sizeDelta.x, contentRect.sizeDelta.y);
            }
        }

        // 4. LayoutElement 추가: Content 높이 동적 조정 (필수!)
        LayoutElement layoutElement = cardsRoot.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = cardsRoot.gameObject.AddComponent<LayoutElement>();
        }
        layoutElement.preferredHeight = CalculateContentHeight();
        layoutElement.preferredWidth = -1; // 너비는 자동

        // Content 높이 직접 설정
        if (contentRect != null)
        {
            float calculatedHeight = CalculateContentHeight();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, calculatedHeight);
        }

        // 5. ScrollRect 설정
        if (cardScrollRect != null)
        {
            cardScrollRect.horizontal = false;           // 수평 스크롤 비활성화
            cardScrollRect.vertical = true;              // 수직 스크롤 활성화
            cardScrollRect.movementType = ScrollRect.MovementType.Clamped; // 끝에서 멈춤
            cardScrollRect.elasticity = 0.01f;           // 탄성 거의 없음 (되돌아오지 않음)
            cardScrollRect.scrollSensitivity = 0f;       // 마우스 휠 비활성화
            cardScrollRect.inertia = false;              // 관성 비활성화 (드래그 후 즉시 멈춤)
            
            // Content 할당
            if (cardScrollRect.content != cardsRoot)
            {
                cardScrollRect.content = (RectTransform)cardsRoot;
            }
        }
    }

    /// <summary>
    /// Content의 높이 계산 (카드 개수에 따라 동적 조정)
    /// </summary>
    private float CalculateContentHeight()
    {
        int totalCards = cardsRoot.childCount;
        if (totalCards == 0) return 0;

        int rowCount = Mathf.CeilToInt((float)totalCards / cardsPerRow);
        float cardHeight = 135f;   // cellSize.y와 일치
        float spacingY = 280f;     // spacing.y와 일치
        
        // 1줄, 2줄: 100 / 3줄 이상: (rowCount - 2) * (cardHeight + spacing) + 70
        float totalHeight = rowCount <= 2 ? 100f : (rowCount - 2) * (cardHeight + spacingY) + 70f;
        return totalHeight;
    }

    // 현재 캐릭터의 보유 카드 확인
    private List<CardModel> CurrentCharacterDeck(CharacterClass characterClass)
    {
        var player = ProgressDataManager.Instance.PlayerDatas
            .FirstOrDefault(p => p.CharacterClass == characterClass);

        if (player == null) return new();

        var allCards = DataManager.Instance.AllCards;

        return player.currentDeckIndexes
            .Select(i => allCards.FirstOrDefault(c => c.index == i))
            .Where(c => c != null)
            .ToList();
    }

    private void ClearCards()
    {
        foreach (Transform child in cardsRoot)
            Destroy(child.gameObject);
    }

    public void UpdatePlayerInfoUI()
    {
        var players = PlayerManager.Instance.activePlayers;
        var exp = ProgressDataManager.Instance.CurrentExp;

        foreach (var text in charInfoText)
        {
            var character = text.Key;
            var textObj = text.Value;

            bool hasPlayer = players.TryGetValue(character, out var playerData);

            // 부모 오브젝트 활성/비활성
            textObj.transform.parent.gameObject.SetActive(hasPlayer);

            if (hasPlayer)
            {
                textObj.text = $"{playerData.currentHP}/{playerData.MaxHP}";
            }
        }

        currentExp.text = exp.ToString();
    }

    public void ChangeHpBar(Image hpBar, float hp, float maxHp, CharacterClass character, bool animate = true)
    {
        if (hpBar == null) return;

        float hpPercent = (hp / maxHp) * 100f;
        Color barColor = hpPercent > 30f ? new Color(0.49f, 0.70f, 0f, 1f) : new Color(0.682f, 0f, 0f, 1f); // #7DB200 or #AE0000
        Color barColorInner = hpPercent > 30f ? new Color(0.49f, 0.70f, 0f, 0.38f) : new Color(0.682f, 0f, 0f, 0.38f); // #7DB200 or #AE0000 with alpha 0.38
        Color textColor = hpPercent > 30f ? new Color(0.373f, 0.518f, 0.024f, 1f) : new Color(0.682f, 0f, 0f, 1f); // #5F8406 or #AE0000

        // 배경 색상 적용
        hpBar.color = barColor;

        // 내부 체력바 색상 적용 (알파값 0.38)
        if (charhpBarInner.TryGetValue(character, out var hpBarInner))
        {
            hpBarInner.color = barColorInner;
        }

        // 텍스트 색상 적용
        if (charInfoText.TryGetValue(character, out var textObj))
        {
            textObj.color = textColor;
        }

        float targetFill = hp / maxHp;

        // 애니메이션 없이 바로 적용
        if (!animate)
        {
            hpBar.fillAmount = targetFill;
            return;
        }

        // 캐릭터별 코루틴 선택 및 중지
        Coroutine currentCoroutine = character switch
        {
            CharacterClass.Sophia => changeHpCoroutine_Sho,
            CharacterClass.Kayla => changeHpCoroutine_Ky,
            CharacterClass.Leon => changeHpCoroutine_Le,
            _ => null
        };

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        // 새 코루틴 시작 및 저장
        var newCoroutine = StartCoroutine(AnimateHpBarChange(hpBar, targetFill, 0.4f));
        
        switch (character)
        {
            case CharacterClass.Sophia:
                changeHpCoroutine_Sho = newCoroutine;
                break;
            case CharacterClass.Kayla:
                changeHpCoroutine_Ky = newCoroutine;
                break;
            case CharacterClass.Leon:
                changeHpCoroutine_Le = newCoroutine;
                break;
        }
    }

    private IEnumerator AnimateHpBarChange(Image hpBar, float targetFill, float duration)
    {
        float startFill = hpBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            hpBar.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }

        hpBar.fillAmount = targetFill;
    }


    private void SetEndingBadge()
    {
        var stageSetting = ProgressDataManager.Instance;

        courageBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Courage));
        loveBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Love));
        wisdomBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Wisdom));
    }
    public void OnClickButtonSound()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
    }
}
