using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StancValue;
[CreateAssetMenu(menuName = "Cards/Conditions/StanceCondition")]

public class StanceCondition : TriggerCondition
{
    public StancType requiredStance;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        return caster.CurrentStance.Equals(requiredStance.ToString());
    }

    public override string Description => $"[{requiredStance}] 자세일 경우";
}
