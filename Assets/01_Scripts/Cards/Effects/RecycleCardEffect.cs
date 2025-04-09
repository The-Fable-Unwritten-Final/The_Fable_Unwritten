using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cards/Effects/RecycleCardEffect")]
public class RecycleCardEffect : CardEffectBase
{

    public int amount = 1;    //되돌릴 카드 수

    /// <summary>
    /// 외부 선택 없이 최근 사용한 카드부터 maxCount개를 되돌림
    /// </summary>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        var deck = caster.Deck;
        if (deck == null) return;

        var usedCards = deck.GetUsedCards();
        int takeCount = Mathf.Min(amount, usedCards.Count);

        for (int i = 0; i < takeCount; i++)
        {
            var card = usedCards[usedCards.Count - 1 - i];
            deck.AddToHand(card);
            deck.RemoveFromUsed(card);
        }
    }

    /// <summary>
    /// 외부에서 특정 카드들을 선택했을 경우 사용하는 선택 기반 리사이클
    /// </summary>
    public void ApplyWithSelection(IStatusReceiver caster, List<int> selectedIndexes)
    {
        var deck = caster.Deck;
        if (deck == null) return;

        var usedCards = deck.GetUsedCards();

        // amount 장까지만 선택 허용
        int allowedCount = Mathf.Min(amount, selectedIndexes.Count);

        for (int k = 0; k < allowedCount; k++)
        {
            int i = selectedIndexes[k];
            if (i >= 0 && i < usedCards.Count)
            {
                var card = usedCards[i];
                deck.AddToHand(card);
                deck.RemoveFromUsed(card);
            }
        }
    }

    public override string GetDescription() => $"사용한 카드 중 {amount}장까지 선택하여 손패로 되돌립니다.";
}