using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalValueCondition : TriggerCondition
{
    public BuffStatType statusType;
    public int requiredValue;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        float total = 0;
        var battleFlow = GameManager.Instance?.turnController?.battleFlow;
        if (battleFlow == null) return false;

        foreach (var enemy in battleFlow.enemyParty)
        {
            if (enemy.IsAlive())
                total += enemy.GetEffectValue(statusType);
        }
        return total >= requiredValue;
    }

    public override string Description => $"전체 {statusType} 합 {requiredValue} 이상";
}
