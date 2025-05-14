using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 턴 카드 사용 조건 
/// </summary>
public class CurrentCardTypeCondition : TriggerCondition
{
    [Tooltip("모든 타입을 만족해야 하는지(And), 하나라도 만족하면 되는지(Or)")]
    public bool isAnd = true;

    public List<CardType> requiredTypes;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        var currentTypes = BattleLogManager.Instance.GetCurrentTurnCardTypes();

        if (isAnd)
        {
            return requiredTypes.TrueForAll(type => currentTypes.Contains(type));
        }
        else
        {
            return requiredTypes.Exists(type => currentTypes.Contains(type));
        }
    }

    public override string Description =>
            $"이번 턴에 {(isAnd ? "모든" : "하나 이상의")} 타입 사용됨: {string.Join(", ", requiredTypes)}";
}
