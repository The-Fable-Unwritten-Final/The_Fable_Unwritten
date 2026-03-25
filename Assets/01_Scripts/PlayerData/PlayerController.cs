using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ReduceNextCardCostEffect;
using System;

public class PlayerController : MonoBehaviour, IStatusReceiver
{
    public PlayerData playerData;       //플레이어의 데이타
    public DeckModel deckModel;         //플레이어가 들고 있는 덱
    public bool hasBlock = false;           //방어막 획득 여부

    [SerializeField] private PotentialBarUI potentialBarUI;
    public bool hasResist { get; set; } = false;          //상태이상 디버프 저항 여부
    private bool isTargetable;              //타겟 가능 여부

    // 스테이지 한 번만 이상실현 사용
    public bool IsIdealUsed { get; private set; }

    // 스테이지 시작 시점(BattleFlowController.StartBattle 등)에서 호출
    public void ResetIdealForStage() => IsIdealUsed = false;
    public void MarkIdealThisStage() => IsIdealUsed = true;

    private bool skipTurnThisRound = false;

    // Burn 관련
    private bool burnIncreasedDuringOpponentTurn = false;

    // Guard 관련 (레온)
    private bool guardRedirectPending = false;

    private bool IsOpponentActingTurn()
    {
        var flow = GameManager.Instance?.turnController?.battleFlow;
        if (flow == null) return false;

        // 플레이어 입장에서는 EnemyTurn이 상대 턴
        return flow.currentTurn == TurnState.EnemyTurn;
    }

    public void OnBattleStart()
    {
        skipTurnThisRound = false;
        burnIncreasedDuringOpponentTurn = false;
        guardRedirectPending = false;

        hasBlock = false;
        hasResist = false;

        tickEffects.Clear();
        instantEffects.Clear();

        ResetStanceBattleState();
        ResetIdealForStage();

        if (stanceSystem == null)
            InitializeStanceSystem();
        else
            stanceSystem.ResetGauge();

        potentialBarUI?.Bind(this);
        potentialBarUI?.Refresh();
    }

    public void OnTurnStart()
    {
        skipTurnThisRound = false;

        ApplyBurnOnTurnStart();

        if (TryTriggerStun())
            skipTurnThisRound = true;

        statusDisplay?.PlayerUpdateUI();
    }

    public void OnTurnEnd()
    {
        ApplyCrimeOnTurnEnd();
        TickStatusEffects();
        ClearTurnEndInstantEffects();
        ClearBlock();

        // Freeze는 턴 종료 시 초기화
        ClearInstantEffect(BuffStatType.Freeze);

        playerData.ResetCurCard();
        statusDisplay?.PlayerUpdateUI();
    }

    private void ClearInstantEffect(BuffStatType type)
    {
        var effect = instantEffects.Find(e => e.statType == type);
        if (effect != null)
            effect.value = 0;
    }

    private void ClearTurnEndInstantEffects()
    {
        for (int i = instantEffects.Count - 1; i >= 0; i--)
        {
            var e = instantEffects[i];
            if (e.value <= 0)
                instantEffects.RemoveAt(i);
        }
    }


    /// <summary>
    /// 이상실현 사용 시 이후 처리를 위해 필요한 bool
    /// </summary>
    public bool justTriggeredStanceThisTurn { get; set; }
    public bool IsTargetable
    {
        get => isTargetable;
        set
        {
            if (isTargetable != value)
            {
                isTargetable = value;
                OnTargetableChanged?.Invoke();
            }
        }
    }

    //스탠스 시스템
    private StanceSystem stanceSystem;
    public StanceEffectData stanceEffectData = new();

    public StanceSystem StanceSystem => stanceSystem;

    private void InitializeStanceSystem()
    {
        stanceSystem?.Dispose();
        stanceSystem = new StanceSystem(this, playerData);
        stanceSystem.OnStanceChanged += HandleStanceChanged;
        stanceSystem.OnStanceEffectTriggered += HandleStanceEffectTriggered;

        potentialBarUI?.Bind(this);
        potentialBarUI?.Refresh();
    }

