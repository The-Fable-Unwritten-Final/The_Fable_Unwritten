using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class StyleDisplay : MonoBehaviour // 기존 팝업 방식(basepopup 상속)에서 노드 선택 씬과 통합 시키며 monobehaviour로 변경.
{
    // 문체의 UI를 담당하는 스크립트 (ui 적인 조작을 메인으로 사용 => 노드 선택 씬에서만 존재)

    // 상단 잉크 표시
    [Header("Upper UI")]
    [SerializeField] Image inkHolder; // 잉크 병 (현재 잉크 보유량을 Fill amount 를 통해 시각적으로 표현)
    public TextMeshProUGUI inkText;

    // 중단 현재 문체 부분
    [Header("Main UI")]
    [SerializeField] Image curStyleIcon; // 현재 문체를 표시하는 UI의 아이콘
    [SerializeField] Image curStyleUI; // 현재 문체를 표시하는 UI의 테두리
    [SerializeField] TextMeshProUGUI curName;
    [SerializeField] Image curNameImage; // 현재 문체 이름 이미지 (한글, 영문, 일문)
    [SerializeField] TextMeshProUGUI curFlav;
    //[SerializeField] TextMeshProUGUI curEff1;
    //[SerializeField] TextMeshProUGUI curEff2;
    [SerializeField] TextMeshProUGUI upgrEff1;
    [SerializeField] GameObject ClickToUp1;
    [SerializeField] GameObject FullToUp1;
    [SerializeField] Image upgrSprite1;
    [SerializeField] GameObject Eff1InkImage;
    [SerializeField] TextMeshProUGUI Eff1Ink;

    [SerializeField] TextMeshProUGUI upgrEff2;
    [SerializeField] GameObject ClickToUp2;
    [SerializeField] GameObject FullToUp2;
    [SerializeField] Image upgrSprite2;
    [SerializeField] GameObject Eff2InkImage;
    [SerializeField] TextMeshProUGUI Eff2Ink;
    
    // 하단 문체 선택 부분
    [Header("Bottom UI")]
    public Button prevButton;
    public Button nextButton;
    public Sprite lockedButtonImage;        // 잠긴 문체 교환 버튼
    public Sprite nonFullUpgradeButtonImage;// 최대 강화가 아닌 문체 효과 강화 버튼
    public Sprite FullUpgradeButtomImage;   // 최대 강화 상태, 문체 효과 강화 버튼
    public RectTransform buttonContainer;
    public GameObject buttonPrefab;
    public TextMeshProUGUI pageText; // 페이지 표시 텍스트 (예: "1 / 3")
    public int buttonsPerPage = 5;
    public float spacingBetweenButton = 15;

    private List<StyleButton> allButtons = new List<StyleButton>();
    private int currentPage = 0;
    private int totalPages = 0;
    private float buttonHeight;
    void Start()
    {
        // 일반 텍스트의 로컬라이제이션 데이터를 받아오는 경우 GetValueFullTextEff 가 아니라 LocaleDataManager.GetLocalizedStyleEffect("key") 형식으로 가져올 것
        CreateButtons(DataManager.Instance.styleDefs.Count);
    }

    void OnEnable()
    {
        StyleManager.Instance.OnInkChange += InkChange;
        StyleManager.Instance.display = this;
        // 스테이지 번호 확인 후, 기본 문체 적용 + UI 활성화 조정
        InkChange(ProgressDataManager.Instance.inkAmount);
    }
    void Oisable()
    {
        StyleManager.Instance.OnInkChange -= InkChange;
        StyleManager.Instance.display = null;     
    }

    /// <summary>
    /// '문체 효과'가 고정 효과(등급 상승x)가 아닐 경우, 받아온 string 값의 ## << 부분에 value값을 변환해서 대입 후 출력
    /// </summary>
    private string GetValueFullTextEff(StyleDefinition sty, string key, bool isPlus)
    {
        if (sty == null) return "";
        string txt = LocaleDataManager.GetLocalizedStyleEffect(key);
        string valueStr;
        StyleEffect eff;

        if (isPlus)
            eff = sty.plusTiers[sty.currentPlus - 1].effects[0];
        else
            eff = sty.minusTiers[sty.currentMinus - 1].effects[0];

        switch (eff.operation)
        {
            case EffectOperation.MulPercent:
                // (eff.value - 1) * 100 을 백분율로 표기
                float rawPercent = (eff.value - 1f) * 100f;
                rawPercent = Mathf.Round(rawPercent * 1000f) / 1000f; // 소수점 오차 정리
                // 양수/음수 부호 유지, 크기는 올림 처리(예: 1.1 -> 10 -> +10%)
                int pct = Mathf.CeilToInt(Mathf.Abs(rawPercent));
                valueStr = (rawPercent >= 0 ? "+" : "-") + pct.ToString() + "%";
                break;

            case EffectOperation.Add:
                int AddVal = Mathf.CeilToInt(eff.value);
                valueStr = "+" + AddVal.ToString();
                break;

            case EffectOperation.Minus:
                int MinusVal = Mathf.CeilToInt(eff.value);
                valueStr = "-" + MinusVal.ToString();
                break;

            case EffectOperation.Set:
                int setVal = Mathf.CeilToInt(eff.value);
                valueStr = setVal.ToString();
                break;

            case EffectOperation.RandomRange:
                int min = Mathf.CeilToInt(eff.valueRange.x);
                int max = Mathf.CeilToInt(eff.valueRange.y);
                valueStr = $"{min} ~ {max}";
                break;

            default:
                // 안전한 기본 포맷
                valueStr = eff.value.ToString();
                break;
        }

        if (txt.Contains("##"))
            txt = txt.Replace("##", valueStr); // value 값이 변하는 경우 csv의 텍스트 중간에 '##' 가 존재.

        return txt;
    }
    /// <summary>
    /// '문체 효과 강화' 부분에서 다음 강화 단계 효과를 표시 (최대 강화가 아닐 시 초록/빨강 으로 강조 표시 추가)
    /// </summary>
    private string GetValueFullTextUpgraded(StyleDefinition sty ,string key, bool isPlus)
    {
        if (sty == null) return "";
        string txt = LocaleDataManager.GetLocalizedStyleEffect(key);
        string valueStr;
        bool isFullUpgrade = true;
        StyleEffect eff;

        if (isPlus)
        {
            eff = sty.plusTiers[sty.currentPlus - 1].effects[0];
            if (sty.plusTiers.Count > sty.currentPlus) // 최대 강화가 아닌 경우
            {
                isFullUpgrade = false;
            }
        }
        else
        {
            eff = sty.minusTiers[sty.currentMinus - 1].effects[0];
            if (sty.minusTiers.Count > sty.currentMinus) // 최대 강화가 아닌 경우
            {
                isFullUpgrade = false;
            }
        }

        switch (eff.operation)
        {
            case EffectOperation.MulPercent:
                // (eff.value - 1) * 100 을 백분율로 표기
                float rawPercent = (eff.value - 1f) * 100f;
                rawPercent = Mathf.Round(rawPercent * 1000f) / 1000f; // 소수점 오차 정리
                // 양수/음수 부호 유지, 크기는 올림 처리(예: 1.1 -> 10 -> +10%)
                int pct = Mathf.CeilToInt(Mathf.Abs(rawPercent));
                valueStr = (rawPercent >= 0 ? "+" : "-") + pct.ToString() + "%";
                break;

            case EffectOperation.Add:
                int AddVal = Mathf.CeilToInt(eff.value);
                valueStr = "+"+ AddVal.ToString();
                break;

            case EffectOperation.Minus:
                int MinusVal = Mathf.CeilToInt(eff.value);
                valueStr =  "-"+ MinusVal.ToString();
                break;

            case EffectOperation.Set:
                int setVal = Mathf.CeilToInt(eff.value);
                valueStr = setVal.ToString();
                break;

            case EffectOperation.RandomRange:
                int min = Mathf.CeilToInt(eff.valueRange.x);
                int max = Mathf.CeilToInt(eff.valueRange.y);
                valueStr = $"{min} ~ {max}";
                break;

            default:
                // 안전한 기본 포맷
                valueStr = eff.value.ToString();
                break;
        }

        if (!isFullUpgrade) // 최대 강화가 아닐 경우, 강화 될 수치를 초록/빨간색으로 표시
        {
            string color = isPlus ? "green" : "red";
            valueStr = $"<color={color}>{valueStr}</color>";   
        }
        if (txt.Contains("##"))
            txt = txt.Replace("##", valueStr); // value 값이 변하는 경우 csv의 텍스트 중간에 '##' 가 존재.
        return txt;
    }

    // 하단 버튼 구간 //
    public void CreateButtons(int totalCount)
    {
        // 버튼 초기화 (만약 기존의 데이터가 남아 있을 경우 대비)
        foreach (var btn in allButtons)
            Destroy(btn);
        allButtons.Clear();

        // 버튼 생성
        for (int i = 0; i < totalCount - 1; i++)
        {
            GameObject newBtn = Instantiate(buttonPrefab, buttonContainer);
            allButtons.Add(newBtn.GetComponent<StyleButton>());
        }

        // 버튼 크기 가져오기
        buttonHeight = buttonPrefab.GetComponent<RectTransform>().rect.height; 

        // Spacing 가져오기
        spacingBetweenButton = buttonContainer.GetComponent<VerticalLayoutGroup>().spacing;

        totalPages = Mathf.CeilToInt((float)(totalCount - 1) / buttonsPerPage);
        currentPage = 0;

        // 버튼들에 문체 정보 입력
        var defs = DataManager.Instance.styleDefs;
        int buttonIndex = 0;

        for (int i = 0; i < totalCount; i++)
        {
            if (!defs[i].isUnlocked) continue; // 잠긴 문체 스킵
            if (defs[i].styleId == StyleManager.Instance.CurrentState.styleId)
            {
                // 현재 적용 문체
                UpdateCurrentStyle(defs[i]);
                continue;
            }

            if (buttonIndex < allButtons.Count)
            {
                StyleDefinition sty = defs[i];
                allButtons[buttonIndex].SetDefinition(sty, GetValueFullTextEff(sty, sty.plusEffectDescription, true), GetValueFullTextEff(sty, sty.minusEffectEffectDesc, false)); // 문체 설정 및 텍스트 입력
                allButtons[buttonIndex].GetComponent<StyleButtonHoverScale>().SetStyle(sty);
                allButtons[buttonIndex].GetComponent<Image>().sprite = sty.buttonSprite;
                allButtons[buttonIndex].styleIcon.sprite = sty.buttonIconSprite;
                allButtons[buttonIndex].styleIcon.gameObject.SetActive(true); // 아이콘 활성화
                buttonIndex++;
            }
        }
        // 문체가 들어있지 않은 버튼들은 잠김 상태 적용 (상호작용 off + 이미지 변경)
        foreach (var buttons in allButtons)
        {
            if (buttons.definition == null)
            {
                buttons.GetComponent<Button>().interactable = false;
                buttons.GetComponent<Image>().sprite = lockedButtonImage;
                buttons.styleIcon.gameObject.SetActive(false); // 아이콘 비활성화
                buttons.TurnOffAll();
            }
        }
        // 현재 문체를 표시하는 UI 업데이트
        if (StyleManager.Instance.CurrentState != null)
        {
            var currentDef = DataManager.Instance.styleDefs.Find(def => def.styleId == StyleManager.Instance.CurrentState.styleId);
            if (currentDef != null)
            {
                curStyleIcon.sprite = currentDef.buttonIconSprite; // 현재 문체 아이콘 업데이트
                curStyleUI.sprite = currentDef.currentStyleSprite; // 현재 문체 UI 테두리 업데이트
            }
        }
        UpdatePage();
    }
    public void UpdateButton(int totalCount)
    {
        // 전체 버튼 내용 초기화
        for (int i = 0; i < totalCount - 1; i++)
            allButtons[i].definition = null;

        var defs = DataManager.Instance.styleDefs;
        int buttonIndex = 0;

        for (int i = 0; i < totalCount; i++)
        {
            if (!defs[i].isUnlocked) continue; // 잠긴 문체 스킵
            if (defs[i].styleId == StyleManager.Instance.CurrentState.styleId)
            {
                // 현재 적용 문체
                UpdateCurrentStyle(defs[i]);
                continue;
            }

            if (buttonIndex < allButtons.Count)
            {
                StyleDefinition sty = defs[i];
                allButtons[buttonIndex].SetDefinition(sty, GetValueFullTextEff(sty, sty.plusEffectDescription, true), GetValueFullTextEff(sty, sty.minusEffectEffectDesc, false)); // 문체 설정 및 텍스트 입력
                allButtons[buttonIndex].GetComponent<StyleButtonHoverScale>().SetStyle(sty);
                allButtons[buttonIndex].GetComponent<Image>().sprite = sty.buttonSprite;
                allButtons[buttonIndex].styleIcon.sprite = sty.buttonIconSprite;
                allButtons[buttonIndex].styleIcon.gameObject.SetActive(true); // 아이콘 활성화
                buttonIndex++;
            }
        }
        // 문체가 들어있지 않은 버튼들은 잠김 상태 적용 (상호작용 off + 이미지 변경)
        foreach (var buttons in allButtons)
        {
            if (buttons.definition == null)
            {
                buttons.GetComponent<Button>().interactable = false;
                buttons.GetComponent<Image>().sprite = lockedButtonImage;
                buttons.styleIcon.gameObject.SetActive(false); // 아이콘 비활성화
                buttons.TurnOffAll();
            }
        }

        // 현재 문체 UI는 UpdateCurrentStyle()의 _ApplyCurrentStyleChanges()에서 애니메이션과 함께 업데이트됨

        UpdatePage();
    }
    public void OnUpgradeClick(bool isPlus)
    {
        StyleManager.Instance.isPlus = isPlus;
        UIManager.Instance.ShowPopupByName("PopupUI_Upgrade");
    }
    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }
    private void UpdatePage()
    {
        // 정확한 페이지 높이 계산: 버튼 * 높이 + 사이 간격 * 버튼 개수
        float pageHeight = buttonsPerPage * buttonHeight + buttonsPerPage * spacingBetweenButton;
        float targetY = currentPage * pageHeight;

        // 이전 애니메이션을 중단하고 새 애니메이션 시작
        buttonContainer.DOKill();
        buttonContainer.DOAnchorPosY(targetY, 0.6f).SetEase(Ease.OutCubic);

        // 페이지 끝 여부에 따라 버튼 활성/비활성 처리
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < totalPages - 1;

        // 현재 페이지 표시 업데이트
        pageText.text = $"{currentPage + 1}/{totalPages}";
    }
    public void InkChange(int amount)
    {
        // inkGauge의 fillAmount를 현재 잉크 양에 맞게 부드럽게 변화
        float targetFill = Mathf.Clamp01((float)amount / 10);
        inkHolder.DOFillAmount(targetFill, 0.5f).SetEase(Ease.OutCubic);
        inkText.text = $"{amount}/{ProgressDataManager.Instance.maxInkAmount}";
    } 
    public void UpdateCurrentStyle(StyleDefinition sty) // 현재 문체 표시 부분의(중단 UI 전부) 정보 업데이트
    {
        StartCoroutine(UpdateCurrentStyleWithAnimation(sty));
    }

    private IEnumerator UpdateCurrentStyleWithAnimation(StyleDefinition sty)
    {
        // 4개 오브젝트의 RectTransform 가져오기
        RectTransform curStyleUIRect = curStyleUI.GetComponent<RectTransform>();
        RectTransform upgrSprite1Rect = upgrSprite1.GetComponent<RectTransform>();
        RectTransform upgrSprite2Rect = upgrSprite2.GetComponent<RectTransform>();

        RectTransform[] targetRects = new RectTransform[] 
        {
            curStyleUIRect,
            upgrSprite1Rect,
            upgrSprite2Rect
        };

        // 현재 위치 저장
        Vector2[] originalPositions = new Vector2[targetRects.Length];
        for (int i = 0; i < targetRects.Length; i++)
        {
            originalPositions[i] = targetRects[i].anchoredPosition;
        }

        // 부모의 LayoutGroup 임시 비활성화 (자동 업데이트 방지)
        LayoutGroup[] parentLayoutGroups = new LayoutGroup[targetRects.Length];
        for (int i = 0; i < targetRects.Length; i++)
        {
            if (targetRects[i].parent != null)
            {
                parentLayoutGroups[i] = targetRects[i].parent.GetComponent<LayoutGroup>();
                if (parentLayoutGroups[i] != null)
                    parentLayoutGroups[i].enabled = false;
            }
        }

        // 1. 왼쪽으로 화면 밖으로 이동
        float animationDuration = 0.75f;
        // 원래 위치에서 더 왼쪽으로 충분히 나가기 (원래 위치 - 1000)
        float screenOffsetX = originalPositions[0].x - 1000f;

        foreach (var rect in targetRects)
        {
            rect.DOAnchorPosX(screenOffsetX, animationDuration).SetEase(Ease.Linear);
        }

        // 4/5 시간 대기  - 이 동안 화면 밖으로 이동 중
        yield return new WaitForSeconds(animationDuration*4 / 5f);

        // 2. 화면 밖에서 시각적 변경 수행
        _ApplyCurrentStyleChanges(sty);

        // 남은 1/5 시간 대기  - 계속 이동하다가 완전히 화면 밖으로
        yield return new WaitForSeconds(animationDuration / 5f);

        // 3. 원래 위치로 복귀 (Linear: 일정한 속도)
        for (int i = 0; i < targetRects.Length; i++)
        {
            targetRects[i].DOAnchorPosX(originalPositions[i].x, animationDuration).SetEase(Ease.Linear);
        }

        // 애니메이션 완료 대기
        yield return new WaitForSeconds(animationDuration);

        // 부모의 LayoutGroup 다시 활성화
        for (int i = 0; i < parentLayoutGroups.Length; i++)
        {
            if (parentLayoutGroups[i] != null)
                parentLayoutGroups[i].enabled = true;
        }
    }

    private void _ApplyCurrentStyleChanges(StyleDefinition sty)
    {
        // 현재 문체 아이콘 및 UI 테두리 업데이트
        curStyleIcon.sprite = sty.buttonIconSprite;
        curStyleUI.sprite = sty.currentStyleSprite;

        // 로케일에 따른 현재 문체 이름 이미지 업데이트
        Sprite targetSprite = null;
        switch (LocaleDataManager.CurrentLocale)
        {
            case LocaleDataManager.SystemLocale.Korean:
                targetSprite = sty.krNameSprite;
                break;
            case LocaleDataManager.SystemLocale.Japanese:
                targetSprite = sty.jpNameSprite;
                break;
            case LocaleDataManager.SystemLocale.English:
            default:
                targetSprite = sty.enNameSprite;
                break;
        }
        
        curNameImage.sprite = targetSprite;
        
        // 스프라이트 크기를 너비 범위 내에서 조정 (종횡비 유지)
        if (targetSprite != null)
        {
            float minWidth, maxWidth;
            
            // 로케일별로 다른 너비 범위 설정
            switch (LocaleDataManager.CurrentLocale)
            {
                case LocaleDataManager.SystemLocale.Korean:
                    minWidth = 240f;
                    maxWidth = 240f;  // 고정값
                    break;
                case LocaleDataManager.SystemLocale.Japanese:
                    minWidth = 255f;
                    maxWidth = 275f;
                    break;
                case LocaleDataManager.SystemLocale.English:
                default:
                    minWidth = 280f;
                    maxWidth = 300f;
                    break;
            }
            
            float spriteWidth = targetSprite.rect.width;
            float spriteHeight = targetSprite.rect.height;
            
            float targetWidth = Mathf.Clamp(spriteWidth, minWidth, maxWidth);
            float aspectRatio = spriteHeight / spriteWidth;
            float targetHeight = targetWidth * aspectRatio;
            
            curNameImage.GetComponent<RectTransform>().sizeDelta = new Vector2(targetWidth, targetHeight);
        }

        // 플레이버 텍스트 업데이트
        curFlav.text = LocaleDataManager.GetLocalizedStyleEffect(sty.description);

        // 첫 번째 강화 효과 (Plus)
        //curEff1.text = GetValueFullTextEff(sty, sty.plusEffectDescription, true);
        upgrEff1.text = GetValueFullTextUpgraded(sty, sty.plusEffectDescription, true);
        Eff1Ink.text = sty.plusTiers[sty.currentPlus - 1].cost.ToString();
        
        if (sty.currentPlus == sty.maxPlusLevel)
        {
            upgrSprite1.sprite = FullUpgradeButtomImage;
            upgrSprite1.GetComponent<Button>().interactable = false;
            upgrSprite1.GetComponent<UIButtonHoverScale>().targetScale = 1.0f;
            ClickToUp1.SetActive(false);
            FullToUp1.SetActive(true);
            Eff1InkImage.SetActive(false);
        }
        else
        {
            upgrSprite1.sprite = nonFullUpgradeButtonImage;
            upgrSprite1.GetComponent<Button>().interactable = true;
            upgrSprite1.GetComponent<UIButtonHoverScale>().targetScale = 1.03f;
            ClickToUp1.SetActive(true);
            FullToUp1.SetActive(false);
            Eff1InkImage.SetActive(true);
        }

        // 두 번째 강화 효과 (Minus)
        //curEff2.text = GetValueFullTextEff(sty, sty.minusEffectEffectDesc, false);
        upgrEff2.text = GetValueFullTextUpgraded(sty, sty.minusEffectEffectDesc, false);
        Eff2Ink.text = sty.minusTiers[sty.currentMinus - 1].cost.ToString();
        
        if (sty.currentMinus == sty.maxMinusLevel)
        {
            upgrSprite2.sprite = FullUpgradeButtomImage;
            upgrSprite2.GetComponent<Button>().interactable = false;
            upgrSprite2.GetComponent<UIButtonHoverScale>().targetScale = 1.0f;
            ClickToUp2.SetActive(false);
            FullToUp2.SetActive(true);
            Eff2InkImage.SetActive(false);
        }
        else
        {
            upgrSprite2.sprite = nonFullUpgradeButtonImage;
            upgrSprite2.GetComponent<Button>().interactable = true;
            upgrSprite2.GetComponent<UIButtonHoverScale>().targetScale = 1.03f;
            ClickToUp2.SetActive(true);
            FullToUp2.SetActive(false);
            Eff2InkImage.SetActive(true);
        }
    }
}
