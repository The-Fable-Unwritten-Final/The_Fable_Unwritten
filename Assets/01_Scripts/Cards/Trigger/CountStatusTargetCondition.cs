using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountStatusTargetCondition : TriggerCondition
{
    public BuffStatType statusType;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        return GetCount() > 0;
    }

    public int GetCount()
    {
        int count = 0;
        var battleFlow = GameManager.Instance?.turnController?.battleFlow;
        if (battleFlow == null) return 0;

        foreach (var enemy in battleFlow.enemyParty)
        {
            if (enemy.IsAlive() && enemy.HasEffect(statusType))
                count++;
        }
        return count;
    }

    public override string Description => $"{statusType} 보유 적 수";
}
