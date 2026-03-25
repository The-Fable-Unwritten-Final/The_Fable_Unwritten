using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/Conditions/SinTotalCondition")]
public class SinTotalCondition : TriggerCondition
{
    public int requiredValue;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (target == null) return false;

        float total = 0f;

        foreach (var t in target)
        {
            if (t is Enemy enemy)
            {
                total += enemy.GetEffectValue(BuffStatType.Crime);
            }
        }

        return total >= requiredValue;
    }

    public override string Description => $"적 전체의 죄악 합이 {requiredValue} 이상일 때";
}