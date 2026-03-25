using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/MultiplyBlessEffect")]
public class MultiplyBlessEffect : CardEffectBase
{
    public int value = 2;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (targets == null) return;

        foreach (var t in targets)
        {
            if (t is PlayerController pc)
            {
                var bless = pc.instantEffects.Find(e => e.statType == BuffStatType.Bless);
                if (bless != null)
                {
                    bless.value *= value;
                }
            }
        }
    }

    public override string GetDescription()
    {
        return $"축복 수치를 {value}배로 만듭니다.";
    }
}