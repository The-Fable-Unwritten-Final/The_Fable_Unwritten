using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CardUnlockPopUp : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Image cardIllust;
    [SerializeField] private Image cardType;
    [SerializeField] private Image character;
    [SerializeField] private TextMeshProUGUI cardCost;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardDesc;

    [SerializeField] private TextMeshProUGUI cardNotes;
    [SerializeField] private Button confirmButton;

    

    private void Start()
    {
        confirmButton.onClick.AddListener(() => popupPanel.SetActive(false));
        popupPanel.SetActive(false);
    }

    private void OnEnable()
    {
        CardUnlocker.OnCardUnlocked += Show;
    }
    private void OnDisable()
    {
        CardUnlocker.OnCardUnlocked -= Show;
    }

    public void Show(CardModel model)
    {
        cardIllust.sprite = model.illustration;
        cardType.sprite = model.cardType;
        character.sprite = model.chClass;
        cardCost.text = model.manaCost.ToString();
        cardName.text = model.cardName;
        cardDesc.text = model.cardText;

        cardNotes.text = model.FlavorText;

        popupPanel.SetActive(true);
    }

}
