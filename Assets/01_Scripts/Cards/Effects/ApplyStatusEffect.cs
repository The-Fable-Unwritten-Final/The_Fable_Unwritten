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

            case 3: // 아군 전체
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

            StatusEffect effect = CreateEffect(statType, value, duration);

            t.ApplyStatusEffect(effect);

            string statusText = GetStatusEffectText(statType, value);

           /* var Text = new DmgTextData
            {
                Text = statusText,
                type = Debuff.IsDebuff(statType, value) ? DmgTextType.Debuff : DmgTextType.Buff,
                isCardEnhanced = isEnhanced == true,
                isStanceEnhanced = caster is PlayerController pc &&
                           (pc.playerData.currentStance == StancType.grace ||
                            pc.playerData.currentStance == StancType.judge),
                isWeakened = false
            };

            t.dmgTextQueue.Enqueue(Text);*/
        }
    }

    private StatusEffect CreateEffect(BuffStatType type, float val, int dur)
    {
        val = Mathf.Clamp(val, -50, 50);

        if (IsTickEffect(type))
        {
            return new TickEffect
            {
                statType = type,
                value = val,
                duration = dur,
            };
        }
        else
        {
            return new InstanceEffect
            {
                statType = type,
                value = val,
                isMaintain = false
            };
        }
    }

    private bool IsTickEffect(BuffStatType type)
    {
        return type switch
        {
            BuffStatType.CantAttackInStance => true,
            BuffStatType.Blind => true,                   // 실명 (명중률 저하 등, 필요 시)
            _ => false
        };
    }

    public override string GetDescription()
    {
        string baseText = $"{statType} {(value > 0 ? "+" : "")}{value}";
        if (IsTickEffect(statType))
            baseText += $" ({duration}턴)";
        return baseText;
    }

    private string GetStatusEffectText(BuffStatType statType, float value)
    {
        string direction = value switch
        {
            > 0 => $"+{value}",
            < 0 => $"{value}",
            _ => ""
        };


        return statType switch
        {
            BuffStatType.Attack => $"공격력 {direction}",
            BuffStatType.Defense => $"방어력 {direction}",
            BuffStatType.Bless => $"축복 {direction}",
            BuffStatType.Grace => $"은총 {direction}",
            BuffStatType.Purify => $"정화 {direction}",
            BuffStatType.Burn => $"화상 {direction}",
            BuffStatType.Freeze => $"빙결 {direction}%",
            BuffStatType.Activate => $"활성화 {direction}",
            BuffStatType.Bleed => $"출혈 {direction}",
            BuffStatType.Stun => $"기절 {direction}",
            BuffStatType.GuardRedirect => $"수호 발동률 {direction}%",
            BuffStatType.CantAttackInStance => $"자세 제한",
            BuffStatType.Blind => $"실명",
            _ => "상태이상"
        };
    }
}

public static class Debuff
{
    public static bool IsDebuff(BuffStatType type, float value)
    {
        return type switch
        {
            BuffStatType.Attack => value < 0,
            BuffStatType.Defense => value < 0,
            BuffStatType.GuardRedirect or BuffStatType.Bless or BuffStatType.Grace => false,
            _ => true
        };
    }
}

