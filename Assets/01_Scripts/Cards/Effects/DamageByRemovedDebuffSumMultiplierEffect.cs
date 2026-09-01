using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffect/DamageByRemovedDebuffSumMultiplierEffect")]
public class DamageByRemovedDebuffSumMultiplierEffect : CardEffectBase
{
    public int multiplier;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        var card = BattleLogManager.Instance.card;
        if (card == null) return;

        var removeEffect = card.effects
            .OfType<RemoveDebuffFromEnemyEffect>()
            .FirstOrDefault();

        if (removeEffect == null || removeEffect.LastRemovedSum <= 0)
            return;

        float damage = removeEffect.LastRemovedSum * multiplier;

        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive()) continue;

            target.TakeDamage(damage);
        }
    }

    public override string GetDescription()
    {
        return $"제거한 디버프 수치 합의 {multiplier}배 피해";
    }
}