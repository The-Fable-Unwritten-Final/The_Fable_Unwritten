using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookCards : MonoBehaviour
{
    [SerializeField] Sprite emptySprite;
    [SerializeField] Image cardIllust;
    [SerializeField] Image cardType;
    [SerializeField] Image cardChar;
    [SerializeField] TextMeshProUGUI cardCost;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDesc;
    public string flavorText; 

    public bool isEmpty = true; // 카드가 비어있는지 체크하는 변수

    public void SetCardInfo(CardModel c)
    {
        if (c == null)
        {
            cardIllust.sprite = emptySprite;
            cardType.sprite = emptySprite;
            cardChar.sprite = emptySprite;

            cardCost.text = string.Empty;
            cardNameText.text = string.Empty;
            cardDesc.text = string.Empty;
            flavorText = string.Empty;
            isEmpty = true; // 카드가 비어있음
            return;
        }
        cardIllust.sprite = c.illustration;
        cardType.sprite = c.cardType;
        cardChar.sprite = c.chClass;

        cardCost.text = c.manaCost.ToString();
        cardNameText.text = c.cardName;
        cardDesc.text = c.cardText;
        flavorText = c.FlavorText;
        isEmpty = false;
    }

    public void GiveCardInfo(BookCards card) // card 로 들어오는 참조값에 현재의 값 넣어주기.
    {
        card.cardIllust.sprite = cardIllust.sprite;
        card.cardType.sprite = cardType.sprite;
        card.cardChar.sprite = cardChar.sprite;
        card.cardCost.text = cardCost.text;
        card.cardNameText.text = cardNameText.text;
        card.cardDesc.text = cardDesc.text;
        card.flavorText = flavorText;
    }
}
