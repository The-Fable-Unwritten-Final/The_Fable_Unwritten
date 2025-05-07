using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 턴 카드 사용 조건 
/// </summary>
public class CurrentCardTypeCondition : TriggerCondition
{
    public List<CardType> requiredTypes;

    public override bool IsConditionMet(IStatusReceiver caster, IStatusReceiver target)
    {
        var currentTypes = BattleLogManager.Instance.GetCurrentTurnCardTypes();
        foreach (var type in requiredTypes)
            if (currentTypes.Contains(type)) return true;
        return false;
    }

    public override string Description => "이번 턴에 '" + string.Join(", ", requiredTypes) + "' 타입 사용됨";
}
