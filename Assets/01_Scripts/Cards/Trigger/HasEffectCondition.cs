using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasEffectCondition : TriggerCondition
{
    public BuffStatType statusType;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        foreach (var target in targets)
        {
            if (target.HasEffect(statusType))
                return true;
        }
        return false;
    }

    public override string Description => "";
}
