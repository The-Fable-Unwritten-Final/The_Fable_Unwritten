using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 버프/디버프 상태효과를 관리하는 시스템
/// </summary>
public class StatusEffectSystem
{
    private readonly List<TickEffect> tickEffects = new();
    private readonly List<InstanceEffect> instantEffects = new();

    private IStatusReceiver owner;

    public event Action OnEffectsChanged;

    public IReadOnlyList<TickEffect> TickEffects => tickEffects;
    public IReadOnlyList<InstanceEffect> InstantEffects => instantEffects;

    public StatusEffectSystem(IStatusReceiver owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// 특정 시점의 효과들 실행
    /// </summary>
    public void ExecuteTrigger(EffectTriggerType trigger)
    {
        for (int i = instantEffects.Count - 1; i >= 0; i--)
        {
            var effect = instantEffects[i];
            if (effect.value <= 0) continue;

            ExecuteEffect(effect, trigger);

            if (effect.value <= 0)
                instantEffects.RemoveAt(i);
        }

        // 턴 종료 시 추가 처리
        if (trigger == EffectTriggerType.TurnEnd)
        {
            ProcessTickEffects();
            ResetMaintainFlags();
        }

        OnEffectsChanged?.Invoke();
    }

    private void ExecuteEffect(InstanceEffect effect, EffectTriggerType trigger)
    {
        switch (effect.statType)
        {
            // 화상: 턴 시작 시 피해, 절반 감소
            case BuffStatType.Burn:
                if (trigger == EffectTriggerType.TurnStart && !effect.isMaintain)
                {
                    owner.TakeTrueDamage(effect.value);
                    effect.value = Mathf.FloorToInt(effect.value / 2f);
                }
                break;

            // 빙결: 턴 종료 시 초기화
            case BuffStatType.Freeze:
                if (trigger == EffectTriggerType.TurnEnd)
                {
                    effect.value = 0;
                }
                break;

            // 죄악: 턴 종료 시 피해 후 제거
            case BuffStatType.Crime:
                if (trigger == EffectTriggerType.TurnEnd)
                {
                    owner.TakeTrueDamage(effect.value);
                    effect.value = 0;
                }
                break;

            // 수호: 턴 종료 시 초기화
            case BuffStatType.Guard:
                if (trigger == EffectTriggerType.TurnEnd)
                {
                    effect.value = 0;
                }
                break;

            // 기절: 턴 시작 시 확률 판정
            case BuffStatType.Stun:
                if (trigger == EffectTriggerType.TurnStart)
                {
                    TryTriggerStunInternal(effect);
                }
                break;
        }
    }

    private void ProcessTickEffects()
    {
        for (int i = tickEffects.Count - 1; i >= 0; i--)
        {
            tickEffects[i].duration--;
            if (tickEffects[i].duration <= 0)
                tickEffects.RemoveAt(i);
        }
    }

    private void ResetMaintainFlags()
    {
        foreach (var effect in instantEffects)
            effect.isMaintain = false;
    }

    /// <summary>
    /// 상태효과 적용
    /// </summary>
    public void ApplyEffect(StatusEffect effect)
    {
        // 자연(Active) 보너스 적용
        if (effect is InstanceEffect inst && inst.statType != BuffStatType.Activate)
        {
            int activeBonus = GetActiveBonus();
            if (activeBonus > 0)
                inst.value += activeBonus;
        }

        switch (effect)
        {
            case TickEffect tick:
                ApplyTickEffect(tick);
                break;
            case InstanceEffect instEffect:
                ApplyInstanceEffect(instEffect);
                break;
            default:
                Debug.LogWarning($"[StatusEffectSystem] 알 수 없는 타입: {effect.GetType()}");
                break;
        }

        OnEffectsChanged?.Invoke();
    }

    private void ApplyTickEffect(TickEffect tick)
    {
        tickEffects.Add(new TickEffect
        {
            statType = tick.statType,
            value = tick.value,
            duration = tick.duration
        });
    }

    private void ApplyInstanceEffect(InstanceEffect inst)
    {
        // 축복 특수 처리
        if (inst.statType == BuffStatType.Bless)
        {
            ApplyBless(inst.value);
            return;
        }

        // 참회 특수 처리
        if (inst.statType == BuffStatType.Penance)
        {
            ApplyPenance(inst.value);
            return;
        }

        var existing = instantEffects.Find(e => e.statType == inst.statType);

        if (existing != null)
        {
            existing.value = Mathf.Clamp(existing.value + inst.value, 0, 99);
            existing.isMaintain = true;
        }
        else
        {
            instantEffects.Add(new InstanceEffect
            {
                statType = inst.statType,
                value = Mathf.Clamp(inst.value, 0, 99),
                isMaintain = true  // 이번 턴에 추가됨
            });
        }
    }

    /// <summary>
    /// 특정 타입의 효과가 있는지 확인
    /// </summary>
    public bool HasEffect(BuffStatType type)
    {
        return tickEffects.Exists(e => e.statType == type)
            || instantEffects.Exists(e => e.statType == type && e.value > 0);
    }

    /// <summary>
    /// 스턴 상태인지 확인 (TickEffect로 1턴 행동불가)
    /// </summary>
    public bool IsStunned() => tickEffects.Exists(e => e.statType == BuffStatType.Stun);

    /// <summary>
    /// 특정 타입의 수치 반환
    /// </summary>
    public float GetEffectValue(BuffStatType type)
    {
        float total = 0;
        foreach (var e in tickEffects)
            if (e.statType == type) total += e.value;
        foreach (var e in instantEffects)
            if (e.statType == type) total += e.value;
        return total;
    }

    /// <summary>
    /// 특정 타입의 InstantEffect 찾기
    /// </summary>
    public InstanceEffect FindInstantEffect(BuffStatType type)
    {
        return instantEffects.Find(e => e.statType == type);
    }

    /// <summary>
    /// 스탯에 적용된 버프 총합 계산
    /// </summary>
    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        return baseValue + GetEffectValue(statType);
    }

