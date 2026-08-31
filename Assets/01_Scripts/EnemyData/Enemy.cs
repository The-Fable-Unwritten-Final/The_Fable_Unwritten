using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using DG.Tweening;

public class Enemy : MonoBehaviour, IStatusReceiver
{
    public EnemyData enemyData;

    public bool hasBlock = false;
    public bool hasResist { get; set; } = false;
    private bool isTargetable;

    private bool isDead = false;
    private bool isDeathPending = false;

    private IEnemyMechanic mechanic;
    public IEnemyMechanic Mechanic => mechanic;

    // 상처 폭발 상태
    private bool scarBurstActive = false; 
    
    // 이번 턴 이미 상처가 발동했는지
    private bool scarTriggeredThisTurn = false;

    public void SetMechanic(IEnemyMechanic enemyMechanic)
    {
        mechanic = enemyMechanic;

        mechanic?.Initialize(
            this,
            GameManager.Instance.turnController.battleFlow
        );
    }

    public bool IsDeathPending => isDeathPending;

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

    [SerializeField] private Transform footpoint;
    [SerializeField] private Transform bodypoint;
    [SerializeField] private Transform headpoint;
    [SerializeField] private Transform overheadpoint;
    [SerializeField] private Transform aheadpoint;

    public Transform FootPoint => anchorController.FootPoint;
    public Transform BodyPoint => anchorController.BodyPoint;
    public Transform HeadPoint => anchorController.HeadPoint;
    public Transform OverheadPoint => anchorController.OverheadPoint;

    public Transform AheadPoint => anchorController.AheadPoint;


    public event System.Action OnTargetableChanged;

    [SerializeField] private HpBarDisplay hpBarDisplay;
    [SerializeField] private DmgBarDisplay dmgBarDisplay;
    [SerializeField] private TargetArrowDisplay targetArrow;
    [SerializeField] private DmgBarQueueHandler queue;

    public SpriteRenderer spriteRenderer;
    public Animator animator;

    private Action currentAttackHitCallback;
    private Coroutine resetAttackRoutine;


    private Sequence hitShakeSequence;
    private StatusDisplay statusDisplay;

    [SerializeField] public List<TickEffect> tickEffects = new();
    [SerializeField] public List<InstanceEffect> instantEffects = new();

    private bool skipTurnThisRound = false;
    private bool burnIncreasedDuringOpponentTurn = false;

    private CharacterClass characterClass = CharacterClass.Enemy;

    public CharacterClass ChClass
    {
        get => characterClass;
        set => characterClass = value;
    }

    public DeckModel Deck => null;
    public bool IsIgnited => false;
    public string CurrentStance => enemyData != null ? enemyData.currentStance.ToString() : "";
    public Transform CachedTransform => transform;
    public DmgBarQueueHandler dmgTextQueue => queue;
    public DmgBarDisplay dmgBar => dmgBarDisplay;
    public TargetArrowDisplay tarArrow => targetArrow;

    [SerializeField] private CharacterAnchorController anchorController;


