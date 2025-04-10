using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 힐 코드
/// </summary>
[CreateAssetMenu(menuName = "Cards/Effects/HealEffect")]
public class HealEffect : CardEffectBase
{

    public float amount;    //힐량

    /// <summary>
    /// 실제로 힐이 진행될 코드
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        //스탯 변경 적용 후
        float finalHeal = caster.ModifyStat(BuffStatType.Attack, amount);
        caster.Heal(finalHeal); //힐 적용
    }

    /// <summary>
    /// 광역힐에 사용
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="targets">타겟 리스트</param>
    public override void ApplyAOE(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        foreach (var target in targets)
        {
            if (target.IsAlive())
            {
                float finalHeal = caster.ModifyStat(BuffStatType.Attack, amount);
                caster.Heal(finalHeal);
            }
        }
    }


    public override string GetDescription() => $"대상을 {amount}만큼 회복합니다.";
}
