using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// 데미지 코드
/// </summary>
[CreateAssetMenu(menuName = "CardEffect/DamageEffect")]
public class DamageEffect : CardEffectBase
{
    public float amount;    //기본 데미지

    /// <summary>
    /// 데미지 처리
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if (!target.IsAlive()) return;
        ApplyAndReturn(caster, target);
    }


    /// <summary>
    /// 데미지 값을 다른 효과에 쓰기 위한 요소
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    /// <returns>공격 데미지</returns>
    public override float ApplyAndReturn(IStatusReceiver caster, IStatusReceiver target)
    {
        if (!target.IsAlive()) return 0;

        float finalDamage = caster.ModifyStat(BuffStatType.Attack, amount);
        target.TakeDamage(finalDamage);

        // 이후 효과에 활용 가능
        return finalDamage;
    }


    /// <summary>
    /// 광역뎀에 사용
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="targets">타겟 리스트</param>
    public override void ApplyAOE(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        foreach (var target in targets)
        {
            if (target.IsAlive())
            {
                float finalDamage = caster.ModifyStat(BuffStatType.Attack, amount);
                target.TakeDamage(finalDamage);
                Debug.Log($"[AOE 피해] {target.CharacterClass}에게 {finalDamage} 피해");
            }
        }
    }

    public override string GetDescription()
    {
        return $"적에게 {amount}의 피해를 줍니다.";
    }
}