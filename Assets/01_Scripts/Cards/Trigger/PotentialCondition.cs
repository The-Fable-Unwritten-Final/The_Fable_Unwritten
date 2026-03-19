using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/Conditions/PotentialCondition")]
public class PotentialCondition : TriggerCondition
{
    public int potentialGauge;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (caster == null)
            return false;

        if (!caster.IsPlayer())
            return false;

        PlayerController player = caster.AsPlayer();
        if (player == null)
            return false;

        if (player.StanceSystem == null)
            return false;

        return player.StanceSystem.Gauge.CurrentGauge >= potentialGauge;
    }

    public override string Description => $"포텐셜 게이지가 {potentialGauge} 이상일 때";
}