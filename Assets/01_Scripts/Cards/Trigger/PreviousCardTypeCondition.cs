using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전 턴에 특정 타입 카드를 사용하였는지 검사하는 클래스
/// </summary>
public class PreviousCardTypeCondition : TriggerCondition
{
    public List<CardType> requiredTypes;    //요구된 사용 카드 타입

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        var previousTypes = BattleLogManager.Instance.GetPreviousTurnCardTypes();
        foreach (var type in requiredTypes)
        {
            if (previousTypes.Contains(type))
                return true;
        }
        return false;
    }

    public override string Description => "이전 턴에 '" + string.Join(", ", requiredTypes) + "' 타입 카드 사용됨";
}
