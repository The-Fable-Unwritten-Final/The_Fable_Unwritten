using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버프/다버프 할당 코드
/// </summary>
[CreateAssetMenu(menuName = "CardEffect/ApplyBuff")]
public class ApplyStatusEffect : CardEffectBase
{
    public BuffStatType statType;
    public float value;
    public int duration;
    public int? target;

    /// <summary>
    /// 시전자가 타겟에게 버프/디버프를 줌
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        List<IStatusReceiver> filteredTargets = new();

        switch (target)
        {
            case 0: // 소피아
                foreach (var t in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (t.ChClass == CharacterClass.Sophia) filteredTargets.Add(t);
                }
                break;

            case 1: // 카일라
                foreach (var t in targets)
                {
                    if (t.ChClass == CharacterClass.Kayla) filteredTargets.Add(t);
                }
                break;

            case 2: // 레온
                foreach (var t in targets)
                {
                    if (t.ChClass == CharacterClass.Leon) filteredTargets.Add(t);
                }
                break;

            case 3: // 모든 대상
                filteredTargets.AddRange(targets); break;

            case 4:
            case null:
            default:
                filteredTargets.AddRange(targets); break;
        }

        foreach (var t in filteredTargets)
        {
            if (!t.IsAlive()) continue;

            t.ApplyStatusEffect(new StatusEffect
            {
                statType = statType,
                value = value,
                duration = duration
            });
        }
    }

    public override string GetDescription() => $"{statType} 스탯에 {value}만큼 {duration}턴 동안 적용";
}
