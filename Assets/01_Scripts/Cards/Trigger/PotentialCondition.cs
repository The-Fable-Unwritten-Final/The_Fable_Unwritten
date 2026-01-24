using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Conditions/PotentialCondition")]
public class PotentialCondition : TriggerCondition
{
    public int requiredPercent;
    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        if(caster is PlayerController pc)
        {
            return pc.potentialGauge.FillPercent >= requiredPercent;
        }
        return false;
    }
    public override string Description => $"{requiredPercent}만큼 찼는가?";
}
