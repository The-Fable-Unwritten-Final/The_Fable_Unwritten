using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/CantAttackInStanceEffect")]

public class CantAttackInStance : CardEffectBase
{
    public StancValue.EStancType blockStance;
    public int duration = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        foreach(var target in targets)
        {
            if (!target.IsAlive()) continue;

            target.ApplyStatusEffect(new StatusEffect
            {
                statType = BuffStatType.CantAttackInStance,
                value = (float)blockStance,
                duration = duration
            });

            Debug.Log($"[CantAttackInStanceEffect] {target} -> {blockStance} 공격 불가 {duration}턴");
        }
    }

    public override string GetDescription() => $"{blockStance} 위치로 공격할 수 없습니다 ({duration}턴)";
}
