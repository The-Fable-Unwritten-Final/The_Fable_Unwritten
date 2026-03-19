using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/Conditions/EnemyHasStatusCondition")]
public class EnemyHasStatusCondition : TriggerCondition
{
    public BuffStatType statusType;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (target == null) return false;

        foreach (var t in target)
        {
            if (t is Enemy enemy)
            {
                if (enemy.HasEffect(statusType))
                    return true;
            }
        }

        return false;
    }

    public override string Description => $"적이 {statusType} 상태일 때";
}