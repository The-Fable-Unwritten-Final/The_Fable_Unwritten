using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Conditions/SinTotalCondition")]
public class SinTotalCondition : TriggerCondition
{
    public int requiredValue;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (caster is not PlayerController pc)
            return false;

        return pc.GetEffectValue(BuffStatType.Sin) >= requiredValue;
    }

    public override string Description => $"죄악이 {requiredValue} 이상일 때";
}