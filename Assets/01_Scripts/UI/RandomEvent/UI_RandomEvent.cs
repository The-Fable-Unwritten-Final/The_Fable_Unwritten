using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_RandomEvent : MonoBehaviour
{
    [Header("ConnetObject")]
    [SerializeField] Image backGround;
    [SerializeField] Image illustration_Img;
    [SerializeField] TextMeshProUGUI titleTxt; 
    [SerializeField] TextMeshProUGUI descriptionTxt;
    [SerializeField] Button optionButton_a;
    [SerializeField] Button optionButton_b;
    [SerializeField] TextMeshProUGUI optionTxt_a;
    [SerializeField] TextMeshProUGUI optionTxt_b;

    [Header("Value")]
    [SerializeField] float typingSpeed;

    private RandomEventData currentData;
    private List<int> results;
    private bool isSelectOption = false;

    private void Start()
    {
        backGround.sprite = DataManager.Instance.GetBackground(ProgressDataManager.Instance.StageIndex);
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

        StartCoroutine(StartTyping());
    }


   private IEnumerator StartTyping()
    {
        yield return StartCoroutine(TypeText(descriptionTxt, LocaleDataManager.GetLocalizedRandomEvent(currentData.description)));
        yield return new WaitForSeconds(0.5f);
        Coroutine op_0 = StartCoroutine(TypeText(optionTxt_a, LocaleDataManager.GetLocalizedRandomEvent(currentData.option_a)));
        Coroutine op_1 = StartCoroutine(TypeText(optionTxt_b, LocaleDataManager.GetLocalizedRandomEvent(currentData.option_b)));

        yield return op_0;
        yield return op_1;

        optionButton_a.interactable = true;
        optionButton_b.interactable = true;
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


        yield return new WaitForSeconds(0.5f);

        float randomValue = Random.value; // 0 ~ 1 사이
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
            .Where(i => {
                var effect = EventEffectManager.Instance.eventEffectDict[i];
                if (effect != null && effect.eventType == 1)
                {
                    var cardEffect = effect as CardEventEffects;
                    if (cardEffect != null && cardEffect.newCardIndex != 0)
                        return false;
                }
                return true;
            })
            .Select(i => EventEffectManager.Instance.GetEventEffectText(i)));

        yield return StartCoroutine(TypeText(descriptionTxt, resultDescription));
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeText(optionTxt_b, resultText));

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