    private void HandleStanceChanged(StancType newStance)
    {
        Debug.Log($"[{playerData.CharacterName}] Stance Changed -> {newStance}");
    }

    private void HandleStanceEffectTriggered()
    {
        if (GameManager.Instance?.turnController?.battleFlow == null)
            return;

        StanceEffectHandler.TriggerStanceEffect(this, GameManager.Instance.turnController.battleFlow);
    }



    public event System.Action OnTargetableChanged; // 타겟 가능 여부 변경 이벤트

    [SerializeField] private HpBarDisplay hpBarDisplay;
    [SerializeField] private DmgBarDisplay dmgBarDisplay;
    [SerializeField] private TargetArrowDisplay targetArrow; 

    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public float GetEffectValue(BuffStatType type)
    {
        float total = 0;
        foreach (var e in tickEffects)
            if (e.statType == type) total += e.value;
        foreach (var e in instantEffects)
            if (e.statType == type) total += e.value;
        return total;
    }




    public DeckModel Deck => deckModel;     //덱 변환 함수
    public bool IsIgnited => false;  // 점화 여부 - 추후 확장
    public string CurrentStance => playerData.currentStance.ToString();       //현재의 자세를 가져옴
    public CharacterClass ChClass{get; set;}

    //---
    [Header("Stance UI")]
    [SerializeField] private Button stanceToggleButton;
    private StatusDisplay statusDisplay;
    //---

    [SerializeField] public List<TickEffect> tickEffects = new();       // 턴마다 지속되는 효과
    [SerializeField] public List<InstanceEffect> instantEffects = new(); // 즉시 적용 효과

    private void Awake()
    {
        statusDisplay = GetComponentInChildren<StatusDisplay>();
        animator = transform.Find("Visual")?.GetComponent<Animator>();
        spriteRenderer = transform.Find("Visual")?.GetComponent<SpriteRenderer>();

        if (playerData != null && playerData.animationController != null)
        {
            animator.runtimeAnimatorController = playerData.animationController;
        }
        else
        {
            Debug.LogWarning($"[{name}] PlayerData 또는 AnimationController가 누락되었습니다.");
        }
    }

    void Start()
    {
        targetArrow.Init(this); // 옵저버 연결
    }

    public void TakeTrueDamage(float damage)
    {
        //Debug.Log($"{playerData.CharacterName}가 {damage}의 트루데미지를 받음! 현재 체력: {playerData.currentHP}");
        // 문체 효과 적용
        damage = StyleManager.Instance.GetOnComingDamageModify(this, this, damage);
        currentHP -= damage;

        var dmg = new DmgTextData
        {
            Text = $"{Mathf.RoundToInt(damage)}",
            type = DmgTextType.Normal,
            isCardEnhanced = false,
            isStanceEnhanced = false,
            isWeakened = false
        };

        this.dmgTextQueue.Enqueue(dmg);

        playerData.currentHP = Mathf.Max(0, playerData.currentHP - damage);
    }

