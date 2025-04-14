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

        float attackerAtk = caster.ModifyStat(BuffStatType.Attack, amount);  // 공격력 기반 수치
        float targetDef = target.ModifyStat(BuffStatType.Defense, 0f);       // 방어력 수치

        float finalDamage = Mathf.Max(attackerAtk - targetDef, 1f); // 최소 피해량 1 보장
        target.TakeDamage(finalDamage);

        Debug.Log($"[피해 처리] {caster.ChClass} -> {target.ChClass} : {finalDamage} 피해 (공:{attackerAtk}, 방:{targetDef})");
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
                float attackerAtk = caster.ModifyStat(BuffStatType.Attack, amount);
                float targetDef = target.ModifyStat(BuffStatType.Defense, 0f);
                float finalDamage = Mathf.Max(attackerAtk - targetDef, 1f);

                target.TakeDamage(finalDamage);
                Debug.Log($"[AOE 피해] {caster.ChClass} -> {target.ChClass} : {finalDamage} 피해 (공:{attackerAtk}, 방:{targetDef})");
            }
        }
    }

    public override string GetDescription()
    {
        return $"적에게 {amount}의 피해를 줍니다.";
    }
}