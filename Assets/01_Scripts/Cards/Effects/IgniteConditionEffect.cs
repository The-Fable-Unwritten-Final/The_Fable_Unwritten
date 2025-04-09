using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/IgniteConditionEffect")]

public class IgniteConditionEffect : CardEffectBase
{
    public CardEffectBase bonusEffect;

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if(caster.IsIgnited)
        {
            bonusEffect?.Apply(caster, target);
        }
    }

    public override string GetDescription() => $"각성 상태일 경우: {bonusEffect?.GetDescription()}";
}
