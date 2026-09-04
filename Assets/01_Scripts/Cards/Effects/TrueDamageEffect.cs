using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/TrueDamageEffect")]
public class TrueDamageEffect : CardEffectBase
{
    public float amount;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (targets == null || targets.Count == 0) return;

        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive()) continue;

            target.TakeTrueDamage(amount);
        }
    }

    public override string GetDescription()
    {
        return $"적에게 {amount}의 고정 피해를 줍니다.";
    }

    public override bool isTriggerHitAnim => true;
}