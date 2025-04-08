using UnityEngine;

/// <summary>
/// 자해 데미지 처리
/// </summary>
[CreateAssetMenu(menuName = "Card/Effect/SelfDamage")]
public class SelfDamageEffect : CardEffectBase
{
    public float amount;    //자해량
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        caster.TakeDamage(amount);  //시전자에게 데미지
    }
}
