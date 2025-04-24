using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터의 자세가 카드의 요구 자세와 같으면 bonusEffect에 해당하는 효과를 적용
/// </summary>
[CreateAssetMenu(menuName = "Cards/Effects/StanceConditionEffect")]
public class StanceConditionEffect : CardEffectBase
{
    public string requiredStance;
    public CardEffectBase bonusEffect;

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if (caster.CurrentStance == requiredStance)
        {
            bonusEffect?.Apply(caster, target);
        }
    }
    public override string GetDescription() => $"[{requiredStance}] 자세일 경우: {bonusEffect?.GetDescription()}";
}