    private void Awake()
    {
        statusDisplay = GetComponentInChildren<StatusDisplay>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyData != null && enemyData.animationController != null && animator != null)
        {
            animator.runtimeAnimatorController = enemyData.animationController;
        }
        else if (enemyData == null)
        {
            Debug.LogWarning($"[{name}] enemyData가 누락되었습니다.");
        }
    }

    private void Start()
    {
        targetArrow?.Init(this);
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;

        anchorController.Apply(enemyData.anchorData);

        // 문체 효과 적용
        enemyData.MaxHP = StyleManager.Instance.ModifyEnemyMaxHp(this, (int)enemyData.MaxHP);
        enemyData.CurrentHP = enemyData.MaxHP;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyData.animationController != null && animator != null)
            animator.runtimeAnimatorController = enemyData.animationController;

        if (hpBarDisplay != null)
            hpBarDisplay.BindEnemyData(enemyData);

        if (hpBarDisplay != null)
        {
            var rt = hpBarDisplay.GetComponent<RectTransform>();
            var s = rt.localScale;

            if (enemyData.type == EnemyType.elite || enemyData.type == EnemyType.boss)
                s.x = 1.8f;
            else
                s.x = 1.1f;

            rt.localScale = s;
        }

        isDead = false;
        isDeathPending = false;

        SetMechanic(EnemyMechanicFactory.Create(this));
    }

    public void ChangeStance(StancType stance)
    {
        
    }

    public float GetEffectValue(BuffStatType type)
    {
        float total = 0;

        foreach (var e in tickEffects)
            if (e.statType == type) total += e.value;

        foreach (var e in instantEffects)
            if (e.statType == type) total += e.value;

        return total;
    }

    private bool IsOpponentActingTurn()
    {
        var flow = GameManager.Instance?.turnController?.battleFlow;
        if (flow == null) return false;

        // 적 입장에서는 PlayerTurn이 상대 턴
        return flow.currentTurn == TurnState.PlayerTurn;
    }

    public void OnBattleStart()
    {
        skipTurnThisRound = false;
        burnIncreasedDuringOpponentTurn = false;

        scarBurstActive = false; 
        scarTriggeredThisTurn = false;

        hasBlock = false;
        hasResist = false;

        tickEffects.Clear();
        instantEffects.Clear();

        mechanic?.OnBattleStart();

        statusDisplay?.EnemyUpdateUI();
    }

    public void OnTurnStart()
    {
        skipTurnThisRound = false;

        float burnDamage = ApplyBurnOnTurnStart();

        if (burnDamage > 0)
            mechanic?.OnBurnDamageTaken(burnDamage);

        if (TryTriggerStun())
            skipTurnThisRound = true;

        mechanic?.OnEnemyTurnStart();

        statusDisplay?.EnemyUpdateUI();
    }

    public void OnTurnEnd()
    {
        ApplyCrimeOnTurnEnd();
        TickStatusEffects();
        ClearTurnEndInstantEffects();
        ClearBlock();

        // Freeze는 턴 종료 시 초기화
        ClearInstantEffect(BuffStatType.Freeze);

        mechanic?.OnEnemyTurnEnd();

        statusDisplay?.EnemyUpdateUI();
    }

    public bool CanActThisTurn() => !skipTurnThisRound;

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect == null)
            return;

        if (hasResist &&
            Debuff.IsDebuff(effect.statType, effect.value))
        {
            return;
        }

        switch (effect)
        {
            case TickEffect tick:
                {
                    float finalValue = tick.value;

                    tickEffects.Add(new TickEffect
                    {
                        statType = tick.statType,
                        value = finalValue,
                        duration = tick.duration
                    });

                    if (tick.statType == BuffStatType.Burn &&
                        finalValue > 0 &&
                        IsOpponentActingTurn())
                    {
                        burnIncreasedDuringOpponentTurn = true;
                    }

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

                    bool isDebuff =
                        Debuff.IsDebuff(
                            incoming.statType,
                            incoming.value
                        );

                    bool isPositive = !isDebuff;

                    // Bless / Penance는
                    // "다음에 들어오는 효과"에 적용
                    if (incoming.statType != BuffStatType.Bless &&
                        incoming.statType != BuffStatType.Penance)
                    {
                        if (isPositive)
                        {
                            TryApplyStoredBlessIfExists(incoming);
                        }
                        else
                        {
                            TryApplyStoredPenanceIfExists(incoming);
                        }
                    }

                    // Bless 자체는 저장
                    if (incoming.statType == BuffStatType.Bless)
                    {
                        TryApplyBlessBonus(incoming.value);
                        statusDisplay?.EnemyUpdateUI();
                        return;
                    }

                    // Penance 자체는 저장
                    if (incoming.statType == BuffStatType.Penance)
                    {
                        TryApplyPenanceBonus(incoming.value);
                        statusDisplay?.EnemyUpdateUI();
                        return;
                    }

                    var existing = instantEffects.Find(e => e.statType == incoming.statType);

                    // Scar만 최대 50
                    float maxValue = incoming.statType == BuffStatType.Scar ? 50f : 999f;

                    if (existing != null)
                    {
                        float before = existing.value;

                        existing.value = Mathf.Clamp(existing.value + incoming.value, 0f, maxValue);

                        existing.isMaintain = existing.isMaintain || incoming.isMaintain;

                        if (incoming.statType == BuffStatType.Burn && existing.value > before && IsOpponentActingTurn())
                            burnIncreasedDuringOpponentTurn = true;
                    }
                    else
                    {
                        instantEffects.Add(new InstanceEffect
                        {
                            statType = incoming.statType,
                            value = Mathf.Clamp(
                                incoming.value,
                                0f,
                                maxValue
                            ),
                            isMaintain = incoming.isMaintain
                        });

                        if (incoming.statType == BuffStatType.Burn && incoming.value > 0 && IsOpponentActingTurn())
                            burnIncreasedDuringOpponentTurn = true;
                    }
                    break;
                }

            default:
                Debug.LogWarning($"[ApplyStatusEffect] 알 수 없는 타입: {effect.GetType()}");
                break;
        }
        statusDisplay?.EnemyUpdateUI();
    }

    public float ApplyBurnOnTurnStart()
    {
        var burn = instantEffects.Find(
            e => e.statType == BuffStatType.Burn
        );

        if (burn == null || burn.value <= 0)
            return 0f;

        // 직전 플레이어 턴에 새로 증가했다면 이번에는 발동 X
        if (burnIncreasedDuringOpponentTurn)
        {
            burnIncreasedDuringOpponentTurn = false;
            return 0f;
        }

        float damage = burn.value;

        TakeTrueDamage(damage);

        burn.value = Mathf.Max(0f, burn.value - 1f);

        if (burn.value < 1f)
            instantEffects.Remove(burn);

        return damage;
    }

    public void ApplyCrimeOnTurnEnd()
    {
        var crime = instantEffects.Find(e => e.statType == BuffStatType.Crime);
        if (crime == null || crime.value <= 0) return;

        float damage = crime.value;

        instantEffects.Remove(crime);

        TakeTrueDamage(damage);
    }

    public float ApplyFreezePenalty(float baseDamage)
    {
        var freeze = instantEffects.Find(e => e.statType == BuffStatType.Freeze);
        if (freeze == null || freeze.value <= 0)
            return baseDamage;

        return Mathf.Max(0, baseDamage - freeze.value);
    }

    public float ApplyScarAttackBonus(float baseDamage)
    {
        if (scarBurstActive) return baseDamage + 10f;

        // 같은 턴 재발동 방지
        if (scarTriggeredThisTurn) return baseDamage;

        var scar = instantEffects.Find(e => e.statType == BuffStatType.Scar);

        if (scar == null || scar.value < 50f) return baseDamage;
        
        // 상처 발동
        scar.value = 0f; 
        scarBurstActive = true; 
        scarTriggeredThisTurn = true; 
        
        Debug.Log( $"[Scar] {enemyData.EnemyName} 상처 발동 / 공격 피해 +10" ); 
        
        return baseDamage + 10f; 
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

    public void TryApplyBlessBonus(float blessValue)
    {
        if (blessValue <= 0) return; 
        
        var bless = instantEffects.Find(e => e.statType == BuffStatType.Bless);
        if (bless != null)  bless.value += blessValue; 
        else 
        {
            instantEffects.Add(
                new InstanceEffect {
                    statType = BuffStatType.Bless, 
                    value = blessValue, 
                    isMaintain = false 
                }
            ); 
        }
    }

    public void TryApplyPenanceBonus(float penanceValue)
    {
        if (penanceValue <= 0) return; 
        
        var penance = instantEffects.Find(e => e.statType == BuffStatType.Penance); 
        if (penance != null)  penance.value += penanceValue; 
        else 
        { 
            instantEffects.Add(
                new InstanceEffect { 
                    statType = BuffStatType.Penance, 
                    value = penanceValue, 
                    isMaintain = false 
                }
            ); 
        }
    }

    private void TryApplyStoredBlessIfExists(InstanceEffect incoming)
    {
        var bless = instantEffects.Find(e => e.statType == BuffStatType.Bless && e.value > 0);
        if (bless == null) return;

        incoming.value += bless.value;
        instantEffects.Remove(bless);
    }

    private void TryApplyStoredPenanceIfExists(InstanceEffect incoming)
    {
        var penance = instantEffects.Find(e => e.statType == BuffStatType.Penance && e.value > 0);
        if (penance == null) return;

        incoming.value = Mathf.Max(0, incoming.value - penance.value);
        instantEffects.Remove(penance);
    }

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

        statusDisplay?.EnemyUpdateUI();
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
            if (instantEffects[i].value <= 0)
                instantEffects.RemoveAt(i);
        }
    }

    public bool HasEffect(BuffStatType type)
    {
        return tickEffects.Exists(e => e.statType == type)
            || instantEffects.Exists(e => e.statType == type);
    }

    public bool IsStunned()
    {
        return HasEffect(BuffStatType.Stun);
    }

    public void BindHpBar(HpBarDisplay bar)
    {
        hpBarDisplay = bar;
        hpBarDisplay.BindEnemyData(enemyData);
    }

    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        float result = baseValue;

        foreach (var e in tickEffects)
            if (e.statType == statType) result += e.value;

        foreach (var e in instantEffects)
            if (e.statType == statType) result += e.value;

        result = mechanic?.ModifyStat(statType, result) ?? result;

        return result;
    }

    public void Heal(float amount)
    {
        enemyData.CurrentHP = Mathf.Min(enemyData.MaxHP, enemyData.CurrentHP + amount);
        Debug.Log($"{enemyData.EnemyName} 회복: {amount}, 현재 체력: {enemyData.CurrentHP}");
    }

    public bool IsAlive()
    {
        return enemyData.CurrentHP > 0;
    }

    public void TakeTrueDamage(float damage)
    {
        damage = StyleManager.Instance.GetDamageGiveModify(this, this, BattleLogManager.Instance.card, damage);
        currentHP = Mathf.Max(0, currentHP - damage);
        
        if (!IsAlive())
        {
            isDeathPending = true;
        }
    }

    public float TakeDamage(float amount)
    {
        if (hasBlock)
        {
            hasBlock = false;
            return 0;
        }

        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f);
        reduced = Mathf.Max(reduced, 0);

        currentHP = Mathf.Max(0, currentHP - reduced);

        if(!IsAlive())
        {
            isDeathPending = true;
        }
        
        return reduced;
    }

    public void GrantBlock()
    {
        hasBlock = true;
        statusDisplay?.EnemyUpdateUI();
    }

    public void ClearBlock()
    {
        if (!hasBlock) return;
        hasBlock = false;
        statusDisplay?.EnemyUpdateUI();
    }

    public void CameraActionPlay()
    {
        // GameManager.Instance.combatCameraController.CameraZoomInAction(transform);
    }

    public float maxHP
    {
        get => enemyData.MaxHP;
        set => enemyData.MaxHP = value;
    }

    public float currentHP
    {
        get => enemyData.CurrentHP;
        set => enemyData.CurrentHP = value;
    }

    public void UpdateHpStatus()
    {
        maxHP = enemyData.MaxHP;
        currentHP = enemyData.CurrentHP;
    }

    public void UpdateHpBarFollowTarget()
    {
        if (hpBarDisplay != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                hpBarDisplay.FollowTarget(sr);
        }
    }

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

    public void OnAttackHitEvent()
    {
        currentAttackHitCallback?.Invoke();
        currentAttackHitCallback = null;
    }

    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            // 이전 진동 효과 취소
            if (hitShakeSequence != null && hitShakeSequence.IsActive())
            {
                hitShakeSequence.Kill();
            }
            // Visual의 Transform을 기준으로, 피격시 진동 효과 재생
            Transform trans = animator.transform;
            hitShakeSequence = DOTween.Sequence()
                .Append(trans.DOLocalMoveX(-0.1f, 0.05f))
                .Append(trans.DOLocalMoveX(0.1f, 0.1f))
                .Append(trans.DOLocalMoveX(0f, 0.05f));

            animator.SetBool("Hit", true);
            GameManager.Instance.StartCoroutine(ResetBool("Hit", 1f));
        }
    }

    private IEnumerator ResetBool(string param, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (this != null && animator != null)
            animator.SetBool(param, false);
    }

    public float GetBuffAtk()
    {
        return ModifyStat(BuffStatType.Attack, 0f);
    }

    public float GetBuffDef()
    {
        return ModifyStat(BuffStatType.Defense, 0f);
    }

    public void TryFinalizeDeath()
    {

        if (!isDeathPending || isDead) return;

        isDead = true;
        isDeathPending = false;

        // 몬스터의 죽음의 경우는 카메라 펀치 효과 재생 하지 않음
        //GameManager.Instance.combatCameraController.CameraPunchHard(); 

        gameObject.SetActive(false);
    }

    public void ClearScarBurst() 
    { 
        scarBurstActive = false; 
        scarTriggeredThisTurn = false; 
    }
}