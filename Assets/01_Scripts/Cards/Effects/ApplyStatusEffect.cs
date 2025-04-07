using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버프/다버프 할당 코드
/// </summary>
[CreateAssetMenu(menuName = "CardEffect/ApplyBuff")]
public class ApplyStatusEffect : CardEffectBase
{
    public BuffStatType statType;
    public int value;
    public int duration;

    /// <summary>
    /// 시전자가 타겟에게 버프/디버프를 줌
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        target.ApplyStatusEffect(new StatusEffect
        {
            statType = statType,
            value = value,
            duration = duration
        });
    }
    public override string GetDescription() => $"{statType} 스탯에 {value}만큼 {duration}턴 동안 적용";
}
