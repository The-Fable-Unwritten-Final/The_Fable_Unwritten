using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버프/다버프 할당 코드
/// </summary>
[CreateAssetMenu(menuName = "Cards/Effects/ApplyBuff")]
public class ApplyStatusEffect : CardEffectBase
{
    public BuffStatType statType;
    public float value;
    public int duration;
    public int target = -1;

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
            case -1:
            default:
                filteredTargets.AddRange(targets); break;
        }

        foreach (var t in filteredTargets)
        {
            if (!t.IsAlive()) continue;

            StatusEffect effect = CreateEffect(statType, value, duration);

            // 문체에 따른 버프/디버프 수치 변환 => 이를 위해 value값은 최대한 int형으로 관리 요망
            effect.value = StyleManager.Instance.ModifyBuffDebuffAmount(t, statType, (int)effect.value);

            t.ApplyStatusEffect(effect);

            string statusText = GetStatusEffectText(statType, value);
            var Text = new DmgTextData
            {
                Text = statusText,

                type = GetDmgTextType(statType, value),
                isCardEnhanced = isEnhanced == true,
                isStanceEnhanced = caster is PlayerController pc &&
                           (pc.playerData.currentStance == StancType.Mercy ||
                            pc.playerData.currentStance == StancType.Discipline),
                isWeakened = false
            };

            // Buff/Debuff는 1.5초 지연 후 Enqueue
            if (Text.type > DmgTextType.Heal) // DmgTextType 기준으로 버프/디버프는 2 이상
                GameManager.Instance.StartCoroutine(DelayedEnqueue(t, Text));
            else
                t.dmgTextQueue.InitPrint(Text);
        }
    }

    private IEnumerator DelayedEnqueue(IStatusReceiver target, DmgTextData text)
    {
        yield return new WaitForSeconds(1.5f);
        target.dmgTextQueue.DmgEnqueue(text);
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
        bool useSignedFormat = statType is BuffStatType.Attack or BuffStatType.Defend;

        string valueText = useSignedFormat? value 
            switch
            {
                > 0 => $"+{value}",
                < 0 => value.ToString(),
                _ => ""
            }
            : value != 0
                ? value.ToString()
                : "";   

        return statType switch
        {
            BuffStatType.Attack => valueText,
            BuffStatType.Defend => valueText,
            BuffStatType.Bless => valueText,
            BuffStatType.Sin => valueText,
            BuffStatType.Penance => valueText,
            BuffStatType.Burn => valueText,
            BuffStatType.Freeze => string.IsNullOrEmpty(valueText) ? "" : $"{valueText}%",
            BuffStatType.Activate => valueText,
            BuffStatType.Scar => valueText,
            BuffStatType.Stun => valueText,
            BuffStatType.Guard => string.IsNullOrEmpty(valueText) ? "" : $"{valueText}%",
            BuffStatType.CantAttackInStance => "",
            BuffStatType.Blind => "",
            _ => ""
        };
    }

    private DmgTextType GetDmgTextType(BuffStatType statType, float value)
    {
        return statType switch
        {
            BuffStatType.Attack => value > 0 ? DmgTextType.AttackBuff : DmgTextType.AttackDebuff,
            BuffStatType.Defend => value > 0 ? DmgTextType.DefenseBuff : DmgTextType.DefenseDebuff,
            
            // 이름이 동일한 것들
            BuffStatType.Burn => DmgTextType.Burn,
            BuffStatType.Freeze => DmgTextType.Freeze,
            BuffStatType.Bless => DmgTextType.Bless,
            BuffStatType.Penance => DmgTextType.Penance,
            BuffStatType.Guard => DmgTextType.Guard,
            BuffStatType.Scar => DmgTextType.Scar,
            BuffStatType.Stun => DmgTextType.Stun,
            BuffStatType.Sin => DmgTextType.Crime,
            BuffStatType.Activate => DmgTextType.Activate,
            
            _ => DmgTextType.Normal,
        };
    }
}

// 이거 문체 시스템에서 버프 디버프 체킹용으로 추가 했어요, 아래쪽에 purify는 없어서 혹시 몰라서 새로 만들었습니다 -민준-
public static class Buff
{
    public static bool IsBuff(BuffStatType type, float value)
    {
        return type switch
        {
            BuffStatType.Attack => value > 0,
            BuffStatType.Defend => value > 0,
            BuffStatType.Guard or BuffStatType.Bless or BuffStatType.Penance => true,
            _ => false
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
            BuffStatType.Defend => value < 0,
            BuffStatType.Guard or BuffStatType.Bless or BuffStatType.Penance=> false, BuffStatType.Undying => false,
            _ => true
        };
    }
    public static ApplyStatusEffect GetRandomDebuffEffect()
    {
        // 후보 효과들: Burn, Freeze, Activate, Bleed, Stun, GuardRedirect, Blind
        BuffStatType[] candidates = new BuffStatType[]
        {
            BuffStatType.Attack,
            BuffStatType.Defend,
            BuffStatType.Burn,
            BuffStatType.Freeze,
            BuffStatType.Scar,
            BuffStatType.Stun,
            BuffStatType.Blind
        };
    
        int idx = Random.Range(0, candidates.Length);
        BuffStatType chosen = candidates[idx];

        var result = ScriptableObject.CreateInstance<ApplyStatusEffect>();
        result.statType = chosen;

        // 각 디버프 효과별 1턴 값 수치 적용
        switch (chosen)
        {
            case BuffStatType.Attack:
                result.value = -1;
                result.duration = 1;
                break;
            case BuffStatType.Defend:
                result.value = -1;
                result.duration = 1;
                break;
            case BuffStatType.Burn:
                result.value = 3; // 화상 피해량
                result.duration = 1;
                break;
            case BuffStatType.Freeze:
                result.value = 100; // 빙결 확률/비율(%)
                result.duration = 1;
                break;
            case BuffStatType.Scar:
                result.value = 3;
                result.duration = 1;
                break;
            case BuffStatType.Stun:
                result.value = 1; 
                result.duration = 1; // 기절 지속(턴)
                break;
            case BuffStatType.Blind:
                result.value = -1; 
                result.duration = 1;
                break;
            default:
                result.value = 1;
                result.duration = 1;
                break;
        }

        return result;
    }

    public static ApplyStatusEffect GetStunEffect(int dur)
    {
        var result = ScriptableObject.CreateInstance<ApplyStatusEffect>();
        result.statType = BuffStatType.Stun;
        result.value =1;
        result.duration = dur;

        return result;
    }
}

