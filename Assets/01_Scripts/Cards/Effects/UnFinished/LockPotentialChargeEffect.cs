using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/LockPotentialChargeEffect")]
public class LockPotentialChargeEffect : CardEffectBase
{
    public int turns = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (caster is PlayerController pc)
        {
            pc.SetPotentialChargeLock(turns);
        }
    }

    public override string GetDescription() => $"{turns}턴 동안 포텐셜 충전 불가";
}