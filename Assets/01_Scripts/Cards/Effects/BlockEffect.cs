using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "CardEffect/BlockEffect")]
public class BlockEffect : CardEffectBase
{
    ///추후 특정 캐릭터 대상 지정 block이 나오면 이 변수를 이용하자
    public CharacterClass? blockTargetClass;
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        
        foreach(var target in targets)
        {
            if (target is PlayerController pc)
            {
                pc.GrantBlock();
                Debug.Log($"{pc.ChClass}에게 block 1회 부여");
            }
        }
        // 추후 적에게도 block 가능하게 하려면 EnemyController도 체크 가능
    }

    public override string GetDescription() => "이 턴 동안 적의 스킬로부터 1회 보호됩니다.";
}
