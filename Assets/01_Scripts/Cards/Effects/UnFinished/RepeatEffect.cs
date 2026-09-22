using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/RepeatEffect")]
public class RepeatEffect : CardEffectBase
{
    // 추가 반복 횟수가 아니라 "총 적용 횟수"
    public int repeatCount = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        // 실제 반복 실행은 CardModel에서 처리
    }

    public override string GetDescription()
    {
        return $"{repeatCount}회 적용";
    }
}