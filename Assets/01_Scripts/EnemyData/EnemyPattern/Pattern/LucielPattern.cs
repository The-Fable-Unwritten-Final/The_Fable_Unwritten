using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LucielMechanic : EnemyMechanicBase
{
    private const int Phase1Barrier = 12;
    private const int Phase2Barrier = 16;

    private const int BarrierDamageReductionPercent = 30;

    private const int BalanceBarrierReduction = 4;

    private const int BalanceSkillThreshold = 2;

    private const int BalanceDamageReductionPerStack = 2;
    private const int BalanceEffectReductionPerStack = 1;

    private const int DevotionDamageReductionPerStack = 1;
    private const int DevotionPerHarmfulReduction = 2;
    private const int DevotionHealPerStack = 2;
    private const int DevotionBarrierRecoveryPerStack = 1;

    private const int ObsessionDamageIncreasePerStack = 1;
    private const int ObsessionPerHarmfulIncrease = 2;
    private const int ObsessionBarrierReductionPerStack = 3;

    private int devotion;
    private int obsession;
    private int balance;
    private int barrier;

    private bool phase2;
    private bool phase2Pending;

    public override void OnBattleStart()
    {
        devotion = 0;
        obsession = 0;
        balance = 0;

        barrier = Phase1Barrier;

        phase2 = false;
        phase2Pending = false;

        DebugState();
    }

    public override void OnCardUsed(
        PlayerController caster,
        CardModel card,
        IReadOnlyList<IStatusReceiver> targets,
        int activationDiscount)
    {
        if (caster == null || card == null || targets == null)
            return;

        if (caster.ChClass != CharacterClass.Kayla)
            return;

        bool targetEnemy = targets.Any(t => t != null && t.ChClass == CharacterClass.Enemy);
        bool targetPlayer = targets.Any(t => t != null && t.ChClass != CharacterClass.Enemy);

        switch (card.type)
        {
            case CardType.Pray:
            case CardType.baptism:
                if (targetEnemy)
                {
                    devotion++;
                    owner.UpdateStatusUI();
                    Debug.Log($"[Luciel] 헌신 +1 / 현재 {devotion}");
                }
                break;

            case CardType.Taboo:
                if (targetPlayer)
                {
                    obsession++;
                    owner.UpdateStatusUI();
                    Debug.Log($"[Luciel] 집착 +1 / 현재 {obsession}");
                }
                break;
        }
    }

    public override void OnPlayerTurnEnd()
    {
        ResolveEmotion();
        owner.UpdateStatusUI();

        if (!phase2 && !phase2Pending && owner.currentHP <= owner.maxHP * 0.5f)
        {
            phase2Pending = true;
            Debug.Log("[Luciel] 2페이즈 전환 예약");
        }
    }

    private void ResolveEmotion()
    {
        if (devotion <= 0 && obsession <= 0)
        {
            balance = 0;
            return;
        }

        if (devotion == obsession)
        {
            balance = devotion;
            devotion = 0;
            obsession = 0;

            Debug.Log($"[Luciel] 균형 {balance}");
            return;
        }

        if (devotion > obsession)
        {
            devotion -= obsession;
            obsession = 0;
            balance = 0;

            Debug.Log($"[Luciel] 헌신 잔여 {devotion}");
            return;
        }

        obsession -= devotion;
        devotion = 0;
        balance = 0;

        Debug.Log($"[Luciel] 집착 잔여 {obsession}");
    }

    public override void OnEnemyTurnStart()
    {
        if (phase2Pending)
            EnterPhase2();
    }

    public override void OnSkillSelected(EnemySkill skill, EnemyAct actData)
    {
        ApplyEmotionBeforeSkill();

        if (obsession > 0 && ShouldTriggerObsessionDamage(actData))
            DealObsessionDamageToParty();
    }

    private void EnterPhase2()
    {
        phase2Pending = false;
        phase2 = true;

        barrier = Phase2Barrier;

        Debug.Log("[Luciel] 2페이즈 진입 / 신성 장막 16");
    }

    private void ApplyEmotionBeforeSkill()
    {
        if (balance > 0)
        {
            barrier = Mathf.Max(0, barrier - balance * BalanceBarrierReduction);

            Debug.Log($"[Luciel] 균형 {balance} / " + $"장막 -{balance * BalanceBarrierReduction}");

            return;
        }

        if (obsession > 0)
        {
            barrier = Mathf.Max(0, barrier - obsession * ObsessionBarrierReductionPerStack);

            Debug.Log($"[Luciel] 집착 {obsession} / " + $"장막 -{obsession * ObsessionBarrierReductionPerStack}");
        }
    }

    public override void OnSkillResolved(EnemySkill skill, EnemyAct actData)
    {
        if (devotion > 0)
        {
            int healAmount = devotion * DevotionHealPerStack;

            int barrierAmount = devotion * DevotionBarrierRecoveryPerStack;

            owner.Heal(healAmount);
            barrier += barrierAmount;

            Debug.Log($"[Luciel] 헌신 후처리 / " + $"체력 +{healAmount} / " + $"장막 +{barrierAmount}");
        }

        devotion = 0;
        obsession = 0;
        balance = 0;
        owner.UpdateStatusUI();
    }

    public override float ModifyStat(BuffStatType statType, float value)
    {
        return value;
    }

    public override int ModifySkillEffectValue(IStatusReceiver target, EnemyEffectType effectType, EnemyEffectTarget targetType, int value)
    {
        if (target == null)
            return value;

        bool targetPlayer = target.ChClass != CharacterClass.Enemy;

        if (targetPlayer)
        {
            switch (effectType)
            {
                case EnemyEffectType.Damage:
                    return ModifyDamageEffect(value);

                case EnemyEffectType.Burn:
                case EnemyEffectType.Freeze:
                case EnemyEffectType.Scar:
                case EnemyEffectType.Stun:
                    return ModifyHarmfulEffect(value);

                default:
                    return value;
            }
        }

        // 균형 2 이상:
        // Luciel 측에 적용되는 이로운 효과 감소
        if (balance >= BalanceSkillThreshold)
        {
            switch (effectType)
            {
                case EnemyEffectType.Heal:
                case EnemyEffectType.Attack:
                case EnemyEffectType.Defense:
                    return Mathf.Max(0, value - balance * BalanceEffectReductionPerStack);
            }
        }

        return value;
    }

    private int ModifyDamageEffect(int value)
    {
        if (balance >= BalanceSkillThreshold)
        {
            return Mathf.Max(0, value - balance * BalanceDamageReductionPerStack);
        }

        if (devotion > 0)
        {
            return Mathf.Max(0, value - devotion * DevotionDamageReductionPerStack);
        }

        if (obsession > 0)
        {
            return value + obsession * ObsessionDamageIncreasePerStack;
        }

        return value;
    }

    private int ModifyHarmfulEffect(int value)
    {
        if (balance >= BalanceSkillThreshold)
        {
            return Mathf.Max(0, value - balance * BalanceEffectReductionPerStack);
        }

        if (devotion > 0)
        {
            return Mathf.Max(0, value - devotion / DevotionPerHarmfulReduction);
        }

        if (obsession > 0)
        {
            return value + obsession / ObsessionPerHarmfulIncrease;
        }

        return value;
    }


    public float ModifyIncomingAttackDamage(float damage)
    {
        if (barrier > 0)
        {
            float multiplier = 1f - BarrierDamageReductionPercent / 100f;
            damage *= multiplier;
        }

        return damage;
    }

    private void DebugState()
    {
        Debug.Log(
            $"[Luciel] 장막 {barrier} / " +
            $"헌신 {devotion} / 집착 {obsession} / 균형 {balance}"
        );
    }

    private bool IsHealOrBuffEffect(EnemyEffectType effectType)
    {
        return effectType switch
        {
            EnemyEffectType.Heal => true,
            EnemyEffectType.Attack => true,
            EnemyEffectType.Defense => true,
            _ => false
        };
    }

    private bool IsHarmfulToPlayerEffect(EnemyEffectType effectType)
    {
        return effectType switch
        {
            EnemyEffectType.Damage => true,
            EnemyEffectType.Burn => true,
            EnemyEffectType.Freeze => true,
            EnemyEffectType.Scar => true,
            EnemyEffectType.Stun => true,
            _ => false
        };
    }

    private bool IsPlayerTargetEffect(EnemyAct actData, EnemyEffectTarget effectTarget)
    {
        if (effectTarget != EnemyEffectTarget.SkillTarget)
            return false;

        return actData.targetType == TargetType.Ally;
    }

    public override IReadOnlyList<EnemyMechanicDisplayData> GetDisplayStatuses()
    {
        List<EnemyMechanicDisplayData> statuses = new();

        statuses.Add(new EnemyMechanicDisplayData(
            "Agape", 0, true, 30
        ));

        statuses.Add(new EnemyMechanicDisplayData(
            "DivineVeil", barrier, false, 31, BarrierDamageReductionPercent
        ));

        if (devotion > 0)
            statuses.Add(new EnemyMechanicDisplayData("Devotion", devotion, false, 32));

        if (obsession > 0)
            statuses.Add(new EnemyMechanicDisplayData("Obsession", obsession, false, 33));

        if (balance > 0)
            statuses.Add(new EnemyMechanicDisplayData("Balance", balance, false, 34, BalanceSkillThreshold));

        return statuses;
    }

    private bool ShouldTriggerObsessionDamage(EnemyAct actData)
    {
        if (actData == null)
            return false;

        bool hasHealOrBuff =
            (IsEnemySideEffect(actData, actData.arg_target1) && IsHealOrBuffEffect(actData.arg_effect1)) ||
            (IsEnemySideEffect(actData, actData.arg_target2) && IsHealOrBuffEffect(actData.arg_effect2));

        if (!hasHealOrBuff)
            return false;

        bool hasPlayerHarmfulEffect =
            (IsPlayerTargetEffect(actData, actData.arg_target1) && IsHarmfulToPlayerEffect(actData.arg_effect1)) ||
            (IsPlayerTargetEffect(actData, actData.arg_target2) && IsHarmfulToPlayerEffect(actData.arg_effect2));

        return !hasPlayerHarmfulEffect;
    }

    private bool IsEnemySideEffect(EnemyAct actData, EnemyEffectTarget effectTarget)
    {
        switch (effectTarget)
        {
            case EnemyEffectTarget.Self:
            case EnemyEffectTarget.Allies:
            case EnemyEffectTarget.LowestHpAlly:
                return true;

            case EnemyEffectTarget.SkillTarget:
                return actData.targetType != TargetType.Ally;

            default:
                return false;
        }
    }

    private void DealObsessionDamageToParty()
    {
        var players = GameManager.Instance.turnController.battleFlow.playerParty;

        foreach (var target in players)
        {
            if (target == null || !target.IsAlive())
                continue;

            if (target is PlayerController pc && pc.IsTemporarilyAbsent)
                continue;

            target.TakeDamage(obsession);
        }

        Debug.Log($"[Luciel] 집착 특수 효과 / 플레이어 전체 {obsession} 피해");
    }

    public override float ModifySkillWeight(EnemySkill skill, float baseWeight)
    {
        if (!phase2 || skill == null)
            return baseWeight;

        return skill.skillIndex switch
        {
            351 => 25f,
            352 => 20f,
            353 => 15f,
            354 => 20f,
            355 => 20f,
            _ => 0f
        };
    }
}