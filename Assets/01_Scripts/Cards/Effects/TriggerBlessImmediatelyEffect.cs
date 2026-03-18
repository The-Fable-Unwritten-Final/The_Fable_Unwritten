using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/TriggerBlessImmediatelyEffect")]
public class TriggerBlessImmediatelyEffect : CardEffectBase
{
    public int value = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        foreach (var t in targets)
        {
            if (t is PlayerController pc)
            {
                var bless = pc.instantEffects.Find(e => e.statType == BuffStatType.Bless);
                if (bless != null && bless.value > 0)
                {
                    pc.TryApplyBlessBonus(bless.value);
                }
            }
        }
    }

    public override string GetDescription()
    {
        return "축복 효과를 즉시 발동합니다.";
    }
}