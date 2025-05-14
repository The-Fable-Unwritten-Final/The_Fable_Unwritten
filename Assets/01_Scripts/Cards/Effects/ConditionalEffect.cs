using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cards/Effects/ConditionalEffect")]
public class ConditionalEffect : CardEffectBase
{
    public TriggerCondition condition;      //효과 만족을 검사할 코드
    public CardEffectBase effectIfTrue;     //효과 만족시 실행할 다른 효과.

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        if (condition != null && condition.IsConditionMet(caster, targets))
        {
            effectIfTrue?.Apply(caster, targets);
        }
    }


    public override string GetDescription()
    {
        return condition?.Description + " → " + effectIfTrue?.GetDescription();
    }
}
