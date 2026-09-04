using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/AutoCastEffect")]
public class AutoCastEffect : CardEffectBase
{
    public int count = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        Debug.Log($"[AutoCastEffect] 자동 발동 {count}회");
    }

    public override string GetDescription() => $"자동 발동 {count}회";
}