    public void BindHpBar(HpBarDisplay bar)
    {
        hpBarDisplay = bar;
        hpBarDisplay.BindPlayerData(playerData);
    }


    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect == null) return;

        // 디버프 저항
        if (hasResist && Debuff.IsDebuff(effect.statType, effect.value))
            return;

        switch (effect)
        {
            case TickEffect tick:
                {
                    float finalValue = tick.value;

                    // Active: 다음 상태이상 수치 증가
                    finalValue += ConsumeActiveBonusIfExists();

                    tickEffects.Add(new TickEffect
                    {
                        statType = tick.statType,
                        value = finalValue,
                        duration = tick.duration
                    });

                    if (tick.statType == BuffStatType.Burn && finalValue > 0 && IsOpponentActingTurn())
                        burnIncreasedDuringOpponentTurn = true;

                    break;
                }

            case InstanceEffect inst:
                {
                    var incoming = new InstanceEffect
                    {
                        statType = inst.statType,
                        value = inst.value,
                        isMaintain = inst.isMaintain
                    };

                    // 1) Active 먼저 적용
                    incoming.value += ConsumeActiveBonusIfExists();

                    bool isDebuff = Debuff.IsDebuff(incoming.statType, incoming.value);
                    bool isPositive = !isDebuff;

                    // 2) Bless / Penance 선처리
                    if (incoming.statType != BuffStatType.Bless &&
                        incoming.statType != BuffStatType.Penance)
                    {
                        if (isPositive)
                            TryApplyStoredBlessIfExists(incoming);
                        else
                            TryApplyStoredPenanceIfExists(incoming);
                    }

                    // 3) Bless 자체 적용
                    if (incoming.statType == BuffStatType.Bless)
                    {
                        TryApplyBlessBonus(incoming.value);
                        statusDisplay?.PlayerUpdateUI();
                        return;
                    }

                    // 4) Penance 자체 적용
                    if (incoming.statType == BuffStatType.Penance)
                    {
                        TryApplyPenanceBonus(incoming.value);
                        statusDisplay?.PlayerUpdateUI();
                        return;
                    }

                    // 5) 실제 저장
                    var existing = instantEffects.Find(e => e.statType == incoming.statType);
                    if (existing != null)
                    {
                        float before = existing.value;
                        existing.value = Mathf.Clamp(existing.value + incoming.value, 0, 999);
                        existing.isMaintain = existing.isMaintain || incoming.isMaintain;

                        if (incoming.statType == BuffStatType.Burn &&
                            existing.value > before &&
                            IsOpponentActingTurn())
                        {
                            burnIncreasedDuringOpponentTurn = true;
                        }
                    }
                    else
                    {
                        instantEffects.Add(new InstanceEffect
                        {
                            statType = incoming.statType,
                            value = Mathf.Clamp(incoming.value, 0, 999),
                            isMaintain = incoming.isMaintain
                        });

                        if (incoming.statType == BuffStatType.Burn &&
                            incoming.value > 0 &&
                            IsOpponentActingTurn())
                        {
                            burnIncreasedDuringOpponentTurn = true;
                        }
                    }

                    break;
                }

            default:
                Debug.LogWarning($"[ApplyStatusEffect] 알 수 없는 타입: {effect.GetType()}");
                break;
        }

        statusDisplay?.PlayerUpdateUI();
    }

    /// <summary>
    /// 해당 스탯에 현재 적용 중인 버프를 계산하여 반환
    /// </summary>
    /// <param name="statType">수정할 스탯 타입</param>
    /// <param name="baseValue">기본값</param>
    /// <returns>버프 적용 후 최종 값</returns>
    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        float result = baseValue;

        foreach (var e in tickEffects)
            if (e.statType == statType)
                result += e.value;

        foreach (var e in instantEffects)
            if (e.statType == statType)
                result += e.value;

        return result;
    }

    /// <summary>
    /// 데미지를 받아 처리하는 함수
    /// </summary>
    /// <param name="amount">데미지 량</param>
    public float TakeDamage(float amount)
    {
        if (hasBlock)
        {
            hasBlock = false;
            return 0;
        }

        amount = StyleManager.Instance.GetOnComingDamageModify(this, this, amount);

        // Guard 감소 적용 (레온용)
        if (HasGuardValue())
        {
            amount = Mathf.Max(0, amount - GetGuardValue());
            ConsumeGuard(); // 발동 후 초기화
        }

        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f);
        reduced = Mathf.Max(reduced, 1f);

        if (playerData.currentStance == StancType.Defense)
            reduced *= 0.5f;
        else if (playerData.currentStance == StancType.Rush)
            reduced *= 2f;

        reduced = Mathf.Round(reduced);

        playerData.currentHP = Mathf.Max(0, playerData.currentHP - reduced);
        return reduced;
    }

    /// <summary>
    /// 체력 회복 (grace 수치 만큼 추가)
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        var grace = instantEffects.Find(e => e.statType == BuffStatType.Bless);
        if (grace != null && grace.value > 0)
        {
            amount += grace.value;
            Debug.Log($"[Grace] 회복량 증가 +{grace.value} → {amount}");
            grace.value = 0;
        }

        playerData.currentHP = Mathf.Min(playerData.MaxHP, playerData.currentHP + amount);
    }

    /// <summary>
    /// 생존 여부 확인
    /// </summary>
    /// <returns>체력이 0 초과인지 여부</returns>
    public bool IsAlive()
    {
        return playerData.currentHP > 0;
    }

    /// <summary>
    /// 플레이어 초기화 (데이터 및 클래스 설정)
    /// </summary>
    /// <param name="data">플레이어 데이터</param>
    /// <param name="charClass">캐릭터 클래스</param>
    public void Initialize(PlayerData data, CharacterClass charClass)
    {
        playerData = data;
        ChClass = charClass;
        deckModel = new DeckModel(); // 덱은 여기서 직접 생성하거나 외부에서 주입
    }

    /// <summary>
    /// 현재 덱 상태 출력 (디버그용)
    /// </summary>
    public void PrintDeckState()
    {
        //Debug.Log($"[{playerData.CharacterName}] Hand: {Deck.Hand.Count}, Used: {Deck.UsedCount()}");
    }


    /// <summary>
    /// 매 턴마다 상태효과 지속시간 감소 및 종료 처리
    /// </summary>
    public void TickStatusEffects()
    {
        for (int i = tickEffects.Count - 1; i >= 0; i--)
        {
            tickEffects[i].duration--;
            if (tickEffects[i].duration <= 0)
            {
                Debug.Log($"[TickEffect 만료] {tickEffects[i].statType}");
                tickEffects.RemoveAt(i);
            }
        }

        // 모든 InstantEffect의 유지 여부 해제
        foreach (var effect in instantEffects)
        {
            effect.isMaintain = false;  // 다음 턴부터는 제거 대상이 됨
        }

        statusDisplay?.PlayerUpdateUI();
    }

    /// <summary>
    /// 특정 타입의 버프/디버프가 있는지 확인
    /// </summary>
    /// <param name="type">스탯 타입</param>
    /// <returns>존재 여부</returns>
    public bool HasEffect(BuffStatType type)
    {
        return tickEffects.Exists(e => e.statType == type)
                || instantEffects.Exists(e => e.statType == type);
    }

    /// <summary>
    /// 스턴 상태인지 확인
    /// </summary>
    /// <returns>스턴 여부</returns>
    public bool IsStunned() => HasEffect(BuffStatType.Stun);

    // block 부여
    public void GrantBlock()
    {
        hasBlock = true;
        statusDisplay?.PlayerUpdateUI();
        //Debug.Log($"{playerData.CharacterName}에게 block 부여 (1턴 1회 무효화)");
    }

    // block 제거
    public void ClearBlock()
    {
        if (hasBlock)
        {
            Debug.Log($"{playerData.CharacterName}의 block 효과 만료");
            hasBlock = false;
            statusDisplay?.PlayerUpdateUI();
        }
    }

    public void Setup(PlayerData data)
    {
        playerData = data;
        ChClass = data.CharacterClass;

        deckModel = new DeckModel();

        if (data.currentDeck == null || data.currentDeck.Count != 5)
            data.ResetDeckIndexesToDefault();

        data.LoadDeckFromIndexes(DataManager.Instance.AllCards);
        deckModel.Initialize(data.currentDeck);

        if (!IsAlive())
            playerData.ReviveIfDead();

        if (hpBarDisplay != null)
            hpBarDisplay.BindPlayerData(playerData);

        if (stanceEffectData == null)
            stanceEffectData = new StanceEffectData();

        InitializeStanceSystem();
    }

    //──────── K.T.H 변경 ────────
    private void OnEnable()
    {
        // 캐릭터가 활성화될 때 버튼도 활성화
        if (stanceToggleButton != null)
            stanceToggleButton.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // 캐릭터가 비활성화될 때 버튼도 비활성화
        if (stanceToggleButton != null)
            stanceToggleButton.gameObject.SetActive(false);
    }

    
    public void ChangeStance(StancType Stance) //StancUI 함수
    {
        StancType stance = Stance;
        playerData.currentStance = stance;
        stanceSystem?.ChangeStance(stance);
    }

    public void CameraActionPlay()
    {
        if(GameManager.Instance == null || GameManager.Instance.combatCameraController == null)
        {
            Debug.LogError("CameraController is not initialized.");
            return;
        }
        else if(this == null)
        {
            Debug.LogError("null");
            return;
        }
        //GameManager.Instance.combatCameraController.CameraZoomInAction(transform);
    }

    // 최대 체력
    public float maxHP
    {
        get => playerData.MaxHP;
        set => playerData.MaxHP = value;
    }
    //현재 체력
    public float currentHP
    {
        get => playerData.currentHP;
        set => playerData.currentHP = value;
    }
    //체력 변화 시
    public void UpdateHpStatus()
    {
        maxHP = playerData.MaxHP;
        currentHP = playerData.currentHP;
    }

    //공격 애니메이션 호출 시
    private Action currentAttackHitCallback;
    private Coroutine resetAttackRoutine;

    public void PlayAttackAnimation(int attackType, Action onHitTiming = null)
    {
        if (animator == null)
            return;

        currentAttackHitCallback = onHitTiming;

        animator.SetInteger("Attack", attackType);

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);

        resetAttackRoutine = StartCoroutine(ResetAttackParam(1.5f));
    }

    private IEnumerator ResetAttackParam(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetInteger("Attack", -1);
        currentAttackHitCallback = null;
        resetAttackRoutine = null;
    }

    // 애니메이션 이벤트에서 호출할 함수
    public void OnAttackHitEvent()
    {
        currentAttackHitCallback?.Invoke();
        currentAttackHitCallback = null; // 한 번만 실행되게
    }

    //피격 애니메이션 호출 시
    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("Hit", true);
            GameManager.Instance.StartCoroutine(ResetBool("Hit", 1.2f));
        }
    }

    private IEnumerator ResetBool(string param, float delay)
    {
        yield return new WaitForSeconds(delay);
        // 오브젝트가 살아있을 때만 실행
        if (this != null && animator != null)
        {
            animator.SetBool(param, false);
        }
    }

    /// <summary>
    /// 자신의 위치 돌려주기
    /// </summary>
    public Transform CachedTransform => transform;

    [SerializeField] private DmgBarQueueHandler queue;

    public DmgBarQueueHandler dmgTextQueue => queue;


    /// <summary>
    /// 현재 적용 중인 공격력 버프 총합 반환
    /// </summary>
    public float GetBuffAtk()
    {
        float atkTotal = 0;
        foreach (var effect in tickEffects)
        {
            if (effect.statType == BuffStatType.Attack)
                atkTotal += effect.value;
        }
        return atkTotal;
    }

    /// <summary>
    /// 현재 적용 중인 방어력 버프 총합 반환
    /// </summary>
    public float GetBuffDef()
    {
        float defTotal = 0;
        foreach (var effect in tickEffects)
        {
            if (effect.statType == BuffStatType.Defense)
                defTotal += effect.value;
        }
        return defTotal;
    }

    public DmgBarDisplay dmgBar => dmgBarDisplay;
    public TargetArrowDisplay tarArrow => targetArrow;

    public void HideStatusUI()
    {
        if (statusDisplay != null)
            statusDisplay.gameObject.SetActive(false);
    }

    public void ShowStatusUI()
    {
        if (statusDisplay != null)
            statusDisplay.gameObject.SetActive(true);
    }

    //발동 시 사라져야 하는 경우
    public void TriggerEffectOnce(BuffStatType type)
    {
        for (int i = instantEffects.Count - 1; i >= 0; i--)
        {
            if (instantEffects[i].statType == type)
            {
                if (!instantEffects[i].isMaintain)
                    instantEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 화상 데미지 처리
    /// </summary>
    public void ApplyBurnEffect()
    {
        var burn = instantEffects.Find(e => e.statType == BuffStatType.Burn);
        if (burn != null && burn.value > 0)
        {
            Debug.Log($"[Burn] {playerData.CharacterName} 화상 피해 {burn.value}");
            TakeTrueDamage(burn.value);

            // isMaintain 상태가 아닌 경우 초기화
            if(!burn.isMaintain)
            {
                burn.value = 0;
            }
        }
    }


    public void ApplyBurnOnTurnStart()
    {
        var burn = instantEffects.Find(e => e.statType == BuffStatType.Burn);
        if (burn == null || burn.value <= 0) return;

        // 직전 상대 턴에 증가했으면 이번 턴에는 발동 안 함
        if (burnIncreasedDuringOpponentTurn)
        {
            burnIncreasedDuringOpponentTurn = false;
            return;
        }

        float damage = burn.value;
        TakeTrueDamage(damage);

        burn.value = Mathf.Floor(burn.value / 2f);

        if (burn.value < 1f)
            instantEffects.Remove(burn);
    }

    /// <summary>
    /// 빙결 처리
    /// </summary>
    /// <param name="baseDamage"></param>
    /// <returns></returns>
    public float ApplyFreezePenalty(float baseDamage)
    {
        var freeze = instantEffects.Find(e => e.statType == BuffStatType.Freeze);
        if (freeze == null || freeze.value <= 0)
            return baseDamage;

        float reduced = Mathf.Max(0, baseDamage - freeze.value);
        return reduced;
    }

    public void ApplyActivateBonus(BuffStatType incoming)
    {
        var activate = instantEffects.Find(e => e.statType == BuffStatType.Activate);
        if (activate == null || activate.value <= 0) return;

        if (incoming == BuffStatType.Burn || incoming == BuffStatType.Freeze)
        {
            var target = instantEffects.Find(e => e.statType == incoming);
            if (target != null)
            {
                target.value = Mathf.Min(50, target.value + activate.value);
                Debug.Log($"[Activate] {playerData.CharacterName} {incoming} 상태 강화됨: +{activate.value}");
            }
        }

        // 활성도는 발동 후 초기화
        activate.value = 0;
    }

    /// <summary>
    /// 축복 버프 처리
    /// </summary>
    /// <param name="blessValue"></param>
    public void TryApplyBlessBonus(float blessValue)
    {
        var targetBuff = instantEffects
            .Where(e => e.statType != BuffStatType.Bless &&
                        !Debuff.IsDebuff(e.statType, e.value) &&
                        e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetBuff != null)
        {
            targetBuff.value += blessValue;
        }
        else
        {
            var bless = instantEffects.Find(e => e.statType == BuffStatType.Bless);
            if (bless != null) bless.value += blessValue;
            else
            {
                instantEffects.Add(new InstanceEffect
                {
                    statType = BuffStatType.Bless,
                    value = blessValue,
                    isMaintain = false
                });
            }
        }
    }

    private void TryApplyStoredBlessIfExists(InstanceEffect incoming)
    {
        var bless = instantEffects.Find(e => e.statType == BuffStatType.Bless && e.value > 0);
        if (bless == null) return;

        incoming.value += bless.value;
        bless.value = 0;
    }

    /// <summary>
    /// 정화 디버프 처리
    /// </summary>
    /// <param name="purifyValue"></param>
    public void TryApplyPenanceBonus(float penanceValue)
    {
        var targetDebuff = instantEffects
            .Where(e => e.statType != BuffStatType.Penance &&
                        Debuff.IsDebuff(e.statType, e.value) &&
                        e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetDebuff != null)
        {
            targetDebuff.value = Mathf.Max(0, targetDebuff.value - penanceValue);
        }
        else
        {
            var penance = instantEffects.Find(e => e.statType == BuffStatType.Penance);
            if (penance != null) penance.value += penanceValue;
            else
            {
                instantEffects.Add(new InstanceEffect
                {
                    statType = BuffStatType.Penance,
                    value = penanceValue,
                    isMaintain = false
                });
            }
        }
    }

    private void TryApplyStoredPenanceIfExists(InstanceEffect incoming)
    {
        var penance = instantEffects.Find(e => e.statType == BuffStatType.Penance && e.value > 0);
        if (penance == null) return;

        incoming.value = Mathf.Max(0, incoming.value - penance.value);
        penance.value = 0;
    }

    /// <summary>
    /// 존재하는 정화 수치 적용
    /// </summary>
    /// <param name="incoming"></param>
    private void TryApplyStoredPurifyIfExists(InstanceEffect incoming)
    {
        var purify = instantEffects.Find(e => e.statType == BuffStatType.Penance && !e.isMaintain);
        if (purify != null)
        {
            incoming.value = Mathf.Max(0, incoming.value - purify.value);
            Debug.Log($"[Purify Triggered] {incoming.statType} 수치 감소 -{purify.value}");
            instantEffects.Remove(purify); // 일회성
        }
    }

    public void ApplyCrimeOnTurnEnd()
    {
        var crime = instantEffects.Find(e => e.statType == BuffStatType.Crime);
        if (crime == null || crime.value <= 0) return;

        TakeTrueDamage(crime.value);
    }

    public float ApplyScarBonus(float baseDamage)
    {
        var scar = instantEffects.Find(e => e.statType == BuffStatType.Scar);
        if (scar == null || scar.value <= 0)
            return baseDamage;

        float finalDamage = baseDamage + scar.value;
        scar.value = 0; // 발동 후 초기화
        return finalDamage;
    }

    public bool TryTriggerStun()
    {
        var stun = instantEffects.Find(e => e.statType == BuffStatType.Stun);
        if (stun == null || stun.value <= 0)
            return false;

        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll <= stun.value)
        {
            stun.value = 0;
            return true;
        }

        return false;
    }

    public bool CanActThisTurn() => !skipTurnThisRound;

    public bool HasGuardValue()
    {
        var guard = instantEffects.Find(e => e.statType == BuffStatType.Guard);
        return guard != null && guard.value > 0;
    }

    public float GetGuardValue()
    {
        var guard = instantEffects.Find(e => e.statType == BuffStatType.Guard);
        return guard?.value ?? 0f;
    }

    public void MarkGuardRedirectPending()
    {
        guardRedirectPending = true;
    }

    private void ConsumeGuard()
    {
        var guard = instantEffects.Find(e => e.statType == BuffStatType.Guard);
        if (guard != null)
            guard.value = 0;

        guardRedirectPending = false;
    }

    /// <summary>
    /// 출혈 수치 적용
    /// </summary>
    /// <param name="baseDamage"></param>
    /// <returns></returns>
    public float ApplyBleedBonus(float baseDamage)
    {
        var bleed = instantEffects.Find(e => e.statType == BuffStatType.Scar);
        if (bleed != null && bleed.value > 0)
        {
            Debug.Log($"[Bleed] {playerData.CharacterName} 추가 피해 {bleed.value}");
            baseDamage += bleed.value;

            if (!bleed.isMaintain)
            {
                bleed.value = 0;
            }
        }

        return baseDamage;
    }

    public bool TryTriggerGuardRedirect(out PlayerController redirectTarget)
    {
        redirectTarget = null;

        var guard = instantEffects.Find(e => e.statType == BuffStatType.Guard);
        if (guard != null && guard.value > 0)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll <= guard.value)
            {
                var leon = GameManager.Instance.turnController.battleFlow.playerParty
                    .Find(p => p is PlayerController pc && pc.ChClass == CharacterClass.Leon) as PlayerController;

                if (leon != null)
                {
                    Debug.Log($"[Guard] {playerData.CharacterName} 대신 레온이 공격받음!");
                    redirectTarget = leon;

                    guard.value = 0;

                    return true;
                }
            }
        }
        return false;
    }


    /// <summary>
    /// 스탠스 메서드
    /// </summary>
    public void NotifyCardUsed(bool isSelf)
    {
        stanceSystem?.OnAllyUsedCard(isSelf);
    }

    public void ResetStanceBattleState()
    {
        stanceEffectData?.Reset();
        stanceSystem?.ResetGauge();
    }

    public void OnBattleEndReset()
    {
        stanceSystem?.OnBattleEnd();
        stanceEffectData?.Reset();
    }

    //규율 메서드
    public void ApplyDisciplineBonusToParty(float sinAmount, List<IStatusReceiver> playerParty)
    {
        if (sinAmount <= 0) return;
        if (stanceEffectData == null || !stanceEffectData.NextAttackSinBuffBonus) return;

        foreach (var ally in playerParty)
        {
            if (ally is PlayerController pc && pc.IsAlive())
            {
                pc.ApplyStatusEffect(new InstanceEffect
                {
                    statType = BuffStatType.Attack,
                    value = sinAmount,
                    isMaintain = false
                });

                pc.ApplyStatusEffect(new InstanceEffect
                {
                    statType = BuffStatType.Defense,
                    value = sinAmount,
                    isMaintain = false
                });
            }
        }

        stanceEffectData.NextAttackSinBuffBonus = false;
        Debug.Log($"[규율] 이번 공격에서 부여한 죄악 {sinAmount} 만큼 아군 공방 증가");
    }

    private int potentialChargeLockTurn = 0;
    private bool noBlessConsumeOnce = false;
    private int immortalMinHp = 0;
    private int immortalCount = 0;

    public void SetPotentialChargeLock(int turns)
    {
        potentialChargeLockTurn = Mathf.Max(potentialChargeLockTurn, turns);
    }

    public void SetNoBlessConsumeOnce(bool value)
    {
        noBlessConsumeOnce = value;
    }

    public void TriggerStoredBlessImmediately()
    {
        var bless = instantEffects.Find(e => e.statType == BuffStatType.Bless);
        if (bless != null && bless.value > 0)
        {
            TryApplyBlessBonus(bless.value);

            if (!noBlessConsumeOnce)
                bless.value = 0;

            noBlessConsumeOnce = false;
        }
    }

    public void MultiplyInstantEffect(BuffStatType statType, int multiplier)
    {
        var eff = instantEffects.Find(e => e.statType == statType);
        if (eff != null)
            eff.value = Mathf.Clamp(eff.value * multiplier, 0, 50);
    }

    public void MultiplyAllPositiveEffects(int multiplier)
    {
        foreach (var eff in instantEffects)
        {
            if (!Debuff.IsDebuff(eff.statType, eff.value) && eff.value > 0)
                eff.value = Mathf.Clamp(eff.value * multiplier, 0, 50);
        }

        foreach (var eff in tickEffects)
        {
            if (!Debuff.IsDebuff(eff.statType, eff.value) && eff.value > 0)
                eff.value = Mathf.Clamp(eff.value * multiplier, 0, 50);
        }
    }

    public void TriggerOppositeStanceEffect()
    {
        if (stanceSystem == null)
            return;

        var current = playerData.currentStance;
        StancType opposite = current switch
        {
            StancType.Seek => StancType.Insight,
            StancType.Insight => StancType.Seek,
            StancType.Mercy => StancType.Discipline,
            StancType.Discipline => StancType.Mercy,
            StancType.Rush => StancType.Defense,
            StancType.Defense => StancType.Rush,
            _ => current
        };

        var old = playerData.currentStance;
        playerData.currentStance = opposite;
        StanceEffectHandler.TriggerStanceEffect(this, GameManager.Instance.turnController.battleFlow);
        playerData.currentStance = old;
    }

    public void SetImmortalThreshold(int minHp, int count)
    {
        immortalMinHp = minHp;
        immortalCount = count;
    }

    private float ConsumeActiveBonusIfExists()
    {
        var active = instantEffects.Find(e => e.statType == BuffStatType.Activate);
        if (active == null || active.value <= 0)
            return 0;

        float bonus = active.value;
        active.value = 0; // 발동 후 초기화
        return bonus;
    }

}
