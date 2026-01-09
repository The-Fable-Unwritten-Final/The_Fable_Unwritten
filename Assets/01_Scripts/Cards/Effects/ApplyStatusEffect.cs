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
        List<IStatusReceiver> filteredTargets = GetFilteredTargets(targets);

        foreach (var t in filteredTargets)
        {
            if (!t.IsAlive()) continue;

            StatusEffect effect = CreateEffect(statType, value, duration);
            t.ApplyStatusEffect(effect);
        }
    }

    private List<IStatusReceiver> GetFilteredTargets(List<IStatusReceiver> targets)
    {
        List<IStatusReceiver> filtered = new();
        var playerParty = GameManager.Instance.turnController.battleFlow.playerParty;

        switch (target)
        {
            case 0: // 소피아
                foreach (var t in playerParty)
                    if (t.ChClass == CharacterClass.Sophia) filtered.Add(t);
                break;
            case 1: // 카일라
                foreach (var t in playerParty)
                    if (t.ChClass == CharacterClass.Kayla) filtered.Add(t);
                break;
            case 2: // 레온
                foreach (var t in playerParty)
                    if (t.ChClass == CharacterClass.Leon) filtered.Add(t);
                break;
            case 3: // 아군 전체
                filtered.AddRange(playerParty);
                break;
            case 4:
            case null:
            default:
                filtered.AddRange(targets);
                break;
        }

        return filtered;
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
            BuffStatType.Attack => "공격력",
            BuffStatType.Defense => "방어력",
            BuffStatType.Burn => "화상",
            BuffStatType.Freeze => "빙결",
            BuffStatType.Activate => "자연",
            BuffStatType.Bless => "축복",
            BuffStatType.Crime => "죄악",
            BuffStatType.Penance => "참회",
            BuffStatType.Scar => "상처",
            BuffStatType.Stun => "기절",
            BuffStatType.Guard => "수호",
            _ => "상태이상"
        };
    }
}

public static class Debuff
{
    public static bool IsDebuff(BuffStatType type, float value)
    {
        return StatusEffectSystem.IsHarmful(type) ||
               (type == BuffStatType.Attack && value < 0) ||
               (type == BuffStatType.Defense && value < 0);
    }
}
