using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 전투 중 특수 상호작용(가드 리다이렉트 등)을 처리하는 핸들러
/// </summary>
public class CombatInteractionHandler
{
    private readonly StatusEffectSystem statusEffects;
    private readonly Func<BattleFlowController> getBattleFlow;
    private readonly CharacterClass ownerClass;

    public CombatInteractionHandler(
        StatusEffectSystem statusEffects,
        Func<BattleFlowController> getBattleFlow,
        CharacterClass ownerClass)
    {
        this.statusEffects = statusEffects;
        this.getBattleFlow = getBattleFlow;
        this.ownerClass = ownerClass;
    }

    /// <summary>
    /// 가드 리다이렉트 시도 (레온에게 데미지 전가)
    /// </summary>
    public bool TryGuardRedirect(out PlayerController redirectTarget)
    {
        redirectTarget = null;

        var guard = statusEffects.FindInstantEffect(BuffStatType.GuardRedirect);
        if (guard == null || guard.value <= 0)
            return false;

        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > guard.value)
            return false;

        var battleFlow = getBattleFlow?.Invoke();
        if (battleFlow == null)
            return false;

        var leon = battleFlow.playerParty
            .OfType<PlayerController>()
            .FirstOrDefault(p => p.ChClass == CharacterClass.Leon);

        if (leon != null)
        {
            Debug.Log($"[Guard] 가드 리다이렉트 발동! 레온이 대신 공격받음");
            redirectTarget = leon;
            guard.value = 0;
            return true;
        }

        return false;
    }
}