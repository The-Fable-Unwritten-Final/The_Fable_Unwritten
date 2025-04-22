using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEventEffects : EventEffects
{
    // 적용 대상
    public bool sophia;
    public bool kyla;
    public bool leon;

    // 효과
    public int newCardIndex; // 새로운 카드가 일시적으로 추가됨 등의 여부.
    public int cardType; // 효과 적용 대상 카드 타입
    public int cost; // 코스트 변화
    public bool unusable; // 사용 불가 판정

    public override void Apply()
    {

    }
    public override EventEffects Clone()
    {
        return Instantiate(this);
    }
}
