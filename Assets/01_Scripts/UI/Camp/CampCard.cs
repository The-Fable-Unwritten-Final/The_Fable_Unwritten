using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampCard : MonoBehaviour
{
    [SerializeField] Image cardIllust;
    [SerializeField] Image cardType;
    [SerializeField] Image cardChar;
    [SerializeField] TextMeshProUGUI cardCost;
    [SerializeField] TextMeshProUGUI cardNameText;
    [SerializeField] TextMeshProUGUI cardDesc;

    public void SetCard(CardModel card)
    {
        cardIllust.sprite = card.illustration;
        cardType.sprite = card.cardType;
        cardChar.sprite = card.chClass;

        cardDesc.text = card.cardText;
        cardNameText.text = card.cardName;
        cardCost.text = card.manaCost.ToString();
    }
}
