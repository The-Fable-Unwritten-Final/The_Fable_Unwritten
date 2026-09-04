using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/NoBlessConsumeEffect")]
public class NoBlessConsumeEffect : CardEffectBase
{
    public int value = 1;

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
                    bless.isMaintain = true;
                }
            }
        }
    }

    public override string GetDescription()
    {
        return "축복을 소모하지 않습니다.";
    }
}