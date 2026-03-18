using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/DamagePercentEffect")]
public class DamagePercentEffect : CardEffectBase
{
    public int percent;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        // 이 효과는 단독 발동용보다
        // Conditional 내부에서 다음 DamageEffect와 조합되는 쪽이 더 자연스러움.
        // 현재 구조에서는 BattleContext 없이 즉시 적용이 어려우므로 로그만 남김.
        Debug.Log($"[DamagePercentEffect] damage +{percent}%");
    }

    public override string GetDescription() => $"피해 +{percent}%";
}