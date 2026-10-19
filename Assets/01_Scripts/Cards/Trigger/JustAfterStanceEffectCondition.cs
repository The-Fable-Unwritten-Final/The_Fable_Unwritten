using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Conditions/JustAfterStanceEffectCondition")]
public class JustAfterStanceEffectCondition : TriggerCondition
{
    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (caster is PlayerController pc)
        {
            return pc.justTriggeredStanceThisTurn;
        }

        return false;
    }

    public override string Description => "스탠스 효과 직후일 때";
}