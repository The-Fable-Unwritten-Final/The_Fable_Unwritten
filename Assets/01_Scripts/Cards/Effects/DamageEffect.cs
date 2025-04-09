using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 데미지 코드
/// </summary>
[CreateAssetMenu(menuName = "CardEffect/DamageEffect")]
public class DamageEffect : CardEffectBase
{
    /// <summary>
    /// 기본 데미지
    /// </summary>
    public float amount;

    /// <summary>
    /// 데미지 처리
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        ApplyAndReturn(caster, target); // 기본 Apply는 AndReturn 재사용
    }


    /// <summary>
    /// 데미지 값을 다른 효과에 쓰기 위한 요소
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    /// <returns>공격 데미지</returns>
    public override float ApplyAndReturn(IStatusReceiver caster, IStatusReceiver target)
    {
        float finalDamage = caster.ModifyStat(BuffStatType.Attack, amount);
        target.TakeDamage(finalDamage);

        // 이후 효과에 활용 가능
        return finalDamage;
    }

    public override void InitializeFromCSV(string param)
    {
        float.TryParse(param, out amount);
    }

    public override string GetDescription() => $"적에게 {amount}의 피해를 줍니다.";
}