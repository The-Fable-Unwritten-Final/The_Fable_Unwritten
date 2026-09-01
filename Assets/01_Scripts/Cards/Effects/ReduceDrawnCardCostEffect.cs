using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/ReduceDrawnCardCostEffect")]
public class ReduceDrawnCardCostEffect : CardEffectBase
{
    public int amount;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        var card = BattleLogManager.Instance.LastDrawnCard;
        if (card == null) return;

        card.ApplyTemporaryDiscount(amount);
    }

    public override string GetDescription()
    {
        return $"드로우한 카드의 비용을 {amount} 감소시킵니다.";
    }
}