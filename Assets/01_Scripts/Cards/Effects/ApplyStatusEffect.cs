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
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
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
                foreach (var t in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (t.ChClass == CharacterClass.Kayla) filteredTargets.Add(t);
                }
                break;

            case 2: // 레온
                foreach (var t in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (t.ChClass == CharacterClass.Leon) filteredTargets.Add(t);
                }
                break;

            case 3: // all
                foreach (var t in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    filteredTargets.Add(t);
                }
                break;

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

            string statusText = GetStatusEffectText(statType, value);

            var Text = new DmgTextData
            {
                Text = statusText,
                type = value < 0 ? DmgTextType.Debuff : DmgTextType.Buff,
                isCardEnhanced = isEnhanced == true,
                isStanceEnhanced = caster is PlayerController pc &&
                           (pc.playerData.currentStance == PlayerData.StancType.grace ||
                            pc.playerData.currentStance == PlayerData.StancType.judge),
                isWeakened = false
            };

            t.dmgBar.Initialize(Text, t.CachedTransform.position);
        }
    }

    public override string GetDescription() => $"{statType} 스탯에 {value}만큼 {duration}턴 동안 적용";

    string GetStatusEffectText(BuffStatType statType, float value)
    {
        string direction = value switch
        {
            > 0 => $"+{value}",
            < 0 => $"{value}",
            _ => ""
        };

        string iconTag = statType switch
        {
            BuffStatType.Attack => "<sprite name=\"atk\">",
            BuffStatType.Defense => "<sprite name=\"def\">",
            _ => ""
        };

        return statType switch
        {
            BuffStatType.Attack => $"{iconTag} {direction}",
            BuffStatType.Defense => $"{iconTag} {direction}",
            BuffStatType.ManaRegen => $"{iconTag}  {direction}",
            BuffStatType.stun => "기절",
            BuffStatType.CantAttackInStance => "실명",
            _ => "상태이상"
        };
    }
}
