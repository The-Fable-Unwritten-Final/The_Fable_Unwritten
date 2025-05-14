using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UsedCardCondition : TriggerCondition
{
    public List<int> cardIndices = new();
    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        var used = BattleLogManager.Instance.UsedCardsForCurrent
           .Select(log => log.cardID)
           .ToHashSet();

        return cardIndices.Any(id => used.Contains(id));
    }

    public override string Description => $"이번 턴에 사용된 카드 인덱스: {string.Join(", ", cardIndices)} 중 하나";
}
