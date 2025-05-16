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
        caster.TakeTrueDamage(amount);  //시전자에게 트루데미지
    }

    public override string GetDescription()
    {
        return $"{amount} 만큼 자신에게 데미지!";
    }
}
