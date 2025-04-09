using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "CardEffect/BlockEffect")]
public class BlockEffect : CardEffectBase
{
    public CharacterClass blockTargetClass;
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        Debug.Log($"[{caster.CharacterClass}]가 {blockTargetClass}를 대상으로 한 스킬을 막습니다.");
        // target에게 block 상태 적용
        target.ApplyStatusEffect(new StatusEffect
        {
            statType = BuffStatType.block,
            value = 1,
            duration = 1 // 1턴 동안 block
        });

        // 실제 block 상태는 target 측에서 처리하도록 확장 가능
    }

    public override string GetDescription() => "이 턴 동안 적의 스킬로부터 보호됩니다.";
}
