using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
    private List<int> results;

    private void Start()
    {
        backGround.sprite = DataManager.Instance.GetBackground(ProgressDataManager.Instance.StageIndex);

        if(ProgressDataManager.Instance.SavedRandomEvent <= 0) // 저장 된 현재 랜덤이밴트 없을 경우
        {
            GetCurrentEvent();
        }
        else
        {
            GetSavedEvent();
        }

        ProgressDataManager.Instance.SaveProgress();

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
        results = null;

        if (optionIndex == 0)
        {
            if (randomValue < currentData.percentage_0)
            {
                resultDescription = currentData.description_01;
                results = currentData.parsed_result_01;
            }
            else
            {
                resultDescription = currentData.description_02;
                results = currentData.parsed_result_02;
            }
        }
        else if (optionIndex == 1)
        {
            if (randomValue < currentData.percentage_1)
            {
                resultDescription = currentData.description_11;
                results = currentData.parsed_result_11;
            }
            else
            {
                resultDescription = currentData.description_12;
                results = currentData.parsed_result_12;
            }
        }

        string resultText = string.Join("\n", results.Select(i => EventEffectManager.Instance.GetEventEffectText(i)));

        yield return StartCoroutine(TypeText(descriptionTxt, resultDescription));
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeText(optionTxt_1, resultText));

        optionButton_1.onClick.RemoveAllListeners();
        optionButton_1.onClick.AddListener(ApplyEffectsAndGoToStage);
        optionButton_1.interactable = true;
    }

    private void ApplyEffectsAndGoToStage()
    {
        ProgressDataManager.Instance.SavedRandomEvent = -1;

        foreach (int resultIndex in results)
        {
            EventEffectManager.Instance.AddEventEffect(resultIndex);

            if (resultIndex != 14)
            {
                SceneManager.LoadScene(SceneNameData.StageScene);
            }
        }  
    }
}
