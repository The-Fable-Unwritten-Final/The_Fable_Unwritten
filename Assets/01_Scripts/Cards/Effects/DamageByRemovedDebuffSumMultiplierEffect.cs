using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/DamageByRemovedDebuffSumMultiplierEffect")]
public class DamageByRemovedDebuffSumMultiplierEffect : CardEffectBase
{
    public int multiplier = 1;
    public int removedSum = 0;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (targets == null) return;

        int damage = removedSum * multiplier;
        if (damage <= 0) return;

        foreach (var t in targets)
        {
            t.TakeDamage(damage);
        }
    }

    public override string GetDescription()
    {
        return $"제거한 해로운 효과 수치 합의 {multiplier}배 피해";
    }

    public void SetRemovedSum(int value)
    {
        removedSum = value;
    }
}