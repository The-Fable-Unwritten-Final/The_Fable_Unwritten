using System;
using UnityEngine;

[Serializable]
public class StanceEffectData
{
    public bool IsActive { get; set; } = false;

    // ===== 소피아 =====
    public bool NextDrawCostZero { get; set; } = false;
    public bool NextCardDoubleEffect { get; set; } = false;

    // ===== 카일라 =====
    public float HealBonusPercent { get; set; } = 0f;
    public bool NextAttackSinBuffBonus { get; set; } = false;

    // ===== 레온 =====
    public float NextAttackDamageBonus { get; set; } = 0f;
    public bool GuardBonusApplied { get; set; } = false;

    public void Reset()
    {
        IsActive = false;
        NextDrawCostZero = false;
        NextCardDoubleEffect = false;
        HealBonusPercent = 0f;
        NextAttackSinBuffBonus = false;
        NextAttackDamageBonus = 0f;
        GuardBonusApplied = false;
    }
}

public static class StanceEffectHandler
{
    private const float MERCY_HEAL_BONUS = 30f;
    private const float RUSH_DAMAGE_BONUS = 100f;
    private const float DEFENSE_HP_BONUS = 0.2f;
    private const float DEFENSE_GUARD_VALUE = 15f;

    public static void TriggerStanceEffect(PlayerController player, BattleFlowController battleFlow)
    {
        var stance = player.playerData.currentStance;
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
                Debug.LogWarning($"[StanceEffectHandler] 알 수 없는 캐릭터 클래스: {charClass}");
                break;
        }

        IdealRealizationManager.Instance?.OnAnyAllyStanceEffectTriggered(player, battleFlow);

        Debug.Log($"[StanceEffect] {player.playerData.CharacterName} - {stance} 효과 발동");
    }

    private static void TriggerSophiaEffect(PlayerController player, StancType stance, StanceEffectData effectData)
    {
        switch (stance)
        {
            case StancType.Seek:
                {
                    effectData.NextDrawCostZero = true;

                    var drawnCards = player.Deck.DrawAndReturn(1);
                    if (drawnCards != null)
                    {
                        foreach (var card in drawnCards)
                        {
                            card.ApplyTemporaryDiscount(card.manaCost);
                        }
                    }
                    break;
                }

            case StancType.Insight:
                effectData.NextCardDoubleEffect = true;
                break;
        }
    }

    private static void TriggerKaylaEffect(PlayerController player, StancType stance, StanceEffectData effectData)
    {
        switch (stance)
        {
            case StancType.Mercy:
                player.Deck.Draw(1);
                effectData.HealBonusPercent = MERCY_HEAL_BONUS;
                break;

            case StancType.Discipline:
                effectData.NextAttackSinBuffBonus = true;
                break;
        }
    }

    private static void TriggerLeonEffect(PlayerController player, StancType stance, StanceEffectData effectData)
    {
        switch (stance)
        {
            case StancType.Rush:
                effectData.NextAttackDamageBonus = RUSH_DAMAGE_BONUS;
                break;

            case StancType.Defense:
                if (!effectData.GuardBonusApplied)
                {
                    float hpBonus = player.maxHP * DEFENSE_HP_BONUS;
                    player.maxHP += hpBonus;
                    player.currentHP += hpBonus;

                    player.ApplyStatusEffect(new InstanceEffect
                    {
                        statType = BuffStatType.Guard,
                        value = DEFENSE_GUARD_VALUE,
                        isMaintain = false
                    });

                    effectData.GuardBonusApplied = true;
                }
                break;
        }
    }

    public static float ApplyHealBonus(float baseHeal, StanceEffectData effectData)
    {
        if (effectData == null) return baseHeal;

        if (effectData.HealBonusPercent > 0)
            return baseHeal * (1f + effectData.HealBonusPercent / 100f);

        return baseHeal;
    }

    public static float ApplyAttackBonus(float baseDamage, StanceEffectData effectData)
    {
        if (effectData == null) return baseDamage;

        if (effectData.NextAttackDamageBonus > 0)
        {
            float result = baseDamage * (1f + effectData.NextAttackDamageBonus / 100f);
            effectData.NextAttackDamageBonus = 0f;
            return result;
        }

        return baseDamage;
    }

    public static bool ShouldDoubleCardEffect(StanceEffectData effectData)
    {
        if (effectData == null) return false;

        if (effectData.NextCardDoubleEffect)
        {
            effectData.NextCardDoubleEffect = false;
            return true;
        }

        return false;
    }

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

    public static void OnTurnEnd(StanceEffectData effectData)
    {
        if (effectData == null) return;

        effectData.HealBonusPercent = 0f;
        effectData.NextDrawCostZero = false;
    }

    public static void OnBattleEnd(StanceEffectData effectData)
    {
        effectData?.Reset();
    }
}