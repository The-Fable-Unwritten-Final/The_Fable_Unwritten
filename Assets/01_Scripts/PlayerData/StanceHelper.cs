using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StanceHelper
{
    public static (float result, bool boosted, bool weakend) ApplyStanceToDamage(PlayerController caster, float baseDamage, CardType cardType)
    {
        bool boosted = false;
        bool weakened = false;
        float result = baseDamage;

        var logManager = BattleLogManager.Instance;
        var currentTypes = logManager.GetCurrentTurnCardTypes();
        var firstCardType = logManager.GetFirstCardTypeInCurrentTurn();

        switch (caster.playerData.currentStance)
        {
            case StancType.Insight:
                if (!firstCardType.HasValue || firstCardType.Value == cardType)
                {
                    result *= 1.5f;
                    boosted = true;
                }
                else
                {
                    result *= 0.5f;
                    weakened = true;
                }
                break;

            case StancType.Seek:
                if (!firstCardType.HasValue || firstCardType.Value != cardType)
                {
                    result *= 1.5f;
                    boosted = true;
                }
                else
                {
                    result *= 0.5f;
                    weakened = true;
                }
                break;
            case StancType.Discipline:
            case StancType.Rush:
                result *= 1.5f;
                boosted = true;
                break;
            case StancType.Mercy:
            case StancType.Defense:
                result *= 0.5f;
                weakened = true;
                break;
        }

        return (result, boosted, weakened);
    }

    public static (float result, bool boosted, bool weakened) ApplyStanceToHeal(PlayerController caster, float baseHeal)
    {
        bool boosted = false;
        bool weakened = false;
        float result = baseHeal;

        switch (caster.playerData.currentStance)
        {
            case StancType.Discipline:
                result *= 1.5f;
                boosted = true;
                break;

            case StancType.Mercy:
                result *= 0.5f;
                weakened = true;
                break;
        }

        return (result, boosted, weakened);
    }

    public static int ModifyBuffByStance(PlayerController caster, BuffStatType type, int value)
    {
        switch (caster.playerData.currentStance)
        {
            case StancType.Mercy:
                if (value > 0 && (type == BuffStatType.Attack || type == BuffStatType.Defense))
                    return value + 1;
                if (value < 0)
                    return value + 1; // 예: -2 → -1
                break;

            case StancType.Discipline:
                // judge: 디버프 강화 (-1 → -2), 버프 약화 (+1 → 0)
                if (value > 0 && (type == BuffStatType.Attack || type == BuffStatType.Defense))
                    return Mathf.Max(value - 1, 0);
                if (value < 0)
                    return value - 1; // 예: -1 → -2
                break;
        }

        return value;
    }
}