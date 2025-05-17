using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn } //적 턴인지 아군 턴인지 판별자



public class BattleFlowController : MonoBehaviour
{
    [Header("Character Setup")]
    [SerializeField] public PlayerController frontSlot;
    [SerializeField] public PlayerController middleSlot;
    [SerializeField] public PlayerController backSlot;
    [SerializeField] private List<Enemy> enemyObjects;
    [SerializeField] private GameObject playerPrefab; // 사용하지 않지만 호환성을 위해 유지
    [SerializeField] public EffectManager effectManage;

    [Header("UI")]
    public TextMeshProUGUI Mana;

    public List<IStatusReceiver> playerParty { get; private set; } = new();
    public List<IStatusReceiver> enemyParty { get; private set; } = new();

    public int startMana = 3; //시작 마나
    public int currentMana;   //현재 마나

    public Dictionary<CharacterClass, DeckModel> decksByCharacter = new();     //캐릭터 마다의 사용, 미사용, 핸드 덱
    public Dictionary<CharacterClass, IStatusReceiver> characterMap = new();   //캐릭터클래스에 대한 정보
    public Dictionary<IStatusReceiver, CardModel> enemyPlannedSkill = new();   //적의 스킬 예측 정보
    public List<int> recentLoots { get; private set; } = new();     //현재 전리품 정보
    public int totalExp = 0;                      //현재 층 총 exp 정보

    private bool isBattleEnded = true;      //배틀 끝났는지 확인용

    public short isWin;                 //배틀 결과 확인용    0, 전투 중 1 승리 -1 패배

    public TurnState currentTurn;       //누구 턴인지 확인용
    public int turn = 1;               //지금 몇턴인지 확인용

