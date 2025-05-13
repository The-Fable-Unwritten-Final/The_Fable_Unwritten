using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이번 턴에 특정 타입 카드가 사용되지 않았을 경우 조건 만족
/// </summary>
public class NotCurrentCardTypeCondition : TriggerCondition
{
    public List<CardType> forbiddenTypes;
    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        var usedTypes = BattleLogManager.Instance.GetCurrentTurnCardTypes();

        foreach (var forbidden in forbiddenTypes)
        {
            if (usedTypes.Contains(forbidden))
                return false;
        }
        return true;
    }

    public override string Description =>
        $"이번 턴에 '{string.Join(", ", forbiddenTypes)}' 타입을 사용하지 않았다면";
}
