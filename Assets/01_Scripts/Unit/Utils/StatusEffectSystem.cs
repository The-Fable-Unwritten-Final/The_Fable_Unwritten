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

    public event Action OnEffectsChanged;

    public IReadOnlyList<TickEffect> TickEffects => tickEffects;
    public IReadOnlyList<InstanceEffect> InstantEffects => instantEffects;

    /// <summary>
    /// 상태효과 적용
    /// </summary>
    public void ApplyEffect(StatusEffect effect)
    {
        switch (effect)
        {
            case TickEffect tick:
                ApplyTickEffect(tick);
                break;
            case InstanceEffect inst:
                ApplyInstanceEffect(inst);
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
        var existing = instantEffects.Find(e => e.statType == inst.statType);

        if (existing != null)
        {
            existing.value = Mathf.Clamp(existing.value + inst.value, 0, 50);
            existing.isMaintain = existing.isMaintain || inst.isMaintain;
        }
        else
        {
            instantEffects.Add(new InstanceEffect
            {
                statType = inst.statType,
                value = Mathf.Clamp(inst.value, 0, 50),
                isMaintain = inst.isMaintain
            });
        }
    }


    /// <summary>
    /// 특정 타입의 효과가 있는지 확인
    /// </summary>
    public bool HasEffect(BuffStatType type)
    {
        return tickEffects.Exists(e => e.statType == type)
            || instantEffects.Exists(e => e.statType == type);
    }

    /// <summary>
    /// 스턴 상태인지 확인
    /// </summary>
    public bool IsStunned() => HasEffect(BuffStatType.Stun);

    /// <summary>
    /// 스탯에 적용된 버프 총합 계산
    /// </summary>
    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        float result = baseValue;

        foreach (var e in tickEffects)
            if (e.statType == statType)
                result += e.value;

        foreach (var e in instantEffects)
            if (e.statType == statType)
                result += e.value;

        return result;
    }

    /// <summary>
    /// 특정 타입의 버프 총합 반환
    /// </summary>
    public float GetBuffTotal(BuffStatType type)
    {
        float total = 0;

        foreach (var effect in tickEffects)
            if (effect.statType == type)
                total += effect.value;

        foreach (var effect in instantEffects)
            if (effect.statType == type)
                total += effect.value;

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
    /// 턴 종료 시 효과 감소/제거
    /// </summary>
    public void OnTurnEnd()
    {
        // TickEffect 지속시간 감소
        for (int i = tickEffects.Count - 1; i >= 0; i--)
        {
            tickEffects[i].duration--;
            if (tickEffects[i].duration <= 0)
            {
                Debug.Log($"[TickEffect 만료] {tickEffects[i].statType}");
                tickEffects.RemoveAt(i);
            }
        }

        // InstantEffect 유지 해제
        foreach (var effect in instantEffects)
        {
            effect.isMaintain = false;
        }

        OnEffectsChanged?.Invoke();
    }

    /// <summary>
    /// 발동 후 제거되는 효과 처리
    /// </summary>
    public void TriggerEffectOnce(BuffStatType type)
    {
        for (int i = instantEffects.Count - 1; i >= 0; i--)
        {
            if (instantEffects[i].statType == type && !instantEffects[i].isMaintain)
            {
                instantEffects.RemoveAt(i);
            }
        }

        OnEffectsChanged?.Invoke();
    }

    /// <summary>
    /// 특정 효과 수치 초기화
    /// </summary>
    public void ResetEffectValue(BuffStatType type)
    {
        var effect = instantEffects.Find(e => e.statType == type);
        if (effect != null)
        {
            effect.value = 0;
        }
    }

    /// <summary>
    /// 모든 효과 제거
    /// </summary>
    public void ClearAll()
    {
        tickEffects.Clear();
        instantEffects.Clear();
        OnEffectsChanged?.Invoke();
    }

    /// <summary>
    /// 화상 데미지 계산 및 수치 반환
    /// </summary>
    public float GetBurnDamage()
    {
        var burn = instantEffects.Find(e => e.statType == BuffStatType.Burn);
        if (burn != null && burn.value > 0)
        {
            float damage = burn.value;
            if (!burn.isMaintain)
                burn.value = 0;
            return damage;
        }
        return 0;
    }

    /// <summary>
    /// 빙결로 인한 데미지 감소 적용
    /// </summary>
    public float ApplyFreezePenalty(float baseDamage)
    {
        var freeze = instantEffects.Find(e => e.statType == BuffStatType.Freeze);
        if (freeze != null && freeze.value > 0)
        {
            float multiplier = Mathf.Clamp01(1f - freeze.value / 100f);
            float reduced = baseDamage * multiplier;
            freeze.value = 0;
            return reduced;
        }
        return baseDamage;
    }

    /// <summary>
    /// 출혈 보너스 데미지 적용
    /// </summary>
    public float ApplyBleedBonus(float baseDamage)
    {
        var bleed = instantEffects.Find(e => e.statType == BuffStatType.Bleed);
        if (bleed != null && bleed.value > 0)
        {
            baseDamage += bleed.value;
            if (!bleed.isMaintain)
                bleed.value = 0;
        }
        return baseDamage;
    }

    /// <summary>
    /// 활성도 보너스 적용
    /// </summary>
    public void ApplyActivateBonus(BuffStatType incoming)
    {
        var activate = instantEffects.Find(e => e.statType == BuffStatType.Activate);
        if (activate == null || activate.value <= 0) return;

        if (incoming == BuffStatType.Burn || incoming == BuffStatType.Freeze)
        {
            var target = instantEffects.Find(e => e.statType == incoming);
            if (target != null)
            {
                target.value = Mathf.Min(50, target.value + activate.value);
            }
        }
        activate.value = 0;
    }

    /// <summary>
    /// 스턴 발동 시도
    /// </summary>
    public bool TryTriggerStun()
    {
        var stun = instantEffects.Find(e => e.statType == BuffStatType.Stun);
        if (stun != null && stun.value > 0)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll <= stun.value)
            {
                ApplyEffect(new TickEffect
                {
                    statType = BuffStatType.Stun,
                    value = 1,
                    duration = 1
                });
                stun.value = 0;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Grace 수치 반환 및 소모
    /// </summary>
    public float ConsumeGrace()
    {
        var grace = instantEffects.Find(e => e.statType == BuffStatType.Grace);
        if (grace != null && grace.value > 0)
        {
            float value = grace.value;
            grace.value = 0;
            return value;
        }
        return 0;
    }

    /// <summary>
    /// 축복 버프 처리
    /// </summary>
    public void ApplyBless(float blessValue)
    {
        var targetBuff = instantEffects
            .Where(e => !Debuff.IsDebuff(e.statType, e.value)
                     && e.statType != BuffStatType.Bless
                     && e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetBuff != null)
        {
            targetBuff.value = Mathf.Min(50, targetBuff.value + blessValue);
        }
        else
        {
            ApplyEffect(new InstanceEffect
            {
                statType = BuffStatType.Bless,
                value = blessValue,
                isMaintain = false
            });
        }
    }

    /// <summary>
    /// 정화 디버프 처리
    /// </summary>
    public void ApplyPurify(float purifyValue)
    {
        var targetDebuff = instantEffects
            .Where(e => Debuff.IsDebuff(e.statType, e.value)
                     && e.statType != BuffStatType.Purify
                     && e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetDebuff != null)
        {
            float reduction = Mathf.Min(targetDebuff.value, purifyValue);
            targetDebuff.value -= reduction;
        }
        else
        {
            ApplyEffect(new InstanceEffect
            {
                statType = BuffStatType.Purify,
                value = purifyValue,
                isMaintain = false
            });
        }
    }

}