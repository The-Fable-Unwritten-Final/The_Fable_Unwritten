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

        // 공격자는 자신의 공격력만 고려
        float attackerAtk = caster.ModifyStat(BuffStatType.Attack, amount);

        // target은 받은 amount에서 방어력을 적용해서 처리
        target.TakeDamage(attackerAtk);

        Debug.Log($"[피해 처리] {caster.ChClass} -> {target.ChClass} : {attackerAtk} 공격력으로 타격");
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
                // 공격자는 자신의 공격력만 고려
                float attackerAtk = caster.ModifyStat(BuffStatType.Attack, amount);

                // target은 받은 amount에서 방어력을 적용해서 처리
                target.TakeDamage(attackerAtk);
                Debug.Log($"[AOE 피해] {caster.ChClass} -> {target.ChClass} : {attackerAtk} 피해 (공:{attackerAtk})");
            }
        }
    }

    /// <summary>
    /// 방어력 무시하고 순수 공격자 기준의 예상 데미지 반환 (UI용)
    /// </summary>
    public float PredictPureDamage(IStatusReceiver caster)
    {
        return caster.ModifyStat(BuffStatType.Attack, amount);
    }

    public override string GetDescription()
    {
        return $"적에게 {amount}의 피해를 줍니다.";
    }

    public override bool isTriggerHitAnim => true;
}