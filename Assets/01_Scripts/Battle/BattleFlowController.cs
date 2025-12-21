using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 전투 흐름 제어 (Facade)
/// </summary>
public class BattleFlowController : MonoBehaviour
{
    [Header("Character Setup")]
    [SerializeField] public PlayerController frontSlot;
    [SerializeField] public PlayerController middleSlot;
    [SerializeField] public PlayerController backSlot;
    [SerializeField] private List<Enemy> enemyObjects;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] public EffectManager effectManage;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI manaText;
    public TextMeshProUGUI Mana => manaText;

    private TurnManager turnManager;
    private CardExecutor cardExecutor;

    public List<IStatusReceiver> playerParty { get; private set; } = new();
    public List<IStatusReceiver> enemyParty = new List<IStatusReceiver> { null, null, null };

    public Dictionary<CharacterClass, DeckModel> decksByCharacter = new();
    public Dictionary<CharacterClass, IStatusReceiver> characterMap = new();
    public Dictionary<IStatusReceiver, CardModel> enemyPlannedSkill = new();
    public List<int> recentLoots { get; private set; } = new();

    public int totalExp = 0;
    public short isWin;

    public int startMana = 4;
    public int currentMana;

    // TurnManager 위임
    public TurnState currentTurn => turnManager?.CurrentTurn ?? TurnState.PlayerTurn;
    public int turn => turnManager?.TurnCount ?? 1;

    private void Start()
    {
        InitializeSystems();

        totalExp = 0;
        isWin = 0;

        SetupPredefinedPlayerSlots();
        Initialize();
    }

    private void InitializeSystems()
    {
        turnManager = new TurnManager();
        turnManager.OnTurnEnd += OnTurnEnd;

        cardExecutor = new CardExecutor(
            () => currentMana,
            value => { currentMana = value; UpdateManaUI(); },
            () => playerParty
        );
        cardExecutor.OnCardUsed += OnCardUsed;
    }

    private void SetupPredefinedPlayerSlots()
    {
        playerParty.Clear();
        var playerDatas = PlayerManager.Instance.GetAllActivePlayerData();

        foreach (var data in playerDatas)
        {
            PlayerController targetSlot = data.CharacterClass switch
            {
                CharacterClass.Leon => frontSlot,
                CharacterClass.Sophia => middleSlot,
                CharacterClass.Kayla => backSlot,
                _ => null
            };

            if (targetSlot == null)
            {
                Debug.LogWarning($"[BattleFlow] 슬롯이 지정되지 않은 캐릭터입니다: {data.CharacterClass}");
                continue;
            }

            playerParty.Add(targetSlot);
        }
    }

    /// <summary>
    /// 캐릭터 및 적 초기화 (배틀 처음 진입시)
    /// </summary>
    public void Initialize()
    {
        characterMap.Clear();
        decksByCharacter.Clear();
        enemyPlannedSkill.Clear();

        foreach (var player in playerParty)
        {
            characterMap[player.ChClass] = player;
            decksByCharacter[player.ChClass] = player.Deck;
        }

        SoundManager.Instance.PlaySFX(SoundCategory.UI, 3);
    }

    /// <summary>
    /// 배틀 시작
    /// </summary>
    public void StartBattle()
    {
        turnManager.StartBattle();
        currentMana = startMana;

        UpdateManaUI();
        InitializePlayerStances();
        InitializeDecks();
        PlanEnemySkills();
    }

    private void InitializePlayerStances()
    {
        foreach (var player in playerParty)
        {
            if (player is PlayerController pc)
            {
                pc.PlayerData.currentStance = pc.PlayerData.IDNum switch
                {
                    0 => StancType.Inquiry,
                    1 => StancType.Compassion,
                    2 => StancType.Rush,
                    _ => pc.PlayerData.currentStance
                };

                pc.potentialGauge.Reset();
                pc.stanceEffectData.Reset();
            }
        }
    }

    private void InitializeDecks()
    {
        foreach (var player in playerParty)
        {
            if (player.IsAlive())
            {
                player.Deck.ResetDeckState();
                player.Deck.Draw(DeckModel.startSize);
            }
        }
    }

    /// <summary>
    /// 플레이어 턴 실행
    /// </summary>
    public void ExecutePlayerTurn()
    {
        if (turnManager.IsBattleEnded) return;

        turnManager.BeginPlayerTurn();

        if (currentMana < startMana)
            currentMana = startMana;

        UpdateManaUI();
        DrawMissingHands();
    }

    /// <summary>
    /// 플레이어 턴 종료
    /// </summary>
    public void EndPlayerTurn()
    {
        DiscardExcessCards();
        turnManager.EndPlayerTurn();
    }

    private void DrawMissingHands()
    {
        foreach (var player in playerParty)
        {
            if (!player.IsAlive()) continue;

            int toDraw = DeckModel.startSize - player.Deck.Hand.Count;
            if (toDraw > 0)
                player.Deck.Draw(toDraw);
        }
    }

    private void DiscardExcessCards()
    {
        foreach (var player in playerParty)
        {
            if (!player.IsAlive()) continue;

            var hand = player.Deck.Hand;
            if (hand.Count > DeckModel.startSize)
            {
                Debug.LogWarning($"{player.ChClass} 카드가 3장을 초과합니다.");

                List<CardModel> toDiscard = new(hand);
                toDiscard.Reverse();
                player.Deck.DiscardHandToThree(toDiscard);
            }
        }
    }

    /// <summary>
    /// 적 턴 실행
    /// </summary>
    public void ExecuteEnemyTurn(Action onComplete)
    {
        if (turnManager.IsBattleEnded) return;

        StartCoroutine(EnemyTurnCoroutine(() =>
        {
            onComplete?.Invoke();
            AfterEnemyTurn();
        }));
    }

    private IEnumerator EnemyTurnCoroutine(Action onComplete)
    {
        var currentEnemies = new List<IStatusReceiver>(enemyParty);

        foreach (var enemy in currentEnemies)
        {
            if (enemy == null || !enemy.IsAlive()) continue;

            yield return EnemyPattern.ExecutePattern(enemy);
            yield return new WaitForSeconds(0.2f);
        }

        CheckBattleEnd();
        onComplete?.Invoke();
    }

    private void AfterEnemyTurn()
    {
        ProcessTurnEndEffects();
        BattleLogManager.Instance.OnTurnEnd();

        SoundManager.Instance.PlaySFX(SoundCategory.UI, 3);
        turnManager.EndEnemyTurn();
    }

    private void ProcessTurnEndEffects()
    {
        // 플레이어 상태효과 처리
        foreach (var player in playerParty)
        {
            if (player.IsAlive())
            {
                (player as PlayerController)?.TickStatusEffects();
                if (player is PlayerController p)
                {
                    p.PlayerData.ResetCurCard();
                }
            }

            player.Deck.DiscardUnmaintainedCardsAtTurnEnd();
        }

        ClearAllDeckEnhanced();

        // 적 상태효과 처리
        foreach (var enemy in enemyParty)
        {
            if (enemy != null && enemy.IsAlive())
                (enemy as Enemy)?.TickStatusEffects();
        }
    }

    private void OnTurnEnd()
    {
        ExecutePlayerTurn();
    }

    /// <summary>
    /// 카드 사용
    /// </summary>
    public void UseCard(CardModel card, IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            int count = Mathf.Max(1, card.targetCount);
            targets = AutoChooseTargets(card.targetType, card.characterClass, count, targets[0]);
        }

        if (cardExecutor.Execute(card, caster, targets))
        {
            GameManager.Instance.analyticsLogger.LogUseCardInfo(card.index);
        }
    }

    /// <summary>
    /// 카드 사용 시도 (UI에서 호출)
    /// </summary>
    public void TryUseCard(CardModel card, CharacterClass caster, IStatusReceiver target)
    {
        if (!turnManager.IsPlayerTurn()) return;

        if (characterMap.TryGetValue(caster, out var casterController))
        {
            var targets = AutoChooseTargets(card.targetType, card.characterClass, card.targetCount, target);

            if (targets.Count > 0)
            {
                BattleLogManager.Instance.card = card;
                UseCard(card, casterController, targets);
                BattleLogManager.Instance.RegisterCardUse(casterController, card);
                RefreshAllDeckEnhanced();
            }
        }
        else
        {
            Debug.LogWarning($"[BattleFlow] 캐릭터 {caster} 의 정보를 찾을 수 없습니다.");
        }
    }

    public bool CanUseCard(CardModel card, IStatusReceiver caster, IStatusReceiver target, int mana)
    {
        return cardExecutor.CanUse(card, caster, target, mana);
    }

    private void OnCardUsed(CardModel card, IStatusReceiver caster)
    {
        // 포텐셜 게이지 알림
        if (caster is PlayerController cardUser)
        {
            NotifyCardUsedToAllAllies(cardUser);

            // 카일라-규율: 공격 카드 사용 시 정화 보너스
            if (card.targetType == TargetType.Enemy && cardUser.ChClass == CharacterClass.Kayla)
            {
                StanceEffectHandler.ApplyPurifyAttackBonus(cardUser, playerParty);
            }
        }

        UpdateManaUI();
    }

    private void NotifyCardUsedToAllAllies(PlayerController cardUser)
    {
        foreach (var ally in playerParty)
        {
            if (ally is PlayerController pc && pc.IsAlive())
            {
                pc.OnAllyUsedCard(cardUser);
            }
        }
    }


    public void CheckBattleEnd()
    {
        bool allPlayersDead = playerParty.TrueForAll(p => !p.IsAlive());
        bool allEnemiesDead = enemyParty.TrueForAll(p => !p?.IsAlive() ?? true);

        if (allPlayersDead)
        {
            HandleDefeat();
        }
        else if (allEnemiesDead)
        {
            HandleVictory();
        }
    }

    private void HandleDefeat()
    {
        turnManager.EndBattle();
        ClearAllDeckEnhanced();
        ClearAllPlayerCardDiscounts();
        ResetAllPotentialGauges();

        isWin = -1;
        ClearEnemyParty();

        StopAllCoroutines();
        GameManager.Instance.turnController.ToGameEnd();
        BattleLogManager.Instance.ResetGameLog();
    }

    private void HandleVictory()
    {
        turnManager.EndBattle();
        ClearAllDeckEnhanced();
        ClearAllPlayerCardDiscounts();
        ResetAllPotentialGauges();

        BattleLogManager.Instance.ResetBattleLog();
        isWin = 1;

        CollectRewards();
        ClearEnemyParty();

        StopAllCoroutines();
        GameManager.Instance.turnController.ToGameEnd();
    }

    private void CollectRewards()
    {
        foreach (var enemy in enemyParty)
        {
            if (enemy == null || enemy.IsAlive()) continue;

            if (enemy is Enemy enemyComponent)
            {
                var enemyData = enemyComponent.enemyData;
                totalExp += enemyData.exp;

                if (enemyData?.loot != null)
                {
                    foreach (int lootIndex in enemyData.loot)
                    {
                        recentLoots.Add(lootIndex);

                        if (lootIndex >= 0 && lootIndex < ProgressDataManager.MAX_ITEM_COUNT)
                        {
                            ProgressDataManager.Instance.itemCounts[lootIndex]++;
                            ProgressDataManager.Instance.CurrentExp += enemyData.exp;
                        }
                        else
                        {
                            Debug.LogWarning($"[BattleFlow] 잘못된 lootIndex: {lootIndex}");
                        }
                    }
                }
            }
        }
    }

    private void ClearEnemyParty()
    {
        for (int i = 0; i < enemyParty.Count; i++)
            enemyParty[i] = null;
    }

    private void ResetAllPotentialGauges()
    {
        foreach (var player in playerParty)
        {
            if (player is PlayerController pc)
            {
                pc.OnBattleEnd();
            }
        }
    }

    /// <summary>
    /// 전투 포기
    /// </summary>
    public void ForceEndBattle(bool playerGaveUp)
    {
        turnManager.ForceEnd();
        ClearEnemyParty();

        if (playerGaveUp)
        {
            Debug.Log("▶ 플레이어 전투 포기 → 타이틀로 이동");
        }
    }

    /// <summary>
    /// 자동 타겟 설정
    /// </summary>
    public List<IStatusReceiver> AutoChooseTargets(TargetType type, CharacterClass classNum, int targetNum, IStatusReceiver originTarget)
    {
        var pool = type switch
        {
            TargetType.None => playerParty,
            TargetType.Ally => playerParty,
            TargetType.Enemy => enemyParty,
            _ => new List<IStatusReceiver>()
        };

        List<IStatusReceiver> result = new();

        if (type == TargetType.None)
        {
            result = GetSelfTargets(classNum, targetNum, originTarget, pool);
        }
        else
        {
            result = GetMultiTargets(targetNum, originTarget, pool);
        }

        return result;
    }

    private List<IStatusReceiver> GetSelfTargets(CharacterClass classNum, int targetNum, IStatusReceiver originTarget, List<IStatusReceiver> pool)
    {
        List<IStatusReceiver> result = new();
        List<IStatusReceiver> candidates = pool.FindAll(p => p != null && p.IsAlive());

        PlayerController selfSlot = classNum switch
        {
            CharacterClass.Sophia => middleSlot,
            CharacterClass.Kayla => backSlot,
            CharacterClass.Leon => frontSlot,
            _ => null
        };

        switch (targetNum)
        {
            case 0:
                if (selfSlot != null) result.Add(selfSlot);
                break;
            case 3:
                result = candidates;
                break;
            default:
                result.Add(originTarget);
                break;
        }

        return result;
    }

    private List<IStatusReceiver> GetMultiTargets(int targetNum, IStatusReceiver originTarget, List<IStatusReceiver> pool)
    {
        List<IStatusReceiver> result = new();
        List<IStatusReceiver> candidates = pool.FindAll(p => p != null && p.IsAlive() && p != originTarget);

        result.Add(originTarget);

        while (result.Count < targetNum && candidates.Count > 0)
        {
            var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            result.Add(pick);
            candidates.Remove(pick);
        }

        return result;
    }

    private void RefreshAllDeckEnhanced()
    {
        foreach (var player in playerParty)
        {
            if (player is PlayerController pc && pc.IsAlive())
            {
                foreach (var card in pc.Deck.hand)
                    card.UpdateEnhancedState();
                foreach (var card in pc.Deck.unusedDeck)
                    card.UpdateEnhancedState();
                foreach (var card in pc.Deck.usedDeck)
                    card.UpdateEnhancedState();
            }
        }
    }

    private void ClearAllPlayerCardDiscounts()
    {
        foreach (var player in playerParty)
        {
            if (!player.IsAlive()) continue;

            foreach (var card in player.Deck.unusedDeck)
                card.ClearAllDiscount();
            foreach (var card in player.Deck.usedDeck)
                card.ClearAllDiscount();
            foreach (var card in player.Deck.Hand)
                card.ClearAllDiscount();
        }
    }

    private void ClearAllDeckEnhanced()
    {
        foreach (var player in playerParty)
        {
            if (!player.IsAlive()) continue;

            foreach (var card in player.Deck.unusedDeck)
                card.isEnhanced = false;
            foreach (var card in player.Deck.usedDeck)
                card.isEnhanced = false;
            foreach (var card in player.Deck.Hand)
                card.isEnhanced = false;
        }
    }

    private void PlanEnemySkills()
    {
        // 적 스킬 설정
    }


    private void UpdateManaUI()
    {
        if (manaText != null)
            manaText.text = $"{currentMana}";
    }

    public bool IsPlayerTurn() => turnManager?.IsPlayerTurn() ?? false;

    public List<CardModel> GetHand(CharacterClass character)
    {
        if (decksByCharacter.TryGetValue(character, out var deck))
            return new List<CardModel>(deck.Hand);
        return new List<CardModel>();
    }

    public void RequestEndTurn()
    {
        if (!IsPlayerTurn()) return;
        EndPlayerTurn();
    }

    public IStatusReceiver GetCharacter(CharacterClass character)
    {
        characterMap.TryGetValue(character, out var characterObj);
        return characterObj;
    }

    public void ReceivePlayerParty(List<IStatusReceiver> players)
    {
        playerParty = players;
        characterMap.Clear();
        decksByCharacter.Clear();

        foreach (var player in playerParty)
        {
            characterMap[player.ChClass] = player;
            decksByCharacter[player.ChClass] = player.Deck;
        }
    }
}