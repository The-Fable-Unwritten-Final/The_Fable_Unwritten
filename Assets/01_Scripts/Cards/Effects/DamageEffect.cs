using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 데미지 코드
/// </summary>
[CreateAssetMenu(menuName = "CardEffect/Damage")]
public class DamageEffect : CardEffectBase
{
    /// <summary>
    /// 기본 데미지
    /// </summary>
    public int baseDamage;

    /// <summary>
    /// 데미지 처리
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        ///최종 데미지 계산
        int finalDamage = caster.ModifyStat(BuffStatType.Attack, baseDamage);
        target.TakeDamage(finalDamage);
    }

    public override string GetDescription() => $"적에게 {baseDamage}의 피해를 줍니다.";
}