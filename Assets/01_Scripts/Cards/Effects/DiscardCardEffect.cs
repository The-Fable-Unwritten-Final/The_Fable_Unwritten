using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//카드 버리기 랜덤 아님. 턴 종료시 버리기와 함께 수정 필요
/// <summary>
/// 카드를 버리는 효과 적용 시
/// </summary>
[CreateAssetMenu(menuName = "Card/Effect/Discard")]
public class DiscardCardEffect : CardEffectBase
{
    public int discardCount = 1;
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        var deck = caster.Deck;
        int count = Mathf.Min(discardCount, deck.Hand.Count);
        for (int i = 0; i < count; i++)
        {
            var randomIndex = Random.Range(0, deck.Hand.Count);
            deck.Discard(deck.Hand[randomIndex]);
        }
    }

    public override string GetDescription() => $"카드를 {discardCount}만큼 버림";

}
