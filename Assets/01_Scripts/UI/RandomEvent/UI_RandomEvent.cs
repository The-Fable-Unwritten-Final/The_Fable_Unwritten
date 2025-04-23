using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;

public class UI_RandomEvent : MonoBehaviour
{
    [Header("ConnetObject")]
    [SerializeField] Image backGround;
    [SerializeField] Image illustration_Img;
    [SerializeField] TextMeshProUGUI titleTxt; 
    [SerializeField] TextMeshProUGUI descriptionTxt;
    [SerializeField] Button optionButton_0;
    [SerializeField] Button optionButton_1;
    [SerializeField] TextMeshProUGUI optionTxt_0;
    [SerializeField] TextMeshProUGUI optionTxt_1;

    [Header("Value")]
    [SerializeField] float typingSpeed;

    private RandomEventData currentData;

    private void Start()
    {
        GetCurrentEvent();

        if (currentData != null)
        {
            InitUI(currentData);
        }
        else
        {
            Debug.LogWarning("랜덤 이벤트 데이터가 없습니다.");
        }
    }

    private void GetCurrentEvent()
    {
        int currentStageIndex = GameManager.Instance.StageSetting.StageIndex;

        currentData = GameManager.Instance.GetRandomEvent();


        if (currentData == null)
        {
            Debug.LogWarning($"스테이지 {currentStageIndex}에 사용 가능한 이벤트가 없습니다.");
            return;
        }

        backGround.sprite = GameManager.Instance.GetBackgroundForStage(currentStageIndex);
    }

    private void InitUI(RandomEventData data)
    {
        illustration_Img.sprite = currentData.illustrationSprite;
        currentData = data;

        titleTxt.text = data.title;

        optionButton_0.interactable = false;
        optionButton_1.interactable = false;

        optionTxt_0.text = "";
        optionTxt_1.text = "";

        StartCoroutine(StartTyping());
    }


   private IEnumerator StartTyping()
    {
        yield return StartCoroutine(TypeText(descriptionTxt, currentData.description));
        yield return new WaitForSeconds(0.5f);
        Coroutine op_0 = StartCoroutine(TypeText(optionTxt_0, currentData.option_0));
        Coroutine op_1 = StartCoroutine(TypeText(optionTxt_1, currentData.option_1));

        yield return op_0;
        yield return op_1;

        optionButton_0.interactable = true;
        optionButton_1.interactable = true;
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
        optionButton_0.gameObject.SetActive(false);
        optionButton_1.interactable = false;

        StartCoroutine(ProessResult(index));
    }

    private IEnumerator ProessResult(int optionIndex)
    {
        descriptionTxt.text = "";
        optionTxt_0.text = "";
        optionTxt_1.text = "";


        yield return new WaitForSeconds(0.5f);

        float randomValue = Random.value; // 0 ~ 1 사이
        string resultDescription = "";
        int resultIndex = -1;

        if (optionIndex == 0)
        {
            if (randomValue < currentData.percentage_0)
            {
                resultDescription = currentData.description_01;
                resultIndex = currentData.result_01;
            }
            else
            {
                resultDescription = currentData.description_02;
                resultIndex = currentData.result_02;
            }
        }
        else if (optionIndex == 1)
        {
            if (randomValue < currentData.percentage_1)
            {
                resultDescription = currentData.description_11;
                resultIndex = currentData.result_11;
            }
            else
            {
                resultDescription = currentData.description_12;
                resultIndex = currentData.result_12;
            }
        }

        EventEffectManager.Instance.AddEventEffect(resultIndex);

        string resultText = EventEffectManager.Instance.GetEventEffectText(resultIndex);

        yield return StartCoroutine(TypeText(descriptionTxt, resultDescription));
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeText(optionTxt_1, resultText));

        optionButton_1.onClick.RemoveAllListeners();
        optionButton_1.onClick.AddListener(GoToStage);
        optionButton_1.interactable = true;
    }

    private void GoToStage()
    {
        SceneManager.LoadScene(SceneNameData.StageScene);
    }
}
