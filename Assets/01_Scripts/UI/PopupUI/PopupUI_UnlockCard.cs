using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupUI_UnlockCard : BasePopupUI
{
    [SerializeField] private Image cardIllust;
    [SerializeField] private Image CardFrame;
    [SerializeField] private CardInHand cardinhand;


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

        // CardInHand를 사용해서 UI 요소 업데이트
        if (cardinhand != null)
        {
            // 카드 데이터 설정 (이미지, 이름, 비용, 설명)
            cardinhand.cardData = model;
            cardinhand.UpdateCardImage();
            cardinhand.UpdateCardInfoOnlyUI();
            
            // CardInHand 컴포넌트 제거 (이벤트 핸들러 비활성화)
            DestroyImmediate(cardinhand);
        }
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
