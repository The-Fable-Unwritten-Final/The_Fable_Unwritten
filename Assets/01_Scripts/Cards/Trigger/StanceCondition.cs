using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StancValue;

public class StanceCondition : TriggerCondition
{
    public EStancType requiredStance;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        return caster.CurrentStance.Equals(requiredStance.ToString());
    }

    public override string Description => $"[{requiredStance}] 자세일 경우";
}