    //임시 inspector 확인용
    private void Start()
    {
        isWin = 0;
        SetupPredefinedPlayerSlots();
        Initialize();
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

            //targetSlot.Setup(data);
            playerParty.Add(targetSlot);
        }
    }

    /// <summary>
    /// 캐릭터 및 적 초기화(배틀 처음 진입시 시작할 것.)
    /// </summary>
    public void Initialize()
    {
        turn = 1;
        characterMap.Clear();
        decksByCharacter.Clear();
        enemyPlannedSkill.Clear();

        foreach (var player in playerParty)
        {
            characterMap[player.ChClass] = player;
            decksByCharacter[player.ChClass] = player.Deck;
        }
    }

    /// <summary>
    /// 배틀 시작 시 마나 및 덱 세팅
    /// </summary>
    public void StartBattle()
    {
        isBattleEnded = false;
        currentMana = startMana;
        currentTurn = TurnState.PlayerTurn;

        UpdateManaUI();

        foreach (var player in playerParty)
        {
            //──────── K.T.H 변경 ────────
            isBattleEnded = false;
            currentMana = startMana;
            currentTurn = TurnState.PlayerTurn;

            // → 여기에 추가: 모든 캐릭터 자세를 Middle로 초기화
            foreach (var receiver in playerParty)
            {
                if (receiver is PlayerController pc)
                {
                    pc.ChangeStance(PlayerData.StancType.Middle);
                }
            }

            UpdateManaUI();
            //──────── K.T.H 변경 ────────

            if (player.IsAlive())            //모두 덱 초기화 후 3장 뽑기
            {
                if (!player.IsAlive()) continue;
                player.Deck.ResetDeckState();
                player.Deck.Draw(DeckModel.startSize);
            }
        }
        PlanEnemySkills();      //적 스킬 목록 설정
    }

    /// <summary>
    /// 플레이어 턴 행동
    /// </summary>
    public void ExecutePlayerTurn()
    {
        if (isBattleEnded) return;      //전투 종료 명령 확인 시 전투 종료
        currentTurn = TurnState.PlayerTurn;

        if (currentMana < startMana)    //마나가 시작 마나보다 적을 시 시작 마나로 초기화
            currentMana = startMana;

        UpdateManaUI(); // << 추가
        DrawMissingHands();             //각각 패가 3장이 되도록(살아 있을 경우에만) 드로우

        Debug.Log("플레이어 턴 시작");

        // 이후 카드 사용 → 외부에서 UseCard 호출
    }

    /// <summary>
    /// 카드 사용하기
    /// </summary>
    /// <param name="card">사용하는 카드</param>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public void UseCard(CardModel card, IStatusReceiver caster, List<IStatusReceiver> targets)
    {

        if (!card.IsUsable(currentMana) || !caster.IsAlive())      //사용 가능하지 않거나 적 또는 사용자가 죽어 있다면 생략하기
        {
            return;
        }
        

        int actualCost = card.GetEffectiveCost();
        currentMana -= actualCost; // 할인된 코스트 차감

        if (targets == null || targets.Count == 0)
        {
            int count = Mathf.Max(1, card.targetCount);
            targets = AutoChooseTargets(card.targetType, count, targets[0]);
        }

        Debug.Log($"{caster.ChClass} 가 {card.cardName} 사용 → {string.Join(", ", targets.ConvertAll(t => t.ChClass.ToString()))}, cost : {actualCost}");

        if (card.ConsumesDiscountOnce)
        {
            foreach (var player in playerParty)
            {
                player.Deck.ClearAllTemporaryDiscounts();
            }
        }

        card.Play(caster, targets); // 카드 효과 실행
        // 임시 카메라 줌 인 아웃 효과 추가 (이후 캐릭터의 모션이 추가되면, 해당 모션의 시작과 끝에 맞춰 줌 인 아웃 재설정)


        caster.CameraActionPlay(); // 시전 캐릭터 카메라 줌 인 아웃 액션 코루틴

        caster.Deck.Discard(card); // 핸드에서 사용 덱으로

        

        UpdateManaUI();

        // 덱 상태 출력
        if (caster is PlayerController pc)
            pc.PrintDeckState();
    }



    /// <summary>
    /// 플레이어 턴 종료시 메서드. 카드 3장으로 맞춤 처리 (미완)
    /// </summary>
    public void EndPlayerTurn()
    {
        foreach (var player in playerParty)
        {
            /*player.Deck.DiscardHand();  // 손패 전체 버리기

            GameManager.Instance.combatUIController.CardStatusUpdate?.Invoke();*/
            if (player.IsAlive())
            {

                var hand = player.Deck.Hand;
                if (hand.Count > DeckModel.startSize)
                {
                    Debug.LogWarning($"{player.ChClass} 카드가 3장을 초과합니다. 버릴 카드 선택이 필요합니다.");

                    // todo: 외부에서 선택한 카드 전달 필요 Todisacrd
                    // 여기선 임시로 가장 뒤의 카드부터 자동으로 버린다고 가정
                    List<CardModel> toDiscard = new(hand);
                    toDiscard.Reverse();
                    player.Deck.DiscardHandToThree(toDiscard);
                }
            }
        }
        currentTurn = TurnState.EnemyTurn;          //적 턴으로 이행
    }

    /// <summary>
    /// 적 턴 진행
    /// </summary>
    public void ExecuteEnemyTurn(Action onEnemyTurnComplete)
    {
        if (isBattleEnded) return;
        Debug.Log("적 턴 시작");
        StartCoroutine(EnemyTurnCoroutine(() =>
        {
            onEnemyTurnComplete?.Invoke();
            AfterEnemyTurn(); // 적 턴 끝난 후 처리
        }));
    }


    private void AfterEnemyTurn()
    {
        // 1. 플레이어/적 모두 상태효과 지속시간 감소
        foreach (var player in playerParty)
        {
            if (player.IsAlive())
                (player as PlayerController)?.TickStatusEffects();

            player.Deck.DiscardUnmaintainedCardsAtTurnEnd();

        }

        foreach (var enemy in enemyParty)
        {
            if (enemy != null && enemy.IsAlive())
                (enemy as Enemy)?.TickStatusEffects();
        }

        // 2. 턴 수 증가
        turn++;
        Debug.Log($"턴 종료 → 새로운 턴 시작: {turn}턴");

        // 3. 플레이어 턴 시작
        ExecutePlayerTurn();
    }

    private IEnumerator EnemyTurnCoroutine(Action onEnemyTurnComplete)
    {
        var currentEnemies = new List<IStatusReceiver>(enemyParty); // 복사본

        foreach (var enemy in currentEnemies)
        {
            if (enemy == null || !enemy.IsAlive()) continue;

            yield return EnemyPattern.ExecutePattern(enemy);
            yield return new WaitForSeconds(0.5f);
            CheckBattleEnd();
        }

        // 사망자 제거 (원본 리스트 정리)
        for (int i = enemyParty.Count - 1; i >= 0; i--)
        {
            var enemy = enemyParty[i];
            if (enemy == null || !enemy.IsAlive())
                enemyParty.RemoveAt(i);
        }

        onEnemyTurnComplete?.Invoke();
    }


    private void PlanEnemySkills()
    {
        //적 스킬 설정
    }

    private void DrawMissingHands()
    {
        //카드 드로우 사운드 출력
        SoundManager.Instance.PlaySFX(SoundCategory.UI, 3);
        foreach (var player in playerParty)
        {
            if (!player.IsAlive()) continue;
            int toDraw = DeckModel.startSize - player.Deck.Hand.Count;  //손패가 3장 보다 적다면 3장이 될때까지 드로우
            if (toDraw > 0)
                player.Deck.Draw(toDraw);
        }
    }


    public void CheckBattleEnd()
    {
        bool allPlayersDead = playerParty.TrueForAll(p => !p.IsAlive());
        bool allEnemiesDead = enemyParty.TrueForAll(p => !p?.IsAlive() ?? true);

        if (allPlayersDead)
        {
            isBattleEnded = true;
            ClearAllDeckEnhanced();
            ClearAllPlayerCardDiscounts();
            Debug.Log("▶ 전투 패배");
            isWin = -1;
            enemyParty.Clear();
            StopAllCoroutines();
            GameManager.Instance.turnController.ToGameEnd();
        }
        else if (allEnemiesDead)
        {
            isBattleEnded = true;
            ClearAllDeckEnhanced();
            ClearAllPlayerCardDiscounts();
            Debug.Log("▶ 전투 승리");
            isWin = 1;
            foreach(var enemy in enemyParty)
            {
                if(enemy is Enemy enemyComponent)
                {
                    var enemyData = enemyComponent.enemyData;

                    if (enemyData != null && enemyData.loot != null)
                    {
                        foreach (int lootIndex in enemyData.loot)
                        {
                            recentLoots.Add(lootIndex);
                            if (lootIndex >= 0 && lootIndex < ProgressDataManager.MAX_ITEM_COUNT)
                            {
                                ProgressDataManager.Instance.itemCounts[lootIndex]++;
                                
                            }
                            else
                            {
                                Debug.LogWarning($"[BattleFlow] 잘못된 lootIndex: {lootIndex} (범위 초과)");
                            }
                        }
                    }
                }
            }

            enemyParty.Clear();
            StopAllCoroutines();
            GameManager.Instance.turnController.ToGameEnd();
        }
    }

    private void UpdateManaUI()
    {
        if (Mana != null)
            Mana.text = $"{currentMana}";
    }

    /// <summary>
    /// 전투 포기 메서드
    /// </summary>
    /// <param name="playerGaveUp">플레이어가 현재 전투를 포기 하였는지 확인</param>
    public void ForceEndBattle(bool playerGaveUp)
    {
        isBattleEnded = true;
        enemyParty.Clear();
        if (playerGaveUp)
        {
            Debug.Log("▶ 플레이어 전투 포기 → 타이틀로 이동");
            // todo : 타이틀 화면으로
        }
    }

    /// <summary>
    /// 현재 플레이어 턴인지 확인
    /// </summary>
    public bool IsPlayerTurn() => currentTurn == TurnState.PlayerTurn && !isBattleEnded;


    /// <summary>
    /// 특정 캐릭터의 핸드 카드 가져오기
    /// </summary>
    public List<CardModel> GetHand(CharacterClass character)
    {
        if (decksByCharacter.TryGetValue(character, out var deck))
            return new List<CardModel>(deck.Hand);

        return new List<CardModel>();
    }

    /// <summary>
    /// 턴 종료 요청 (UI 버튼에서 연결)
    /// </summary>
    public void RequestEndTurn()
    {
        if (!IsPlayerTurn()) return;
        EndPlayerTurn();
    }

    /// <summary>
    /// 카드 사용 시도 요청 (UI에서 호출)
    /// </summary>
    public void TryUseCard(CardModel card, CharacterClass caster, IStatusReceiver target)
    {
        if (!IsPlayerTurn()) return;

        if (characterMap.TryGetValue(caster, out var casterController))
        {
            List<IStatusReceiver> targets = AutoChooseTargets(card.targetType, card.targetCount, target);

            if (targets.Count > 0)
            {
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

    /// <summary>
    /// 특정 캐릭터 클래스에 해당하는 IStatusReceiver 반환
    /// </summary>
    public IStatusReceiver GetCharacter(CharacterClass character)
    {
        characterMap.TryGetValue(character, out var characterObj);
        return characterObj;
    }

    public bool CanUseCard(CardModel card, IStatusReceiver caster, IStatusReceiver target, int currentMana)
    {
        if (caster == null || target == null || card == null) return false;
        if (!card.IsUsable(currentMana)) return false;
        if (!card.CanBeUsedBy(caster.ChClass)) return false;
        if (!card.IsTargetValid(caster, target)) return false;
        if (caster.IsStunned()) return false; // 스턴 상태면 사용 불가

        foreach (var effect in card.effects)        //버릴 카드 부족하면 사용 불가
        {
            if (effect is DiscardCardEffect discard)
            {
                if (caster.Deck.Hand.Count < discard.discardCount)
                {
                    return false;
                }
            }
        }
        return true;
    }

   //파티 정보를 받아 characterMap 및 deckbyCharacters 초기화
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

    /// <summary>
    /// 자동 타겟 설정
    /// </summary>
    /// <param name="type"></param>
    /// <param name="targetNum"></param>
    /// <returns></returns>
    public List<IStatusReceiver> AutoChooseTargets(TargetType type, int targetNum, IStatusReceiver originTarget)
    {
        var pool = type switch
        {
            TargetType.None => playerParty,
            TargetType.Ally => playerParty,
            TargetType.Enemy => enemyParty,
            _ => new List<IStatusReceiver>()
        };

        List<IStatusReceiver> result = new();

        if (type != TargetType.None)
        {
             result.Add(originTarget);
        }

        List<IStatusReceiver> candidates = pool.FindAll(p => p != null && p.IsAlive() && p != originTarget);


        while (result.Count < targetNum && candidates.Count > 0)
        {
            var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            result.Add(pick);
            candidates.Remove(pick);
        }
        
        if(type == TargetType.None)
        {
            result.Clear(); // 이전 값 제거

            switch (targetNum)
            {
                case 0:
                    result = candidates;
                    break;
                case 1:
                    result.Add(middleSlot); 
                    break;
                case 2:
                    result.Add(backSlot);
                    break;
                case 3:
                    result.Add(frontSlot);
                    break;
                default:
                    result = enemyParty;
                    break;
            }
        }
        return result;
    }

    /// <summary>
    /// 강화 조건 확인
    /// </summary>
    void RefreshAllDeckEnhanced()
    {
        foreach(var player in playerParty)
        {
            if(player is PlayerController pc && pc.IsAlive())
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

    /// <summary>
    /// 할인 제거
    /// </summary>
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
}