using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Card/Effect/Recycle")]
public class RecycleCardEffect : CardEffectBase
{
    public int selectIndex = 0; // 선택할 카드 인덱스

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        var deck = caster.Deck;
        if (deck != null && deck.UsedCount() > 0)
        {
            var usedCards = deck.GetUsedCards();
            if (selectIndex >= 0 && selectIndex < usedCards.Count)
            {
                var selected = usedCards[selectIndex];
                deck.AddToHand(selected);
                deck.RemoveFromUsed(selected);
            }
        }
    }

    public override string GetDescription() => "사용한 카드 중 하나를 다시 손으로 가져옵니다.";
}