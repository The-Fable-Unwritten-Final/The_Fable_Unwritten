using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cards/Effects/ConditionalEffect")]
public class ConditionalEffect : CardEffectBase
{
    public CardEffectBase effectIfTrue;     //효과 만족시 실행할 다른 효과.
    public TriggerCondition condition;      //효과 만족을 검사할 코드

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if (condition != null && condition.IsConditionMet(caster, target))
        {
            effectIfTrue?.Apply(caster, target);
        }
    }


    public override string GetDescription()
    {
        return condition?.Description + " → " + effectIfTrue?.GetDescription();
    }
}
