using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스탠스 효과 상태 데이터
/// 각 캐릭터가 보유하며, 효과 발동/소모 여부를 추적
/// </summary>
[Serializable]
public class StanceEffectData
{
    public bool IsActive { get; set; } = false;

    // ===== 소피아 =====
    // 탐구(Inquiry): 다음 드로우 카드 1턴간 비용 0
    public bool NextDrawCostZero { get; set; } = false;

    // 통찰(Insight): 다음 카드 효과 2번 적용
    public bool NextCardDoubleEffect { get; set; } = false;

    // ===== 카일라 =====
    // 자비(Compassion): 이번 턴 회복량 +30%
    public float HealBonusPercent { get; set; } = 0f;

    // 규율(Discipline): 다음 공격 시 정화 수치만큼 아군 공방 증가
    public bool NextAttackPurifyBonus { get; set; } = false;

    // ===== 레온 =====
    // 돌진(Rush): 다음 공격 피해 +100%
    public float NextAttackDamageBonus { get; set; } = 0f;

    // 수비(Protection): 체력 +20%, 수호 +15 (1회성)
    public bool GuardBonusApplied { get; set; } = false;

    public void Reset()
    {
        IsActive = false;
        NextDrawCostZero = false;
        NextCardDoubleEffect = false;
        HealBonusPercent = 0f;
        NextAttackPurifyBonus = false;
        NextAttackDamageBonus = 0f;
        GuardBonusApplied = false;
    }
}

/// <summary>
/// 스탠스 효과 처리 유틸리티
/// </summary>
public static class StanceEffectHandler
{
    // 상수 정의
    private const float COMPASSION_HEAL_BONUS = 30f;      // 자비: 회복량 +30%
    private const float RUSH_DAMAGE_BONUS = 100f;         // 돌진: 피해 +100%
    private const float PROTECTION_HP_BONUS = 0.2f;       // 수비: 체력 +20%
    private const float PROTECTION_GUARD_VALUE = 15f;     // 수비: 수호 +15

    /// <summary>
    /// 포텐셜 게이지 만충 시 스탠스 효과 발동
    /// </summary>
    public static void TriggerStanceEffect(PlayerController player, BattleFlowController battleFlow)
    {
        var stance = player.PlayerData.currentStance;
        var charClass = player.ChClass;
        var effectData = player.stanceEffectData;

        effectData.IsActive = true;

        switch (charClass)
        {
            case CharacterClass.Sophia:
                TriggerSophiaEffect(player, stance, effectData);
                break;
            case CharacterClass.Kayla:
                TriggerKaylaEffect(player, stance, effectData);
                break;
            case CharacterClass.Leon:
                TriggerLeonEffect(player, stance, effectData);
                break;
            default:
                Debug.LogWarning($"[StanceEffect] 알 수 없는 캐릭터 클래스: {charClass}");
                break;
        }

        Debug.Log($"[StanceEffect] {player.PlayerData.CharacterName} - {stance} 효과 발동!");
    }


    /// <summary>
    /// 소피아 스탠스 효과
    /// - 탐구(Inquiry): 카드 1장 드로우, 1턴간 비용 0
    /// - 통찰(Insight): 다음 카드 효과 2번 적용
    /// </summary>
    private static void TriggerSophiaEffect(PlayerController player, StancType stance, StanceEffectData effectData)
    {
        switch (stance)
        {
            case StancType.Seek:
                // 탐구: 카드 1장 드로우, 1턴간 비용 0
                effectData.NextDrawCostZero = true;

                // 카드 1장 드로우
                var drawnCards = player.Deck.DrawAndCheck(1);

                // 드로우한 카드에 비용 0 적용
                if (drawnCards != null && drawnCards.Count > 0)
                {
                    foreach (var card in drawnCards)
                    {
                        card.ApplyTemporaryDiscount(card.manaCost); // 1턴간 비용만큼 감소 = 0
                        Debug.Log($"[탐구] {card.cardName} 드로우, 1턴간 비용 0");
                    }
                }
                break;

            case StancType.Insight:
                // 통찰: 다음 카드 효과 2번 적용
                effectData.NextCardDoubleEffect = true;
                Debug.Log("[통찰] 다음 카드 효과 2회 적용 대기");
                break;

            default:
                Debug.LogWarning($"[소피아] 알 수 없는 스탠스: {stance}");
                break;
        }
    }

    /// <summary>
    /// 카일라 스탠스 효과
    /// - 자비(Compassion): 카드 1장 드로우, 이번 턴 회복량 +30%
    /// - 규율(Discipline): 다음 공격 시 정화 수치만큼 아군 공방 증가
    /// </summary>
    private static void TriggerKaylaEffect(PlayerController player, StancType stance, StanceEffectData effectData)
    {
        switch (stance)
        {
            case StancType.Mercy:
                // 자비: 카드 1장 드로우, 이번 턴 회복량 +30%
                player.Deck.Draw(1);
                effectData.HealBonusPercent = COMPASSION_HEAL_BONUS;
                Debug.Log($"[자비] 카드 1장 드로우, 이번 턴 회복량 +{COMPASSION_HEAL_BONUS}%");
                break;

            case StancType.Discipline:
                // 규율: 다음 공격 시 정화 수치만큼 아군 공방 증가
                effectData.NextAttackPurifyBonus = true;
                Debug.Log("[규율] 다음 공격 시 정화 보너스 대기");
                break;

            default:
                Debug.LogWarning($"[카일라] 알 수 없는 스탠스: {stance}");
                break;
        }
    }

