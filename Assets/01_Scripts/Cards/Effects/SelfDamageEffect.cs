using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자해 데미지 처리
/// </summary>
[CreateAssetMenu(menuName = "Cards/Effects/SelfDamageEffect")]
public class SelfDamageEffect : CardEffectBase
{
    public float amount;    //자해량
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (caster == null) return;

        float damage = Mathf.Min(amount, caster.currentHP - 1);
        caster.TakeTrueDamage(damage);

    }

    public override string GetDescription()
    {
        return $"{amount} 만큼 자신에게 데미지!";
    }
}
