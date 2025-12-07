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
    // 탐구(refine): 다음 드로우 카드 1턴간 비용 0
    public bool NextDrawCostZero { get; set; } = false;

    // 통찰(mix): 다음 카드 효과 2번 적용
    public bool NextCardDoubleEffect { get; set; } = false;

    // ===== 카일라 =====
    // 자비(grace): 이번 턴 회복량 +30%
    public float HealBonusPercent { get; set; } = 0f;

    // 규율(judge): 다음 공격 시 정화 수치만큼 아군 공방 증가
    public bool NextAttackPurifyBonus { get; set; } = false;

    // ===== 레온 =====
    // 돌진(rush): 다음 공격 피해 +100%
    public float NextAttackDamageBonus { get; set; } = 0f;

    // 수비(guard): 체력 +20%, 수호 +15 (1회성)
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
    /// <summary>
    /// 포텐셜 게이지 만충 시 스탠스 효과 발동
    /// </summary>
    public static void TriggerStanceEffect(PlayerController player, BattleFlowController battleFlow)
    {
        var stance = player.playerData.currentStance;
        var charClass = player.ChClass;
        var effectData = player.stanceEffectData;

        effectData.IsActive = true;

        switch (charClass)
        {
            case CharacterClass.Sophia:
                TriggerSophiaEffect(player, stance, battleFlow);
                break;
            case CharacterClass.Kayla:
                TriggerKaylaEffect(player, stance, battleFlow);
                break;
            case CharacterClass.Leon:
                TriggerLeonEffect(player, stance);
                break;
            default:
                Debug.LogWarning($"[StanceEffect] 알 수 없는 캐릭터 클래스: {charClass}");
                break;
        }

        Debug.Log($"[StanceEffect] {player.playerData.CharacterName} - {stance} 효과 발동!");
    }

    /// <summary>
    /// 소피아 스탠스 효과
    /// - 탐구(refine): 카드 1장 드로우, 1턴간 비용 0
    /// - 통찰(mix): 다음 카드 효과 2번 적용
    /// </summary>
    private static void TriggerSophiaEffect(PlayerController player, PlayerData.StancType stance, BattleFlowController battleFlow)
    {
        var effectData = player.stanceEffectData;

        switch (stance)
        {
            case PlayerData.StancType.refine: // 탐구
                // 드로우 전 핸드 수 기록
                int handCountBefore = player.Deck.Hand.Count;

                // 카드 1장 드로우
                player.Deck.Draw(1);

                // 드로우 성공 확인 (핸드 수 증가 여부)
                if (player.Deck.Hand.Count > handCountBefore)
                {
                    // 드로우한 카드에 1턴간 비용 0 적용
                    var drawnCard = player.Deck.Hand[player.Deck.Hand.Count - 1]; // 마지막으로 드로우한 카드
                    drawnCard.ApplyTemporaryDiscount(drawnCard.manaCost); // 비용만큼 할인 (= 0)
                    Debug.Log($"[Sophia-탐구] {drawnCard.cardName} 드로우, 1턴간 비용 0");
                }
                else
                {
                    Debug.LogWarning("[Sophia-탐구] 드로우 실패 (핸드가 가득 찼거나 덱이 비어있음)");
                }
                break;

            case PlayerData.StancType.mix: // 통찰
                effectData.NextCardDoubleEffect = true;
                Debug.Log("[Sophia-통찰] 다음 카드 효과 2배 활성화");
                break;

            default:
                Debug.LogWarning($"[Sophia] 해당 캐릭터에 맞지 않는 스탠스: {stance}");
                break;
        }
    }

    /// <summary>
    /// 카일라 스탠스 효과
    /// - 자비(grace): 카드 1장 드로우, 이번 턴 회복량 +30%
    /// - 규율(judge): 다음 공격 시 정화 수치만큼 아군 공방 증가
    /// </summary>
    private static void TriggerKaylaEffect(PlayerController player, PlayerData.StancType stance, BattleFlowController battleFlow)
    {
        var effectData = player.stanceEffectData;

        switch (stance)
        {
            case PlayerData.StancType.grace: // 자비
                // 카드 1장 드로우
                player.Deck.Draw(1);
                effectData.HealBonusPercent = 30f;
                Debug.Log("[Kayla-자비] 카드 드로우 + 이번 턴 회복량 +30%");
                break;

            case PlayerData.StancType.judge: // 규율
                effectData.NextAttackPurifyBonus = true;
                Debug.Log("[Kayla-규율] 다음 공격 시 정화 보너스 활성화");
                break;

            default:
                Debug.LogWarning($"[Kayla] 해당 캐릭터에 맞지 않는 스탠스: {stance}");
                break;
        }
    }

    /// <summary>
    /// 레온 스탠스 효과
    /// - 돌진(rush): 다음 공격 피해 +100%
    /// - 수비(guard): 체력 +20%, 수호 +15
    /// </summary>
    private static void TriggerLeonEffect(PlayerController player, PlayerData.StancType stance)
    {
        var effectData = player.stanceEffectData;

        switch (stance)
        {
            case PlayerData.StancType.rush: // 돌진
                effectData.NextAttackDamageBonus = 100f; // +100%
                Debug.Log("[Leon-돌진] 다음 공격 피해 +100% 활성화");
                break;

            case PlayerData.StancType.guard: // 수비
                // 체력 +20%
                float hpBonus = player.maxHP * 0.2f;
                player.maxHP += hpBonus;
                player.currentHP += hpBonus;

                // 수호 +15
                player.ApplyStatusEffect(new InstanceEffect
                {
                    statType = BuffStatType.GuardRedirect,
                    value = 15,
                    isMaintain = false
                });

                effectData.GuardBonusApplied = true;
                Debug.Log($"[Leon-수비] 체력 +{hpBonus}, 수호 +15 적용");
                break;

            default:
                Debug.LogWarning($"[Leon] 해당 캐릭터에 맞지 않는 스탠스: {stance}");
                break;
        }
    }

    // ===== 효과 적용 헬퍼 메서드 =====

    /// <summary>
    /// 회복량에 스탠스 보너스 적용 (카일라-자비)
    /// </summary>
    public static float ApplyHealBonus(float baseHeal, StanceEffectData effectData)
    {
        if (effectData.HealBonusPercent > 0)
        {
            float bonus = baseHeal * (effectData.HealBonusPercent / 100f);
            Debug.Log($"[자비] 회복량 보너스: {baseHeal} + {bonus} = {baseHeal + bonus}");
            return baseHeal + bonus;
        }
        return baseHeal;
    }

    /// <summary>
    /// 공격 피해에 스탠스 보너스 적용 (레온-돌진)
    /// </summary>
    public static float ApplyAttackBonus(float baseDamage, StanceEffectData effectData)
    {
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
        var effectData = kayla.stanceEffectData;
        if (!effectData.NextAttackPurifyBonus) return;

        // 정화 수치 확인
        var purify = kayla.instantEffects.Find(e => e.statType == BuffStatType.Purify);
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
    /// 턴 종료 시 효과 초기화
    /// </summary>
    public static void OnTurnEnd(StanceEffectData effectData)
    {
        // 카일라-자비의 회복량 보너스는 턴 종료 시 초기화
        effectData.HealBonusPercent = 0f;
    }

    /// <summary>
    /// 전투 종료 시 모든 효과 초기화
    /// </summary>
    public static void OnBattleEnd(StanceEffectData effectData)
    {
        effectData.Reset();
    }
}