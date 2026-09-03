using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/DamagePercentEffect")]
public class DamagePercentEffect : CardEffectBase
{
    public int percent;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        var card = BattleLogManager.Instance.card;
        if (card == null) return;

        var damageEffect = card.effects.Find(e => e is DamageEffect) as DamageEffect;
        if (damageEffect == null) return;

        damageEffect.ApplyWithMultiplier(caster, targets, percent / 100f, isEnhanced);
    }

    public override string GetDescription() => $"피해 +{percent}%";
}