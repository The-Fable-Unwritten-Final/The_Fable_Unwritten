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
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        //스탯 변경 적용 후
        float finalHeal = caster.ModifyStat(BuffStatType.Defense, amount);
        finalHeal = (isEnhanced == null) ? finalHeal * 1.5f : finalHeal;
        foreach(var target in targets)
        {
            if (target.IsAlive())
                target.Heal(finalHeal);
        }
    }


    public override string GetDescription() => $"대상을 {amount}만큼 회복합니다.";
}
