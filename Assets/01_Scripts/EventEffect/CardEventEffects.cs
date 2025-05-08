using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardEventEffects : EventEffects
{
    // 적용 대상
    public bool sophia;
    public bool kyla;
    public bool leon;
    public bool unusable;

    // 효과
    public int newCardIndex; // 새로운 카드가 일시적으로 추가됨 등의 여부.
    public int cardType; // 효과 적용 대상 카드 타입
    public int cost; // 코스트 변화

    // 효과 적용 방식 추후 리펙토링 하기.., 일단은 최대한 넓은 확장성을 고려한 구조로 작성함.
    public override void Apply()
    {
        if(cost != 0)// 코스트 변화.
        {
            foreach(var chars in GameManager.Instance.turnController.battleFlow.playerParty)
            {
                switch(chars.ChClass)
                {
                    case CharacterClass.Sophia:
                        if(sophia)
                            chars.Deck.ApplyPersistentDiscountToAllCards(-cost);
                        break;
                    case CharacterClass.Kayla:
                        if(kyla)
                            chars.Deck.ApplyPersistentDiscountToAllCards(-cost);
                        break;
                    case CharacterClass.Leon:
                        if(leon)
                            chars.Deck.ApplyPersistentDiscountToAllCards(-cost);
                        break;
                }
            }
        }
        if(newCardIndex != 0)// 카드 추가.
        {
            if(sophia)
            {
                ProgressDataManager.Instance.PlayerDatas[1].currentDeckIndexes.Add(newCardIndex);
            }
            if(kyla)
            {
                ProgressDataManager.Instance.PlayerDatas[0].currentDeckIndexes.Add(newCardIndex);
            }
            if(leon)
            {
                ProgressDataManager.Instance.PlayerDatas[2].currentDeckIndexes.Add(newCardIndex);
            }
        }
    }
    public override void UnApply()
    {
        if(cost != 0)
        {
            foreach(var chars in GameManager.Instance.turnController.battleFlow.playerParty)
            {
                switch(chars.ChClass)
                {
                    case CharacterClass.Sophia:
                        if(sophia)
                            chars.Deck.ApplyPersistentDiscountToAllCards(cost);
                        break;
                    case CharacterClass.Kayla:
                        if(kyla)
                            chars.Deck.ApplyPersistentDiscountToAllCards(cost);
                        break;
                    case CharacterClass.Leon:
                        if(leon)
                            chars.Deck.ApplyPersistentDiscountToAllCards(cost);
                        break;
                }
            }
        }
    }
    public override EventEffects Clone()
    {
        return new CardEventEffects
        {
            index = this.index,
            text = this.text,
            eventType = this.eventType,
            duration = this.duration,
            sophia = this.sophia,
            kyla = this.kyla,
            leon = this.leon,
            unusable = this.unusable,
            newCardIndex = this.newCardIndex,
            cardType = this.cardType,
            cost = this.cost
        };
    }
}