    /// <summary>
    /// 빙결 감소량 반환
    /// </summary>
    public float GetFreezePenalty()
    {
        return GetEffectValue(BuffStatType.Freeze);
    }

    /// <summary>
    /// 상처 보너스 반환 및 소모
    /// </summary>
    public float GetScarBonus()
    {
        var scar = FindInstantEffect(BuffStatType.Scar);
        if (scar == null || scar.value <= 0) return 0;

        float bonus = scar.value;
        instantEffects.Remove(scar);
        return bonus;
    }

    /// <summary>
    /// 수호 효과 확인
    /// </summary>
    public (bool shouldRedirect, float damageReduction) GetGuardEffect()
    {
        var guard = FindInstantEffect(BuffStatType.Guard);
        if (guard == null || guard.value <= 0)
            return (false, 0);

        return (true, guard.value);
    }

    /// <summary>
    /// 수호 소모
    /// </summary>
    public void ConsumeGuard()
    {
        RemoveEffect(BuffStatType.Guard);
    }

    private void TryTriggerStunInternal(InstanceEffect stun)
    {
        float roll = UnityEngine.Random.Range(0f, 100f);
        bool triggered = roll <= stun.value;

        stun.value = 0; // 판정 후 제거

        if (triggered)
        {
            tickEffects.Add(new TickEffect
            {
                statType = BuffStatType.Stun,
                value = 1,
                duration = 1
            });
        }
    }


    private int ConsumeActiveBonus()
    {
        var active = FindInstantEffect(BuffStatType.Activate);
        if (active == null || active.value <= 0) return 0;

        int bonus = (int)active.value;
        instantEffects.Remove(active);
        return bonus;
    }

    private void ApplyBless(float blessValue)
    {
        var targetBuff = instantEffects
            .Where(e => IsBeneficial(e.statType)
                     && e.statType != BuffStatType.Bless
                     && e.statType != BuffStatType.Penance
                     && e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetBuff != null)
        {
            targetBuff.value = Mathf.Min(99, targetBuff.value + blessValue);
        }
        else
        {
            var existing = FindInstantEffect(BuffStatType.Bless);
            if (existing != null)
                existing.value = Mathf.Min(99, existing.value + blessValue);
            else
                instantEffects.Add(new InstanceEffect { statType = BuffStatType.Bless, value = blessValue });
        }
    }

