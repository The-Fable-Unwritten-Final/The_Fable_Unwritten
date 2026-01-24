// AfterStanceEffectCondition.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Conditions/AfterStanceCondition")]
public class AfterStanceCondition : TriggerCondition
{
    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        if (caster is PlayerController pc)
        {
            return pc.stanceEffectData.JustTriggeredStance;
        }
        return false;
    }

    public override string Description => "";
}
