using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 힐 코드
/// </summary>
[CreateAssetMenu(menuName = "CardEffect/Heal")]
public class HealEffect : CardEffectBase
{
    /// <summary>
    /// 기본 힐량
    /// </summary>
    public int baseHeal;

    /// <summary>
    /// 실제로 힐이 진행될 코드
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        //스탯 변경 적용 후
        int finalHeal = caster.ModifyStat(BuffStatType.ManaRegen, baseHeal);
        caster.Heal(finalHeal); //힐 적용
    }

    public override string GetDescription() => $"대상을 {baseHeal}만큼 회복합니다.";
}
