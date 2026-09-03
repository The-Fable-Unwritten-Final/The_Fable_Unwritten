using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// 데미지 코드
/// </summary>
[CreateAssetMenu(menuName = "Cards/Effects/DamageEffect")]
public class DamageEffect : CardEffectBase
{
    public float amount;    //기본 데미지

    /// <summary>
    /// 데미지 처리 (다중)
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        ApplyWithMultiplier(caster, targets, 1f, isEnhanced);
    }

    public void ApplyWithMultiplier(IStatusReceiver caster, List<IStatusReceiver> targets, float multiplier, bool? isEnhanced = null)
    {
        if (targets == null || targets.Count == 0) return;

        float attackerAtk = caster.ModifyStat(BuffStatType.Attack, amount);
        var card = BattleLogManager.Instance.card;
        bool stanceBoosted = false;
        bool stanceWeakened = false;

        attackerAtk = isEnhanced == true ? attackerAtk * 1.5f : attackerAtk;

        if (caster is PlayerController pc)
        {
            (attackerAtk, stanceBoosted, stanceWeakened) =
                StanceHelper.ApplyStanceToDamage(pc, attackerAtk, card.type);
        }

        // 조건부 피해 배율
        attackerAtk *= multiplier;

        attackerAtk = Mathf.Round(attackerAtk);
        attackerAtk = StyleManager.Instance.GetDamageGiveModify(caster, null, card, attackerAtk);

        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive()) continue;

            float finalAttackDamage = attackerAtk;

            finalAttackDamage = target.ApplyScarAttackBonus(finalAttackDamage);

            float result = target.TakeDamage(finalAttackDamage);

            if (target is Enemy enemy)
            {
                var context = new DamageContext
                {
                    Attacker = caster,
                    Target = target,
                    SourceCard = card,
                    RawDamage = finalAttackDamage,
                    FinalDamage = result,
                    IsAttackDamage = true,
                    IsTrueDamage = false,
                    HitIndex = 0
                };

                enemy.Mechanic?.OnDamaged(context);
            }

            var dmgData = new DmgTextData
            {
                Text = $"{Mathf.RoundToInt(result)}",
                type = DmgTextType.Normal,
                isStanceEnhanced = stanceBoosted,
                isCardEnhanced = isEnhanced == true,
                isWeakened = stanceWeakened
            };

            target.dmgTextQueue.InitPrint(dmgData);
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