    private void ApplyPenance(float penanceValue)
    {
        var targetDebuff = instantEffects
            .Where(e => IsHarmful(e.statType)
                     && e.statType != BuffStatType.Bless
                     && e.statType != BuffStatType.Penance
                     && e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetDebuff != null)
        {
            targetDebuff.value = Mathf.Max(0, targetDebuff.value - penanceValue);
            if (targetDebuff.value <= 0)
                instantEffects.Remove(targetDebuff);
        }
        else
        {
            var existing = FindInstantEffect(BuffStatType.Penance);
            if (existing != null)
                existing.value = Mathf.Min(99, existing.value + penanceValue);
            else
                instantEffects.Add(new InstanceEffect { statType = BuffStatType.Penance, value = penanceValue });
        }
    }

    public float ConsumePendingBless()
    {
        var bless = FindInstantEffect(BuffStatType.Bless);
        if (bless == null || bless.value <= 0) return 0;
        float value = bless.value;
        instantEffects.Remove(bless);
        return value;
    }

    public float ConsumePendingPenance()
    {
        var penance = FindInstantEffect(BuffStatType.Penance);
        if (penance == null || penance.value <= 0) return 0;
        float value = penance.value;
        instantEffects.Remove(penance);
        return value;
    }

    public void RemoveEffect(BuffStatType type)
    {
        instantEffects.RemoveAll(e => e.statType == type);
        OnEffectsChanged?.Invoke();
    }

    public void TriggerEffectOnce(BuffStatType type)
    {
        for (int i = instantEffects.Count - 1; i >= 0; i--)
        {
            if (instantEffects[i].statType == type && !instantEffects[i].isMaintain)
                instantEffects.RemoveAt(i);
        }
        OnEffectsChanged?.Invoke();
    }

    public void ClearAll()
    {
        tickEffects.Clear();
        instantEffects.Clear();
        OnEffectsChanged?.Invoke();
    }

    public static bool IsBeneficial(BuffStatType type) => type switch
    {
        BuffStatType.Attack or BuffStatType.Defense or BuffStatType.Activate
        or BuffStatType.Bless or BuffStatType.Penance or BuffStatType.Guard => true,
        _ => false
    };

    public static bool IsHarmful(BuffStatType type) => type switch
    {
        BuffStatType.Burn or BuffStatType.Freeze or BuffStatType.Crime
        or BuffStatType.Scar or BuffStatType.Stun => true,
        _ => false
    };

    /// <summary>
    /// 자연(Active) 보너스 가져오고 소모
    /// </summary>
    private int GetActiveBonus()
    {
        var active = FindInstantEffect(BuffStatType.Activate);
        if (active == null || active.value <= 0) return 0;

        int bonus = (int)active.value;
        instantEffects.Remove(active);
        return bonus;
    }

    /// <summary>
    /// 죄악 처리: 턴 종료 시 피해
    /// </summary>
    private float ProcessCrime()
    {
        var crime = FindInstantEffect(BuffStatType.Crime);
        if (crime == null || crime.value <= 0) return 0;

        float damage = crime.value;
        instantEffects.Remove(crime);
        return damage;
    }

    /// <summary>
    /// 기절 발동 시도: 확률 기반
    /// </summary>
    public bool TryTriggerStun()
    {
        var stun = FindInstantEffect(BuffStatType.Stun);
        if (stun == null || stun.value <= 0) return false;

        float roll = UnityEngine.Random.Range(0f, 100f);
        bool triggered = roll <= stun.value;

        // 발동 여부와 관계없이 수치 초기화
        instantEffects.Remove(stun);

        if (triggered)
        {
            // 1턴간 행동 불가 (TickEffect)
            tickEffects.Add(new TickEffect
            {
                statType = BuffStatType.Stun,
                value = 1,
                duration = 1
            });
        }

        return triggered;
    }



    /// <summary>
    /// 가장 높은 이로운 효과 찾기
    /// </summary>
    private InstanceEffect GetHighestBeneficialEffect()
    {
        return instantEffects
            .Where(e => IsBeneficial(e.statType)
                     && e.statType != BuffStatType.Bless
                     && e.statType != BuffStatType.Penance
                     && e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();
    }

    /// <summary>
    /// 가장 높은 해로운 효과 찾기
    /// </summary>
    private InstanceEffect GetHighestHarmfulEffect()
    {
        return instantEffects
            .Where(e => IsHarmful(e.statType)
                     && e.statType != BuffStatType.Bless
                     && e.statType != BuffStatType.Penance
                     && e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();
    }

}