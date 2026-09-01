using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LucielMechanic : EnemyMechanicBase
{
    private const int Phase1Barrier = 12;
    private const int Phase2Barrier = 16;

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
                    Debug.Log($"[Luciel] 헌신 +1 / 현재 {devotion}");
                }
                break;

            case CardType.Taboo:
                if (targetPlayer)
                {
                    obsession++;
                    Debug.Log($"[Luciel] 집착 +1 / 현재 {obsession}");
                }
                break;
        }
    }

    public override void OnPlayerTurnEnd()
    {
        ResolveEmotion();

        if (!phase2 &&
            !phase2Pending &&
            owner.currentHP <= owner.maxHP * 0.5f)
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

        ApplyEmotionBeforeSkill();
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
            barrier = Mathf.Max(0, barrier - balance * 4);

            Debug.Log(
                $"[Luciel] 균형 {balance} / 장막 -{balance * 4}"
            );

            return;
        }

        if (obsession > 0)
        {
            barrier = Mathf.Max(0, barrier - obsession * 3);

            Debug.Log(
                $"[Luciel] 집착 {obsession} / 장막 -{obsession * 3}"
            );
        }
    }

    public override void OnEnemyTurnEnd()
    {
        devotion = 0;
        obsession = 0;
        balance = 0;
    }

    public override float ModifyStat(BuffStatType statType, float value)
    {
        return value;
    }

    public override int ModifySkillEffectValue(
    IStatusReceiver target,
    EnemyEffectType effectType,
    EnemyEffectTarget targetType,
    int value)
    {
        if (target == null)
            return value;

        bool targetPlayer = target.ChClass != CharacterClass.Enemy;

        if (!targetPlayer)
            return value;

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

    private int ModifyDamageEffect(int value)
    {
        if (balance >= 2)
            return Mathf.Max(0, value - balance * 2);

        if (devotion > 0)
            return Mathf.Max(0, value - devotion * 2);

        if (obsession > 0)
            return value + obsession * 2;

        return value;
    }

    private int ModifyHarmfulEffect(int value)
    {
        if (balance > 0)
            return Mathf.Max(0, value - balance);

        if (devotion > 0)
            return Mathf.Max(0, value - devotion);

        if (obsession > 0)
            return value + obsession;

        return value;
    }


    public float ModifyIncomingAttackDamage(float damage)
    {
        if (barrier > 0)
            damage *= 0.7f;

        return damage;
    }

    private void DebugState()
    {
        Debug.Log(
            $"[Luciel] 장막 {barrier} / " +
            $"헌신 {devotion} / 집착 {obsession} / 균형 {balance}"
        );
    }
}