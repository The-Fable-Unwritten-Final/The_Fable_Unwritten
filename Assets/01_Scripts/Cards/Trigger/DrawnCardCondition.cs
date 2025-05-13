using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawnCardCondition : TriggerCondition
{
    public List<CardType> requiredTypes;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        var lastDrown = BattleLogManager.Instance?.LastDrawnCard;
        if (lastDrown == null) return false;

        return requiredTypes.Contains(lastDrown.type);
    }

    public override string Description =>
            $"마지막으로 드로우한 카드가 다음 타입 중 하나일 경우: {string.Join(", ", requiredTypes)}";
}
