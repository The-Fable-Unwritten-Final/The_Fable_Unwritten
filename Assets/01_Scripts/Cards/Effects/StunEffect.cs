using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/StunEffect")]
public class StunEffect : CardEffectBase
{
    public int duration;
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        target.ApplyStatusEffect(new StatusEffect
        {
            statType = BuffStatType.stun,
            value = -999, // stun은 value로 처리하기보다 별도 처리 권장
            duration = duration
        });
        // 타겟 내부에서 stun 상태를 해석하도록
    }

    public override string GetDescription() => "대상을 1턴간 기절시킵니다.";
}
