using UnityEngine;

[CreateAssetMenu(menuName = "CardEffect/BlindEffect")]
public class BlindEffect : CardEffectBase
{
    public PlayerData.StencType blockedStance;
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        // 상태 이상으로 처리
        target.ApplyStatusEffect(new StatusEffect
        {
            statType = BuffStatType.blind,
            value = -1, // 블라인드 효과
            duration = 1
        });
    }

    public override string GetDescription() => "적의 방향 지정 스킬을 무효화합니다.";
}