using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IzkalMechanic : EnemyMechanicBase
{
    private const int Phase1MaxRune = 10;
    private const int Phase2MaxRune = 12;
    private const int StormCountdownStart = 2;
    private bool grantNatureRewardNextPlayerTurn;

    private const int LifeOathSkillIndex = 255;
    private bool forceLifeOath;

    private int unreadRune;
    private int stormCountdown;

    private bool phase2;
    private bool usedStormThisTurn;
    private bool phase2Pending;

    private bool skipNormalAction;
    public override bool SkipNormalActionThisTurn => skipNormalAction;

    private readonly List<CardType> currentRunes = new();

    // 2페이즈용
    private int fireTrace;
    private int iceTrace;
    private int natureTrace;
    private CardType lastTraceType;



    public override void OnBattleStart()
    {
        phase2 = false;
        phase2Pending = false;
        usedStormThisTurn = false;
        grantNatureRewardNextPlayerTurn = false;
        forceLifeOath = false;

        unreadRune = Phase1MaxRune;
        stormCountdown = StormCountdownStart;

        fireTrace = 0;
        iceTrace = 0;
        natureTrace = 0;


        currentRunes.Clear();
        AddRandomRune();

        DebugState();
    }

    public override void OnPlayerTurnStart()
    {
        if (!grantNatureRewardNextPlayerTurn)
            return;

        grantNatureRewardNextPlayerTurn = false;

        var sophia =
            battleFlow.playerParty
                .OfType<PlayerController>()
                .FirstOrDefault(
                    p =>
                        p.IsAlive() &&
                        p.ChClass ==
                        CharacterClass.Sophia
                );

        if (sophia == null)
            return;

        sophia.ApplyStatusEffect(
            new InstanceEffect
            {
                statType = BuffStatType.Activate,
                value = 10,
                isMaintain = false
            }
        );

        sophia.Deck.Draw(1);

        Debug.Log(
            "[Izkal] 자연 판독 흔적 보상 / 활성 10 + 카드 1장"
        );
    }

    public override void OnPlayerTurnEnd()
    {
        // 체력 50% 이하 최초 진입 확인
        if (!phase2 &&
            !phase2Pending &&
            owner.currentHP <= owner.maxHP * 0.5f)
        {
            phase2Pending = true;

            Debug.Log("[Izkal] 2페이즈 전환 예약");
        }
    }

    public override void OnEnemyTurnStart()
    {
        usedStormThisTurn = false;
        skipNormalAction = false;

        // 50% 이하 진입 후 다음 적 턴 시작
        if (phase2Pending)
        {
            EnterPhase2();
            return;
        }

        if (stormCountdown <= 0)
        {
            UseRuneStorm();
            skipNormalAction = true;
        }
    }

    public override void OnEnemyTurnEnd()
    {
        // 룬 폭풍 사용 턴에는 예고 감소하지 않음
        if (usedStormThisTurn)
            return;

        stormCountdown = Mathf.Max(0, stormCountdown - 1);

        Debug.Log($"[Izkal] 룬 폭풍 예고 {stormCountdown}");
    }

    private void EnterPhase2()
    {
        lastTraceType = default;
        phase2Pending = false;
        phase2 = true;

        unreadRune = Phase2MaxRune;
        stormCountdown = StormCountdownStart;

        fireTrace = 0;
        iceTrace = 0;
        natureTrace = 0;

        currentRunes.Clear();

        AddRandomRune();
        AddDifferentRune();

        forceLifeOath = true;

        Debug.Log("[Izkal] 2페이즈 진입");
        DebugState();

        // TODO:
        // 이 턴에는 '생명의 서약' 확정 사용
        // 다른 일반 스킬 사용 금지
    }

    private void AddRandomRune()
    {
        CardType rune = GetRandomRune();

        currentRunes.Add(rune);
    }

    private void AddDifferentRune()
    {
        CardType rune;

        do
        {
            rune = GetRandomRune();
        }
        while (currentRunes.Contains(rune));

        currentRunes.Add(rune);
    }

    private CardType GetRandomRune()
    {
        int value = Random.Range(0, 3);

        return value switch
        {
            0 => CardType.Fire,
            1 => CardType.Ice,
            _ => CardType.Nature
        };
    }

    public override void OnCardUsed(PlayerController caster, CardModel card, IReadOnlyList<IStatusReceiver> targets, int activationDiscount)
    {
        if (caster == null || card == null)
            return;

        if (caster.ChClass != CharacterClass.Sophia)
            return;

        if (!targets.Contains(owner))
            return;

        if (card.type != CardType.Fire &&
            card.type != CardType.Ice &&
            card.type != CardType.Nature)
            return;

        int runeIndex = currentRunes.IndexOf(card.type);

        if (runeIndex < 0)
            return;

        ResolveRuneReading(card.type, runeIndex, activationDiscount);
    }

    private void ResolveRuneReading(CardType type, int runeIndex, int activationDiscount)
    {
        int activationDecode = Mathf.Min(2, activationDiscount);

        int totalDecode = 2 + activationDecode;

        int before = unreadRune;

        unreadRune = Mathf.Max(0, unreadRune - totalDecode);

        if (phase2)
            AddTrace(type);

        ReplaceRune(runeIndex);

        Debug.Log(
            $"[Izkal] {type} 판독 성공 / " +
            $"기본 2 + 활성 {activationDecode} / " +
            $"미해독 룬 {before} → {unreadRune}"
        );

        DebugState();
    }

    private void ReplaceRune(int index)
    {
        if (!phase2)
        {
            currentRunes[index] = GetRandomRune();
            return;
        }

        int otherIndex = index == 0 ? 1 : 0;
        CardType otherRune = currentRunes[otherIndex];

        CardType next;

        do
        {
            next = GetRandomRune();
        }
        while (next == otherRune);

        currentRunes[index] = next;
    }

    private void AddTrace(CardType type)
    {
        switch (type)
        {
            case CardType.Fire:
                fireTrace++;
                break;

            case CardType.Ice:
                iceTrace++;
                break;

            case CardType.Nature:
                natureTrace++;
                break;
        }

        lastTraceType = type;

        Debug.Log(
            $"[Izkal] 판독 흔적 " +
            $"화염 {fireTrace} / 얼음 {iceTrace} / 자연 {natureTrace}"
        );
    }

    private void UseRuneStorm()
    {
        usedStormThisTurn = true;

        if (unreadRune <= 0)
        {
            UseReadStorm();
        }
        else
        {
            UseNormalStorm();
        }

        ResetRuneCycle();
    }

    private void UseReadStorm()
    {
        foreach (var player in battleFlow.playerParty)
        {
            if (player is not PlayerController pc ||
                !pc.CanParticipateInBattle())
                continue;

            pc.TakeDamage(5f);
        }

        Debug.Log("[Izkal] 읽힌 폭풍 / 전체 피해 5");
    }

    private void UseNormalStorm()
    {
        int remainingRune = unreadRune;

        float freeze = owner.GetEffectValue(BuffStatType.Freeze);

        // 실제 피해
        float baseDamage = (phase2 ? 9f : 8f) + remainingRune;

        float damage = Mathf.Max(0f, baseDamage - freeze);

        // 상태이상 판정용 룬
        int freezeRuneReduce =
            Mathf.Min(
                2,
                Mathf.FloorToInt(freeze / 3f)
            );

        int effectiveRune =
            Mathf.Max(
                0,
                remainingRune - freezeRuneReduce
            );

        CardType? dominantTrace = null;

        if (phase2)
            dominantTrace = GetDominantTrace();

        // 2페이즈 화염 흔적
        if (dominantTrace == CardType.Fire)
        {
            damage =
                Mathf.Max(0f, damage - 4f);
        }

        foreach (var player in battleFlow.playerParty)
        {
            if (player is not PlayerController pc ||
                !pc.CanParticipateInBattle())
                continue;

            pc.TakeDamage(damage);

            ApplyStormStatus(
                pc,
                effectiveRune,
                dominantTrace
            );
        }

        // 자연 흔적 보상 예약
        if (phase2 &&
            dominantTrace == CardType.Nature)
        {
            grantNatureRewardNextPlayerTurn = true;
        }

        Debug.Log(
            $"[Izkal] 룬 폭풍 / " +
            $"기본 룬 {remainingRune} / " +
            $"빙결 {freeze} / " +
            $"상태 판정 룬 {effectiveRune} / " +
            $"최종 피해 {damage}"
        );
    }

    private CardType? GetDominantTrace()
    {
        int total =
            fireTrace +
            iceTrace +
            natureTrace;

        if (total < 3)
            return null;

        int max =
            Mathf.Max(
                fireTrace,
                Mathf.Max(
                    iceTrace,
                    natureTrace
                )
            );

        List<CardType> tied = new();

        if (fireTrace == max)
            tied.Add(CardType.Fire);

        if (iceTrace == max)
            tied.Add(CardType.Ice);

        if (natureTrace == max)
            tied.Add(CardType.Nature);

        if (tied.Count == 1)
            return tied[0];

        // 동률이면 마지막 판독 타입 우선
        if (tied.Contains(lastTraceType))
            return lastTraceType;

        return tied[0];
    }

    private void ApplyStormStatus(
    PlayerController target,
    int remainingRune,
    CardType? dominantTrace)
    {
        int stunThreshold =
            phase2 ? 4 : 5;

        int scarThreshold =
            phase2 ? 7 : 8;

        bool applyBurn =
            remainingRune >= 1;

        bool applyStun =
            remainingRune >= stunThreshold;

        bool applyScar =
            remainingRune >= scarThreshold;

        // 얼음 흔적:
        // 가장 위험한 상태 딱 하나 제거
        if (phase2 &&
            dominantTrace == CardType.Ice)
        {
            if (applyScar)
                applyScar = false;
            else if (applyStun)
                applyStun = false;
            else if (applyBurn)
                applyBurn = false;
        }

        if (applyBurn)
        {
            target.ApplyStatusEffect(
                new InstanceEffect
                {
                    statType = BuffStatType.Burn,
                    value = 2,
                    isMaintain = false
                }
            );
        }

        if (applyStun)
        {
            target.ApplyStatusEffect(
                new InstanceEffect
                {
                    statType = BuffStatType.Stun,
                    value = 3,
                    isMaintain = false
                }
            );
        }

        if (applyScar)
        {
            target.ApplyStatusEffect(
                new InstanceEffect
                {
                    statType = BuffStatType.Scar,
                    value = 10,
                    isMaintain = false
                }
            );
        }
    }

    private void ResetRuneCycle()
    {
        lastTraceType = default;
        unreadRune =
            phase2
                ? Phase2MaxRune
                : Phase1MaxRune;

        stormCountdown = StormCountdownStart;

        currentRunes.Clear();

        if (phase2)
        {
            AddRandomRune();
            AddDifferentRune();

            fireTrace = 0;
            iceTrace = 0;
            natureTrace = 0;
        }
        else
        {
            AddRandomRune();
        }

        DebugState();
    }

    public override void OnBurnDamageTaken(float damage)
    {
        int reduce =
            Mathf.Min(
                2,
                Mathf.FloorToInt(damage / 5f)
            );

        if (reduce <= 0)
            return;

        int before = unreadRune;

        unreadRune =
            Mathf.Max(0, unreadRune - reduce);

        Debug.Log(
            $"[Izkal] 화상 해독 / 피해 {damage} / " +
            $"미해독 룬 {before} → {unreadRune}"
        );
    }


    public override EnemySkill GetForcedSkill()
    {
        if (!forceLifeOath)
            return null;

        forceLifeOath = false;

        EnemySkill skill =
            owner.enemyData.SkillList
                .FirstOrDefault(
                    s => s.skillIndex == LifeOathSkillIndex
                );

        if (skill == null)
        {
            Debug.LogWarning(
                "[Izkal] 생명의 서약(255)을 SkillList에서 찾을 수 없습니다."
            );
        }

        return skill;
    }

    private void DebugState()
    {
        Debug.Log(
            $"[Izkal] 미해독 룬 {unreadRune} / " +
            $"예고 {stormCountdown} / " +
            $"현재 룬: {string.Join(", ", currentRunes)}"
        );
    }
}