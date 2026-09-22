using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/DamagePercentEffect")]
public class DamagePercentEffect : CardEffectBase
{
    public int percent;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
    }

    public override string GetDescription() => $"피해 +{percent}%";
}