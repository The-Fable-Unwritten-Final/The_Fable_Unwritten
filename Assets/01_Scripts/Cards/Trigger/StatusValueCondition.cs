using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusValueCondition : TriggerCondition
{
    public BuffStatType statusType;
    public int requiredValue;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        foreach (var target in targets)
        {
            if (target.GetEffectValue(statusType) >= requiredValue)
                return true;
        }
        return false;
    }

    public override string Description => $"{statusType} {requiredValue} 이상";
}