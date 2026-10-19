using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IdealRealizationManager : MonoBehaviour
{
    public static IdealRealizationManager Instance { get; private set; }

    [Header("이상실현 발동 조건")]
    [SerializeField] private int requiredStanceTriggerCount = 3;

    [Header("이상실현 카드 Index")]
    [SerializeField] private int sophiaVisionaryCardIndex = 4001;
    [SerializeField] private int sophiaAlchemistCardIndex = 4002;

    [SerializeField] private int kaylaSacredFlameCardIndex = 4003;
    [SerializeField] private int kaylaJudgeCardIndex = 4004;

    [SerializeField] private int leonLionheartCardIndex = 4005;
    [SerializeField] private int leonShadowWarriorCardIndex = 4006;

    // 캐릭터별 이번 전투 스탠스 효과 발동 횟수
    private readonly Dictionary<CharacterClass, int> stanceTriggerCounts
        = new();

    // 이번 스테이지에서 이미 이상실현을 사용한 캐릭터
    private readonly HashSet<CharacterClass> usedCharactersThisStage
        = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // Reset
    // =========================================================

    public void ResetBattleState()
    {
        stanceTriggerCounts.Clear();
    }

    public void ResetStageState()
    {
        stanceTriggerCounts.Clear();
        usedCharactersThisStage.Clear();
    }

    // =========================================================
    // Stance Effect Trigger
    // =========================================================

    public void OnAnyAllyStanceEffectTriggered(
        PlayerController triggerPlayer,
        BattleFlowController battleFlow)
    {
        if (triggerPlayer == null || battleFlow == null)
            return;

        if (!triggerPlayer.IsAlive())
            return;

        CharacterClass characterClass = triggerPlayer.ChClass;

        if (!stanceTriggerCounts.ContainsKey(characterClass))
        {
            stanceTriggerCounts[characterClass] = 0;
        }

        stanceTriggerCounts[characterClass]++;

        Debug.Log(
            $"[IdealRealization] " +
            $"{triggerPlayer.playerData.CharacterName} " +
            $"스탠스 발동 누적 " +
            $"{stanceTriggerCounts[characterClass]}/" +
            $"{requiredStanceTriggerCount}");

        if (stanceTriggerCounts[characterClass] < requiredStanceTriggerCount)
            return;

        // n회 달성 시점의 현재 스탠스를 기준으로 이상실현 결정
        StancType currentStance =
            triggerPlayer.playerData.currentStance;

        if (!CanGenerateIdealCard(triggerPlayer, currentStance))
            return;

        int idealCardIndex =
            GetIdealCardIndex(triggerPlayer, currentStance);

        if (idealCardIndex < 0)
            return;

        if (CreateIdealCard(
                triggerPlayer,
                idealCardIndex))
        {
            stanceTriggerCounts[characterClass]
                -= requiredStanceTriggerCount;
        }
    }

    // =========================================================
    // Generate Check
    // =========================================================

    private bool CanGenerateIdealCard(
        PlayerController player,
        StancType stance)
    {
        if (player == null || player.Deck == null)
            return false;

        if (!player.IsAlive())
            return false;

        // 이번 스테이지에서 이미 이상실현 사용한 캐릭터
        if (usedCharactersThisStage.Contains(player.ChClass))
        {
            Debug.Log(
                $"[IdealRealization] " +
                $"{player.playerData.CharacterName}은 " +
                $"이번 스테이지에서 이미 이상실현을 사용함");

            return false;
        }

        // 이 캐릭터의 핸드에 이미 이상실현 카드가 있다면 추가 생성 X
        if (HasIdealCardInHand(player))
        {
            Debug.Log(
                $"[IdealRealization] " +
                $"{player.playerData.CharacterName}의 핸드에 " +
                $"이미 이상실현 카드가 존재함");

            return false;
        }

        // 현재 스탠스에 대응하는 이상실현 해금 확인
        if (!IsIdealUnlocked(player, stance))
        {
            Debug.Log(
                $"[IdealRealization] " +
                $"{player.playerData.CharacterName} / {stance} " +
                $"이상실현 미해금");

            return false;
        }

        return true;
    }

    private bool HasIdealCardInHand(PlayerController player)
    {
        if (player == null || player.Deck == null)
            return false;

        return player.Deck.Hand.Any(card =>
            IsIdealCardIndex(card.index));
    }

    private bool IsIdealCardIndex(int cardIndex)
    {
        return
            cardIndex == sophiaVisionaryCardIndex ||
            cardIndex == sophiaAlchemistCardIndex ||
            cardIndex == kaylaSacredFlameCardIndex ||
            cardIndex == kaylaJudgeCardIndex ||
            cardIndex == leonLionheartCardIndex ||
            cardIndex == leonShadowWarriorCardIndex;
    }

    // =========================================================
    // Card Mapping
    // =========================================================

    private int GetIdealCardIndex(
        PlayerController player,
        StancType stance)
    {
        switch (player.ChClass)
        {
            case CharacterClass.Sophia:
                switch (stance)
                {
                    case StancType.Seek:
                        return sophiaVisionaryCardIndex;

                    case StancType.Insight:
                        return sophiaAlchemistCardIndex;
                }
                break;

            case CharacterClass.Kayla:
                switch (stance)
                {
                    case StancType.Mercy:
                        return kaylaSacredFlameCardIndex;

                    case StancType.Discipline:
                        return kaylaJudgeCardIndex;
                }
                break;

            case CharacterClass.Leon:
                switch (stance)
                {
                    case StancType.Defense:
                        return leonLionheartCardIndex;

                    case StancType.Rush:
                        return leonShadowWarriorCardIndex;
                }
                break;
        }

        Debug.LogWarning(
            $"[IdealRealization] " +
            $"{player.ChClass} / {stance}에 대응하는 " +
            $"이상실현 카드가 없음");

        return -1;
    }

    // =========================================================
    // Unlock
    // =========================================================

    private bool IsIdealUnlocked(
        PlayerController player,
        StancType stance)
    {
        // TODO:
        // 추후 IdealRealizationUnlockManager / SaveData와 연결

        // 현재는 모든 이상실현 해금 상태로 테스트
        switch (player.ChClass)
        {
            case CharacterClass.Sophia:
                return stance == StancType.Seek ||
                       stance == StancType.Insight;

            case CharacterClass.Kayla:
                return stance == StancType.Mercy ||
                       stance == StancType.Discipline;

            case CharacterClass.Leon:
                return stance == StancType.Defense ||
                       stance == StancType.Rush;
        }

        return false;
    }

    // =========================================================
    // Card Creation
    // =========================================================

    private bool CreateIdealCard(
        PlayerController player,
        int cardIndex)
    {
        if (player == null || player.Deck == null)
            return false;

        CardModel origin =
            DataManager.Instance.GetCardByIndex(cardIndex);

        if (origin == null)
        {
            Debug.LogWarning(
                $"[IdealRealization] " +
                $"index={cardIndex} 카드 데이터를 찾을 수 없음");

            return false;
        }

        CardModel newCard = origin.Clone();

        player.Deck.AddToHandRightmost(newCard);

        Debug.Log(
            $"[IdealRealization] " +
            $"{player.playerData.CharacterName}의 핸드에 " +
            $"이상실현 카드 {cardIndex} 생성");

        return true;
    }

    // =========================================================
    // Use
    // =========================================================

    public bool CanUseIdeal(PlayerController owner)
    {
        if (owner == null)
            return false;

        if (!owner.IsAlive())
            return false;

        if (usedCharactersThisStage.Contains(owner.ChClass))
            return false;

        return true;
    }

    public bool TryUseIdeal(
        PlayerController owner,
        CardModel idealCard)
    {
        if (owner == null || idealCard == null)
            return false;

        if (!CanUseIdeal(owner))
            return false;

        if (!IsIdealCardIndex(idealCard.index))
            return false;

        bool activated =
            ActivateIdeal(owner, idealCard.index);

        if (!activated)
            return false;

        // 캐릭터는 스테이지 동안 이상실현 1회만 사용 가능
        usedCharactersThisStage.Add(owner.ChClass);

        owner.MarkIdealThisStage();

        Debug.Log(
            $"[IdealRealization] " +
            $"{owner.playerData.CharacterName} " +
            $"이상실현 사용 완료");

        return true;
    }

    // =========================================================
    // Actual Effect
    // =========================================================

    private bool ActivateIdeal(
        PlayerController player,
        int cardIndex)
    {
        if (cardIndex == sophiaVisionaryCardIndex)
        {
            Debug.Log("[IdealRealization] 비전술사 발동");

            // TODO: 비전술사 효과
            return true;
        }

        if (cardIndex == sophiaAlchemistCardIndex)
        {
            Debug.Log("[IdealRealization] 연금술사 발동");

            // TODO: 연금술사 효과
            return true;
        }

        if (cardIndex == kaylaSacredFlameCardIndex)
        {
            Debug.Log("[IdealRealization] 성화 발동");

            // TODO: 성화 효과
            return true;
        }

        if (cardIndex == kaylaJudgeCardIndex)
        {
            Debug.Log("[IdealRealization] 심판자 발동");

            // TODO: 심판자 효과
            return true;
        }

        if (cardIndex == leonLionheartCardIndex)
        {
            Debug.Log("[IdealRealization] 라이온하트 발동");

            // TODO: 라이온하트 효과
            return true;
        }

        if (cardIndex == leonShadowWarriorCardIndex)
        {
            Debug.Log("[IdealRealization] 그림자 전사 발동");

            // TODO: 그림자 전사 효과
            return true;
        }

        return false;
    }
}