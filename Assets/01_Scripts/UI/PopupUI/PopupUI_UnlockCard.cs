using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupUI_UnlockCard : BasePopupUI
{
    [SerializeField] private Image cardIllust;
    [SerializeField] private Image cardType;
    [SerializeField] private Image character;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardDesc;

    public override void Open()
    {
        base.Open();
        Show();
    }
    public void Show()
    {
        CardModel model = EventEffectManager.Instance.cardData;
        if (model == null)
        {
            Debug.LogError("[PopupUI_UnlockCard] 카드 데이터가 없습니다.");
            return;
        }

        cardIllust.sprite = model.illustration;
        cardType.sprite = model.cardType;
        character.sprite = model.chClass;
        cardName.text = model.cardName;
        cardDesc.text = model.cardText;

    }
    
    public void OnConfirmUnlock()
    {
        CardModel cardData = EventEffectManager.Instance.cardData;
        if (cardData != null)
        {
            ProgressDataManager.Instance.unlockedCards.Add(cardData.index);
            cardData.isUnlocked = true;  // CardModel의 isUnlocked도 즉시 업데이트
            ProgressDataManager.Instance.SaveProgress(true);
        }

        // 팝업 닫기
        base.Close();
    }
}
