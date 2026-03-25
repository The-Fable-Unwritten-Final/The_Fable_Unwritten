using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/MultiplyBuffEffect")]
public class MultiplyBuffEffect : CardEffectBase
{
    public int value = 2;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (targets == null) return;

        foreach (var t in targets)
        {
            if (t is PlayerController pc)
            {
                foreach (var effect in pc.instantEffects)
                {
                    if (!Debuff.IsDebuff(effect.statType, effect.value))
                        effect.value *= value;
                }

                foreach (var effect in pc.tickEffects)
                {
                    if (!Debuff.IsDebuff(effect.statType, effect.value))
                        effect.value *= value;
                }
            }
        }
    }

    public override string GetDescription()
    {
        return $"이로운 효과를 {value}배로 만듭니다.";
    }
}