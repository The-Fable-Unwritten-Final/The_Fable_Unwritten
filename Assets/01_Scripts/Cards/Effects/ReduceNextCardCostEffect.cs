using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effect/ReduceCost")]
public class ReduceNextCardCostEffect : CardEffectBase
{
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        // 구현 필요: 다음 카드 비용을 줄이는 로직
    }
}