    /// <summary>
    /// 레온 스탠스 효과
    /// - 돌진(Rush): 다음 공격 피해 +100%
    /// - 수비(Protection): 체력 +20%, 수호 +15
    /// </summary>
    private static void TriggerLeonEffect(PlayerController player, StancType stance, StanceEffectData effectData)
    {
        switch (stance)
        {
            case StancType.Rush:
                // 돌진: 다음 공격 피해 +100%
                effectData.NextAttackDamageBonus = RUSH_DAMAGE_BONUS;
                Debug.Log($"[돌진] 다음 공격 피해 +{RUSH_DAMAGE_BONUS}%");
                break;

            case StancType.Defense:
                // 수비: 체력 +20%, 수호 +15 (1회성)
                if (!effectData.GuardBonusApplied)
                {
                    // 체력 +20%
                    float hpBonus = player.maxHP * PROTECTION_HP_BONUS;
                    player.maxHP += hpBonus;
                    player.currentHP += hpBonus;

                    // 수호 +15
                    player.ApplyStatusEffect(new InstanceEffect
                    {
                        statType = BuffStatType.Guard,
                        value = PROTECTION_GUARD_VALUE,
                        isMaintain = false
                    });

                    effectData.GuardBonusApplied = true;
                    Debug.Log($"[수비] 체력 +{hpBonus} (20%), 수호 +{PROTECTION_GUARD_VALUE}");
                }
                else
                {
                    Debug.Log("[수비] 이미 적용된 효과 (1회성)");
                }
                break;

            default:
                Debug.LogWarning($"[레온] 알 수 없는 스탠스: {stance}");
                break;
        }
    }


    /// <summary>
    /// 회복량에 스탠스 보너스 적용 (카일라-자비)
    /// </summary>
    public static float ApplyHealBonus(float baseHeal, StanceEffectData effectData)
    {
        if (effectData == null) return baseHeal;

        if (effectData.HealBonusPercent > 0)
        {
            float bonus = baseHeal * (effectData.HealBonusPercent / 100f);
            float result = baseHeal + bonus;
            Debug.Log($"[자비] 회복량 보너스: {baseHeal} + {bonus} = {result}");
            return result;
        }
        return baseHeal;
    }

    /// <summary>
    /// 공격 피해에 스탠스 보너스 적용 (레온-돌진)
    /// </summary>
    public static float ApplyAttackBonus(float baseDamage, StanceEffectData effectData)
    {
        if (effectData == null) return baseDamage;

        if (effectData.NextAttackDamageBonus > 0)
        {
            float bonus = baseDamage * (effectData.NextAttackDamageBonus / 100f);
            float result = baseDamage + bonus;

            // 1회성 효과 소모
            effectData.NextAttackDamageBonus = 0;

            Debug.Log($"[돌진] 공격력 보너스: {baseDamage} + {bonus} = {result}");
            return result;
        }
        return baseDamage;
    }

    /// <summary>
    /// 카드 효과 2배 적용 여부 확인 및 소모 (소피아-통찰)
    /// </summary>
    public static bool ShouldDoubleCardEffect(StanceEffectData effectData)
    {
        if (effectData == null) return false;

        if (effectData.NextCardDoubleEffect)
        {
            effectData.NextCardDoubleEffect = false;
            Debug.Log("[통찰] 카드 효과 2배 적용됨");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 정화 보너스 적용 (카일라-규율)
    /// 정화 수치만큼 아군 전체 공방 증가
    /// </summary>
    public static void ApplyPurifyAttackBonus(PlayerController kayla, List<IStatusReceiver> playerParty)
    {
        if (kayla == null || kayla.stanceEffectData == null) return;

        var effectData = kayla.stanceEffectData;
        if (!effectData.NextAttackPurifyBonus) return;

        // 정화 수치 확인
        var purify = kayla.instantEffects.Find(e => e.statType == BuffStatType.Bless);
        if (purify == null || purify.value <= 0)
        {
            effectData.NextAttackPurifyBonus = false;
            return;
        }

        float bonusValue = purify.value;

        // 아군 전체에게 공격력/방어력 버프 적용
        foreach (var ally in playerParty)
        {
            if (ally is PlayerController pc && pc.IsAlive())
            {
                pc.ApplyStatusEffect(new InstanceEffect
                {
                    statType = BuffStatType.Attack,
                    value = bonusValue,
                    isMaintain = false
                });
                pc.ApplyStatusEffect(new InstanceEffect
                {
                    statType = BuffStatType.Defense,
                    value = bonusValue,
                    isMaintain = false
                });
            }
        }

        Debug.Log($"[규율] 정화 {bonusValue} → 아군 전체 공격력/방어력 +{bonusValue}");
        effectData.NextAttackPurifyBonus = false;
    }

    /// <summary>
    /// 드로우 시 비용 0 적용 여부 확인 (소피아-탐구)
    /// </summary>
    public static bool ShouldApplyCostZeroOnDraw(StanceEffectData effectData)
    {
        if (effectData == null) return false;

        if (effectData.NextDrawCostZero)
        {
            effectData.NextDrawCostZero = false;
            return true;
        }
        return false;
    }


    /// <summary>
    /// 턴 종료 시 효과 초기화
    /// </summary>
    public static void OnTurnEnd(StanceEffectData effectData)
    {
        if (effectData == null) return;

        // 카일라-자비의 회복량 보너스는 턴 종료 시 초기화
        effectData.HealBonusPercent = 0f;

        // 탐구 효과도 턴 종료 시 초기화 (1턴간이므로)
        effectData.NextDrawCostZero = false;
    }

    /// <summary>
    /// 전투 종료 시 모든 효과 초기화
    /// </summary>
    public static void OnBattleEnd(StanceEffectData effectData)
    {
        effectData?.Reset();
    }
}