using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_RandomEvent : MonoBehaviour
{
    [Header("ConnetObject")]
    //[SerializeField] Image backGround;
    [SerializeField] Image illustration_Img;
    [SerializeField] TextMeshProUGUI titleTxt; 
    [SerializeField] TextMeshProUGUI descriptionTxt;
    [SerializeField] TextMeshProUGUI pageIndicatorText;
    [SerializeField] Button optionButton_a;
    [SerializeField] Button optionButton_b;
    [SerializeField] TextMeshProUGUI optionTxt_a;
    [SerializeField] TextMeshProUGUI optionTxt_b;

    [Header("Value")]
    [SerializeField] float typingSpeed;
    [SerializeField] float minFontSize = 20f;
    [SerializeField] float maxFontSize = 30f;
    [SerializeField] int maxLinesPerPage = 15;
    [SerializeField] int lineThresholdForPagination = 18;

    private RandomEventData currentData;
    private List<int> results;
    private bool isSelectOption = false;

    // 페이지 관리
    private List<string> descriptionPages = new List<string>();
    private int currentDescriptionPage = 0;
    private TextMeshProUGUI nextPageIndicatorText;
    private bool isLoadingPage = false;
    private Coroutine pageIndicatorAnimationCoroutine;    private string currentResultText = ""; // 결과 페이지의 결과 텍스트 저장
    private bool isResultPage = false; // 결과 페이지 여부
    private void Start()
    {
        //backGround.sprite = DataManager.Instance.GetBackground(ProgressDataManager.Instance.StageIndex);
        ProgressDataManager.Instance.IsNewStage = false;

        if (ProgressDataManager.Instance.SavedRandomEvent <= 0) // 저장 된 현재 랜덤이밴트 없을 경우
        {
            GetCurrentEvent();
        }
        else
        {
            GetSavedEvent();
        }

        ProgressDataManager.Instance.SaveProgress(true);

        if (currentData != null)
        {
            InitUI(currentData);
        }

        SoundManager.Instance.PlayBGM(SoundCategory.RandomEventBGM, currentData.index); // 랜덤 이벤트 BGM 재생
    }
    private void GetSavedEvent()
    {
        int savedIndex = ProgressDataManager.Instance.SavedRandomEvent;

        var savedEventData = DataManager.Instance.allRandomEvents.FirstOrDefault(e => e.index == savedIndex);

        currentData = savedEventData;
    }

    private void GetCurrentEvent()
    {
        var pdm = ProgressDataManager.Instance; 
        int currentStageIndex = pdm.StageIndex;
        currentData = pdm.GetRandomEvent(pdm.CurrentTheme);

        ProgressDataManager.Instance.SavedRandomEvent = currentData.index;

        if (currentData == null) return;        
    }

    /// <summary>
    /// 텍스트를 페이지 단위로 분할 (18줄 이상이면 15줄+a로 자르기)
    /// 실제 렌더링되는 줄 수를 기준으로 분할
    /// </summary>
    private List<string> SplitTextIntoPages(string fullText)
    {
        List<string> pages = new List<string>();

        // 전체 텍스트를 임시로 설정해서 실제 렌더링 줄 수 확인
        descriptionTxt.text = fullText;
        Canvas.ForceUpdateCanvases();
        int totalVisualLines = descriptionTxt.textInfo.lineCount;

        // 18줄 이하이면 그대로 반환
        if (totalVisualLines <= lineThresholdForPagination)
        {
            pages.Add(fullText);
            return pages;
        }

        // 줄 단위로 텍스트 분할 (\n 기준)
        string[] lines = fullText.Split('\n');
        int currentLineCount = 0;
        string currentPage = "";

        foreach (var line in lines)
        {
            currentPage += line + "\n";
            currentLineCount++;

            // maxLinesPerPage(15줄)에 도달했는지 확인
            descriptionTxt.text = currentPage;
            Canvas.ForceUpdateCanvases();
            int currentVisualLines = descriptionTxt.textInfo.lineCount;

            if (currentVisualLines >= maxLinesPerPage)
            {
                pages.Add(currentPage.TrimEnd());
                currentPage = "";
            }
        }

        // 마지막 페이지 추가
        if (!string.IsNullOrEmpty(currentPage))
        {
            pages.Add(currentPage.TrimEnd());
        }

        return pages;
    }

    /// <summary>
    /// 텍스트 양에 따라 최적의 폰트 크기 계산 (적용 중 X)
    /// </summary>
    private float CalculateOptimalFontSize(string text) // 현재 컴포넌트의 autoSize 사용중이라 미적용
    {
        if (string.IsNullOrEmpty(text))
            return maxFontSize;

        // 줄 수 계산
        int lineCount = descriptionTxt.textInfo.lineCount;
        // 문자 수 계산
        int charCount = text.Length;

        // 줄 수에 따른 폰트 크기 계산 (선형 감소)
        float sizeByLineCount = maxFontSize - (lineCount - 1) * 1.5f;

        // 문자 수에 따른 폰트 크기 계산
        float sizeByCharCount = maxFontSize - (charCount / 100f) * 2f;

        // 더 작은 값을 선택
        float calculatedSize = Mathf.Min(sizeByLineCount, sizeByCharCount);

        int extraSize = 0;
        if (descriptionPages.Count >= 2) extraSize = 3; // 임시 크기 보정

        // 최소/최대 범위 내로 제한
        return Mathf.Clamp(calculatedSize, minFontSize, maxFontSize) + extraSize;
    }

    private void InitUI(RandomEventData data)
    {
        illustration_Img.sprite = data.illustrationSprite;
        // 연계 이벤트 플레그 확인
        if (ProgressDataManager.Instance.IsChainEventTriggered(data.index))
        {
            // 인과 이벤트가 활성화 된 상태면, 2번째 확률 선택지의 결과로 이어지게 설정 
            data.percentage_a = 0;
            data.percentage_b = 0;
        }
        currentData = data;

        titleTxt.text = LocaleDataManager.GetLocalizedRandomEvent(data.title);

        optionButton_a.interactable = false;
        optionButton_b.interactable = false;

        optionTxt_a.text = "";
        optionTxt_b.text = "";

        // 페이지 상태 초기화
        currentDescriptionPage = 0;
        descriptionPages.Clear();
        isLoadingPage = false;
        isResultPage = false;
        currentResultText = "";

        // 페이지 표시기 텍스트 초기화
        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = "";
        }

        // 기존 애니메이션 중지
        if (pageIndicatorAnimationCoroutine != null)
        {
            StopCoroutine(pageIndicatorAnimationCoroutine);
            pageIndicatorAnimationCoroutine = null;
        }

        // descriptionTxt의 버튼 컴포넌트 설정
        Button descriptionButton = descriptionTxt.GetComponent<Button>();
        if (descriptionButton != null)
        {
            descriptionButton.onClick.RemoveAllListeners();
            descriptionButton.onClick.AddListener(OnDescriptionButtonClicked);
        }

        StartCoroutine(StartTyping());
    }


   private IEnumerator StartTyping()
    {
        var customEffect = descriptionTxt.GetComponent<TMPCustomEffect>();
        if (customEffect != null)
        {
            customEffect.Reset();
        }
        
        string fullText = LocaleDataManager.GetLocalizedRandomEvent(currentData.description);
        
        // 텍스트를 페이지로 분할
        descriptionPages = SplitTextIntoPages(fullText);
        currentDescriptionPage = 0;

        // 첫 번째 페이지 표시
        yield return StartCoroutine(DisplayDescriptionPage(0));

        yield return new WaitForSeconds(0.5f);
        Coroutine op_0 = StartCoroutine(TypeText(optionTxt_a, LocaleDataManager.GetLocalizedRandomEvent(currentData.option_a)));
        Coroutine op_1 = StartCoroutine(TypeText(optionTxt_b, LocaleDataManager.GetLocalizedRandomEvent(currentData.option_b)));

        yield return op_0;
        yield return op_1;

        optionButton_a.interactable = true;
        optionButton_b.interactable = true;
    }

    /// <summary>
    /// 지정된 페이지의 설명을 표시
    /// </summary>
    private IEnumerator DisplayDescriptionPage(int pageIndex)
    {
        isLoadingPage = true;

        if (pageIndex < 0 || pageIndex >= descriptionPages.Count)
        {
            isLoadingPage = false;
            yield break;
        }

        currentDescriptionPage = pageIndex;
        string pageText = descriptionPages[pageIndex];

        // 최적 폰트 크기 계산 및 설정 (현재 컴포넌트의 autoSize 사용중이라 미적용)
        float optimalFontSize = CalculateOptimalFontSize(pageText);
        descriptionTxt.fontSize = optimalFontSize;

        var customEffect = descriptionTxt.GetComponent<TMPCustomEffect>();
        if (customEffect != null)
        {
            customEffect.Reset();
        }

        // 타이핑 이펙트와 함께 텍스트 표시
        yield return StartCoroutine(TypeTextWithEffect(descriptionTxt, pageText));

        // 다음 페이지가 있으면 페이지 표시기 활성화
        if (pageIndex < descriptionPages.Count - 1)
        {
            ShowPageIndicator();
        }
        else
        {
            HidePageIndicator();
        }

        isLoadingPage = false;
    }

    /// <summary>
    /// 페이지 표시기 활성화 (강조 애니메이션)
    /// </summary>
    private void ShowPageIndicator()
    {
        if (pageIndicatorText == null)
            return;

        // 기존 애니메이션 중지
        if (pageIndicatorAnimationCoroutine != null)
        {
            StopCoroutine(pageIndicatorAnimationCoroutine);
        }

        pageIndicatorText.text = "N e x t  >>";
        pageIndicatorAnimationCoroutine = StartCoroutine(PageIndicatorPulseAnimation());
    }

    /// <summary>
    /// 페이지 표시기 비활성화
    /// </summary>
    private void HidePageIndicator()
    {
        if (pageIndicatorText == null)
            return;

        // 기존 애니메이션 중지
        if (pageIndicatorAnimationCoroutine != null)
        {
            StopCoroutine(pageIndicatorAnimationCoroutine);
            pageIndicatorAnimationCoroutine = null;
        }

        pageIndicatorText.text = "";
    }

    /// <summary>
    /// 페이지 표시기 스케일 애니메이션 (1.0 ~ 1.1배)
    /// </summary>
    private IEnumerator PageIndicatorPulseAnimation()
    {
        if (pageIndicatorText == null)
            yield break;

        float duration = 0.5f; // 한 사이클에 0.5초 (커졌다 작아졌다)
        float elapsedTime = 0f;

        while (true)
        {
            elapsedTime = (elapsedTime + Time.deltaTime) % (duration * 2);

            float t;
            if (elapsedTime < duration)
            {
                // 1.0 → 1.1 (커지는 구간)
                t = elapsedTime / duration;
                t = Mathf.Lerp(1f, 1.1f, Mathf.Sin(t * Mathf.PI / 2)); // easeOut
            }
            else
            {
                // 1.1 → 1.0 (작아지는 구간)
                t = (elapsedTime - duration) / duration;
                t = Mathf.Lerp(1.1f, 1f, Mathf.Sin(t * Mathf.PI / 2)); // easeIn
            }

            pageIndicatorText.transform.localScale = new Vector3(t, t, 1f);

            yield return null;
        }
    }

    /// <summary>
    /// descriptionTxt 클릭 이벤트 (다음 페이지로 이동)
    /// </summary>
    private void OnDescriptionButtonClicked()
    {
        if (isLoadingPage)
            return;

        if (currentDescriptionPage < descriptionPages.Count - 1)
        {
            if (isResultPage)
            {
                // 결과 페이지
                StartCoroutine(DisplayDescriptionPageForResult(currentDescriptionPage + 1, currentResultText));
            }
            else
            {
                // 초기 페이지
                StartCoroutine(DisplayDescriptionPage(currentDescriptionPage + 1));
            }
        }
    }

    /// <summary>
    /// 결과 페이지 표시 (resultText 포함)
    /// </summary>
    private IEnumerator DisplayDescriptionPageForResult(int pageIndex, string resultText)
    {
        isLoadingPage = true;

        if (pageIndex < 0 || pageIndex >= descriptionPages.Count)
        {
            isLoadingPage = false;
            yield break;
        }

        currentDescriptionPage = pageIndex;
        string pageText = descriptionPages[pageIndex];

        // 최적 폰트 크기 계산 및 설정 (현재 컴포넌트의 autoSize 사용중이라 미적용)
        float optimalFontSize = CalculateOptimalFontSize(pageText);
        descriptionTxt.fontSize = optimalFontSize;

        var customEffect = descriptionTxt.GetComponent<TMPCustomEffect>();
        if (customEffect != null)
        {
            customEffect.Reset();
        }

        // 타이핑 이펙트와 함께 텍스트 표시
        yield return StartCoroutine(TypeTextWithEffect(descriptionTxt, pageText));
        yield return new WaitForSeconds(0.5f);

        // 마지막 페이지에서만 결과 텍스트 표시
        if (pageIndex == descriptionPages.Count - 1)
        {
            yield return StartCoroutine(TypeText(optionTxt_b, resultText));
        }

        // 다음 페이지가 있으면 페이지 표시기 활성화
        if (pageIndex < descriptionPages.Count - 1)
        {
            ShowPageIndicator();
        }
        else
        {
            HidePageIndicator();
        }

        isLoadingPage = false;
    }


    private IEnumerator TypeText(TextMeshProUGUI textUI, string fullText)
    {
        textUI.text = "";
        foreach (var c in fullText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator TypeTextWithEffect(TextMeshProUGUI textUI, string rawText)
    {
        var customEffect = textUI.GetComponent<TMPCustomEffect>();
        string parsedText = GradientTextParser.Parse(rawText, out var _);
        
        textUI.text = "";
        int charCount = parsedText.Length;
        
        for (int i = 0; i < charCount; i++)
        {
            textUI.text += parsedText[i];
            
            // 매 글자마다 부분적으로 이펙트 적용 (타이핑 진행 상황 유지)
            if (customEffect != null)
            {
                customEffect.SetGradientTextPartial(rawText);
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void SelectOption(int index)
    {
        if (isSelectOption == true) return;

        // 반복 이벤트가 아닐 경우 연속 선택지 종료
        if (currentData.repeatIndex == 0)
        {
            optionButton_a.gameObject.SetActive(false);
            optionButton_b.interactable = false;
        }

        StartCoroutine(ProcessResult(index));

        // 애널리틱스
        GameManager.Instance.analyticsLogger.LogRandomEventInfo(currentData.index, index);
    }

    private IEnumerator ProcessResult(int optionIndex)
    {
        descriptionTxt.text = "";
        optionTxt_a.text = "";
        optionTxt_b.text = "";

        optionButton_a.interactable = false;
        optionButton_b.interactable = false;

        // 페이지 상태 초기화
        currentDescriptionPage = 0;
        descriptionPages.Clear();
        isLoadingPage = false;
        isResultPage = true;

        // 페이지 표시기 텍스트 초기화
        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = "";
        }

        // 기존 애니메이션 중지
        if (pageIndicatorAnimationCoroutine != null)
        {
            StopCoroutine(pageIndicatorAnimationCoroutine);
            pageIndicatorAnimationCoroutine = null;
        }

        yield return new WaitForSeconds(0.5f);

        float randomValue = UnityEngine.Random.value; // 0 ~ 1 사이
        string resultDescription = "";
        results = null;

        if (optionIndex == 0)
        {
            if (randomValue < currentData.percentage_a)
            {
                resultDescription = LocaleDataManager.GetLocalizedRandomEvent(currentData.description_a1);
                results = currentData.parsed_result_a1;
            }
            else
            {
                resultDescription = LocaleDataManager.GetLocalizedRandomEvent(currentData.description_a2);
                results = currentData.parsed_result_a2;
            }
        }
        else if (optionIndex == 1)
        {
            if (randomValue < currentData.percentage_b)
            {
                resultDescription = LocaleDataManager.GetLocalizedRandomEvent(currentData.description_b1);
                results = currentData.parsed_result_b1;
            }
            else
            {
                resultDescription = LocaleDataManager.GetLocalizedRandomEvent(currentData.description_b2);
                results = currentData.parsed_result_b2;
            }
        }

        // 반복 이벤트 처리 (결과값에 100000가 있을 경우)
        if (results[0] == 100000)
        {
            int repeatIndex = currentData.repeatIndex; // 반복 이벤트 인덱스로 데이터 교체
            var repeatEventData = DataManager.Instance.allRandomEvents.FirstOrDefault(e => e.index == repeatIndex);
            if (repeatEventData != null)
            {
                ProgressDataManager.Instance.SavedRandomEvent = repeatEventData.index;
                foreach (int resultIndex in results)
                {
                    if (resultIndex == 100000) continue;
                    EventEffectManager.Instance.AddEventEffect(resultIndex);// 반복 이벤트 발생 효과 추가 or 적용
                }
                InitUI(repeatEventData);
                yield break;
            }
            else
            {
                Debug.LogError($"반복 이벤트 인덱스 {repeatIndex}에 해당하는 이벤트를 찾을 수 없습니다.");
                yield break;
            }
        }
        else
        {
            optionButton_a.gameObject.SetActive(false);
            optionButton_b.interactable = false;
        }

        // 적용되는 효과를 텍스트로 나열 및 출력 해주는 효과 (이때 카드 해금은 텍스트 출력하지 않음 => 별도의 팝업 UI로 처리)
        string resultText = string.Join("\n", results
            .Where(i =>
            {
                var effect = EventEffectManager.Instance.eventEffectDict[i];
                if (effect != null && effect.eventType == 1)
                {
                    var cardEffect = effect as CardEventEffects;
                    if (cardEffect != null && cardEffect.newCardIndex != 0)
                        return false;
                }
                return true;
            })
            .Select(i => LocaleDataManager.GetLocalizedRandomEventEffect("Event_" + i)));

        // 결과 텍스트 저장
        currentResultText = resultText;

        // 결과 설명을 페이지로 분할
        descriptionPages = SplitTextIntoPages(resultDescription);
        currentDescriptionPage = 0;

        var customEffect = descriptionTxt.GetComponent<TMPCustomEffect>();
        if (customEffect != null)
        {
            customEffect.Reset();
        }
        
        // 첫 번째 페이지 표시 (결과 설명)
        yield return StartCoroutine(DisplayDescriptionPageForResult(0, resultText));

        // 만약 카드 해금이 포함되어 있으면, 해금 팝업 UI 출력
        foreach (int resultIndex in results)
        {
            var effect = EventEffectManager.Instance.eventEffectDict[resultIndex];
            if (effect != null && effect.eventType == 1)
            {
                var cardEffect = effect as CardEventEffects;
                if (cardEffect != null && cardEffect.newCardIndex != 0)
                {
                    // 팝업 UI 출력
                    var allCards = DataManager.Instance.AllCards;
                    var card = allCards.FirstOrDefault(c => c.index == cardEffect.newCardIndex);
                    EventEffectManager.Instance.cardData = card;
                    UIManager.Instance.ShowPopupByName("PopupUI_UnlockCard");
                }
            }
        }

        optionButton_b.onClick.RemoveAllListeners();
        // 최종적으로 등장하는 ~~ 효과 적용 버튼을 클릭해야지 다음 스테이지 이동 + 효과 적용이 실행된다
        optionButton_b.onClick.AddListener(ApplyEffectsAndGoToStage);
        optionButton_b.interactable = true;
        isSelectOption = true;
    }

    /// <summary>
    /// 랜덤이벤트로 얻게된 효과 적용
    /// 
    private void ApplyEffectsAndGoToStage()
    {
        // 이전 스테이지 상태를 복원하도록 설정
        ProgressDataManager.Instance.RetryFromStart = false;
        
        ProgressDataManager.Instance.SavedRandomEvent = -1;
        foreach (int resultIndex in results)
        {
            // 인과 이벤트 플레그 설정 (결과값에 200000 이상의 값이 존재하는 경우)
            if (resultIndex >= 200000)
            {
                ProgressDataManager.Instance.SetChainEventTriggered(resultIndex - 200000);
                continue;
            }


            // 효과 적용
            EventEffectManager.Instance.AddEventEffect(resultIndex);
        }
        
        if (!results.Contains(14)) // 14는 전투 입장
        {
            UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
        }
    }
}