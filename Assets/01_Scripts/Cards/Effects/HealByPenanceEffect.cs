using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffect/HealByPenanceEffect")]
public class HealByPenanceEffect : CardEffectBase
{
    public int value;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (targets == null || targets.Count == 0) return;

        foreach (var target in targets)
        {
            if (target is not PlayerController pc) continue;

            float penanceValue = pc.GetEffectValue(BuffStatType.Penance);
            if (penanceValue <= 0) continue;

            pc.Heal(penanceValue * value);
        }
    }

    public override string GetDescription()
    {
        return $"참회 수치 × {value}만큼 추가 회복";
    }
}