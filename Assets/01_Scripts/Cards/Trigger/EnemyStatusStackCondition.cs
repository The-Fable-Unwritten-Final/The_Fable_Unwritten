using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/Conditions/EnemyStatusStackCondition")]
public class EnemyStatusStackCondition : TriggerCondition
{
    public BuffStatType statusType;
    public int requiredStack;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (target == null) return false;

        foreach (var t in target)
        {
            if (t is Enemy enemy)
            {
                float value = enemy.GetEffectValue(statusType);
                if (value >= requiredStack)
                    return true;
            }
        }

        return false;
    }

    public override string Description => $"적의 {statusType} 수치가 {requiredStack} 이상일 때";
}