using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanceConditionEffect : CardEffectBase
{
    public string requiredStance;
    public CardEffectBase bonusEffect;

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if (caster.CurrentStance == requiredStance)
        {
            bonusEffect?.Apply(caster, target);
        }
    }
    public override string GetDescription() => $"[{requiredStance}] 자세일 경우: {bonusEffect?.GetDescription()}";
}
