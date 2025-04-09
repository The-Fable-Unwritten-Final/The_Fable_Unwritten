using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "CardEffects/AOE/DamageAOE")]
public class AOEDamageEffect : CardEffectBase
{
    public float damageAmount;

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        // 개별 적용은 비워두거나 단일 대상일 경우 사용
        target.TakeDamage(damageAmount);
    }

    public override void ApplyAOE(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        foreach (var target in targets)
        {
            if (target.IsAlive())
            {
                float finalDamage = caster.ModifyStat(BuffStatType.Attack, damageAmount);
                target.TakeDamage(finalDamage);
                Debug.Log($"[AOE 피해] {target.CharacterClass}에게 {finalDamage} 피해");
            }
        }
    }

    public override string GetDescription() => $"다수의 적에게 {damageAmount}의 피해";

    public override void InitializeFromCSV(string param)
    {
        float.TryParse(param, out damageAmount);
    }
}
