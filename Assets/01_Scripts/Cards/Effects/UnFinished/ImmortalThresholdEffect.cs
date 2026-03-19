using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/ImmortalThresholdEffect")]
public class ImmortalThresholdEffect : CardEffectBase
{
    public int minHp = 1;
    public int count = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (targets == null) return;

        foreach (var t in targets)
        {
            if (t is PlayerController pc)
            {
                pc.ApplyStatusEffect(new InstanceEffect
                {
                    statType = BuffStatType.Guard,
                    value = count,
                    isMaintain = true
                });
            }
        }
    }

    public override string GetDescription()
    {
        return $"이 턴에 체력이 {minHp} 미만으로 내려가지 않음 ({count}회)";
    }
}