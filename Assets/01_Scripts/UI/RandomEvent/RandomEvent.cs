using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class RandomEvent : MonoBehaviour
{
    [Header("ConnetObject")]
    [SerializeField] Image illustration_Img;
    [SerializeField] TextMeshProUGUI titleTxt; 
    [SerializeField] TextMeshProUGUI descriptionTxt;
    [SerializeField] Button optionButton_0;
    [SerializeField] Button optionButton_1;
    [SerializeField] TextMeshProUGUI optionTxt_0;
    [SerializeField] TextMeshProUGUI optionTxt_1;

    [Header("Value")]
    [SerializeField] float typingSpeed;

    private string descriptionFullTxt_0 = "숲을 지나는 도중, 발 밑에서 누군가가 말을 걸어온다.\n\"너희들은... 누구냐?\"\n파이프를 물고 있는 애벌레가 대답을 기다리는 것 같다.";
    private string optionFullTxt_0 = "[소피아] 호기심 많은 탐험가지";
    private string optionFullTxt_1 = "[레온] 그냥 길을 일었을 뿐이잖아요";

    private void Start()
    {
        optionButton_0.interactable = false;
        optionButton_1.interactable = false;

        StartCoroutine(StartTyping());
    }

   private IEnumerator StartTyping()
    {
        yield return StartCoroutine(TypeText(descriptionTxt, descriptionFullTxt_0));
        yield return new WaitForSeconds(0.5f);
        Coroutine op_0 = StartCoroutine(TypeText(optionTxt_0, optionFullTxt_0));
        Coroutine op_1 = StartCoroutine(TypeText(optionTxt_1, optionFullTxt_1));

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

    public void SelectOption()
    {
        ClearPanel();
    }

    private void ClearPanel()
    {
        descriptionTxt.text = "";
        optionTxt_0.text = "";
        optionTxt_1.text = "";

        optionButton_0.interactable = false;
        optionButton_1.interactable = false;

    }
}
