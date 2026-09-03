using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/RepeatEffect")]
public class RepeatEffect : CardEffectBase
{
    public int repeatCount = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        Debug.Log($"[RepeatEffect] repeat x{repeatCount}");
    }

    public override string GetDescription() => $"{repeatCount}회 적용";
}