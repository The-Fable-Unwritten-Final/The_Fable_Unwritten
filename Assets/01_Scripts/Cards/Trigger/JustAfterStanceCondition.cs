using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/Conditions/JustAfterStanceCondition")]
public class JustAfterStanceCondition : TriggerCondition
{
    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (caster is PlayerController pc)
        {
            return pc.justTriggeredStanceThisTurn;
        }

        return false;
    }

    public override string Description => "스탠스 발동 직후일 때";
}