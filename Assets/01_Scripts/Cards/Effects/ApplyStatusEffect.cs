using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffect/ApplyBuff")]
public class ApplyStatusEffect : CardEffectBase
{
    public BuffStatType statType;
    public float value;
    public int duration;
    public int? target;

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
            case 0:
                foreach (var t in playerParty)
                    if (t.ChClass == CharacterClass.Sophia) filtered.Add(t);
                break;
            case 1:
                foreach (var t in playerParty)
                    if (t.ChClass == CharacterClass.Kayla) filtered.Add(t);
                break;
            case 2:
                foreach (var t in playerParty)
                    if (t.ChClass == CharacterClass.Leon) filtered.Add(t);
                break;
            case 3:
                filtered.AddRange(playerParty);
                break;
            case 5: // 적 전체
                filtered.AddRange(GameManager.Instance.turnController.battleFlow.enemyParty);
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
            BuffStatType.Blind => true,
            BuffStatType.Undying => true,  // 추가
            _ => false
        };
    }

    public override string GetDescription()
    {
        string baseText = $"{GetStatusEffectText(statType, value)} {(value > 0 ? "+" : "")}{value}";
        if (IsTickEffect(statType))
            baseText += $" ({duration}턴)";
        return baseText;
    }

    private string GetStatusEffectText(BuffStatType statType, float value)
    {
        return statType switch
        {
            BuffStatType.Attack => "공격력",
            BuffStatType.Defense => "방어력",
            BuffStatType.Burn => "화상",
            BuffStatType.Freeze => "빙결",
            BuffStatType.Activate => "활성",
            BuffStatType.Bless => "축복",
            BuffStatType.Crime => "죄악",
            BuffStatType.Penance => "참회",
            BuffStatType.Scar => "상처",
            BuffStatType.Stun => "기절",
            BuffStatType.Guard => "수호",
            BuffStatType.Undying => "불사",  // 추가
            _ => "상태이상"
        };
    }
}