using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어와 적의 공통 기능을 담은 추상 베이스 클래스
/// </summary>
public abstract class UnitBase : MonoBehaviour, IStatusReceiver
{
    [Header("UI References")]
    [SerializeField] protected HpBarDisplay hpBarDisplay;
    [SerializeField] protected DmgBarDisplay dmgBarDisplay;
    [SerializeField] protected TargetArrowDisplay targetArrow;
    [SerializeField] protected DmgBarQueueHandler dmgQueue;

    protected StatusEffectSystem statusEffectSystem;
    protected AnimationHandler animationHandler;
    protected StatusDisplay statusDisplay;
    protected bool hasBlockInternal;

    private bool isTargetable;

    /// <summary>최대 체력</summary>
    public abstract float maxHP { get; set; }

    /// <summary>현재 체력</summary>
    public abstract float currentHP { get; set; }

    /// <summary>캐릭터 클래스</summary>
    public abstract CharacterClass ChClass { get; set; }

    /// <summary>덱 (적은 null 반환)</summary>
    public abstract DeckModel Deck { get; }

    /// <summary>각성 상태</summary>
    public abstract bool IsIgnited { get; }

    /// <summary>현재 자세</summary>
    public abstract string CurrentStance { get; }

    public virtual bool hasBlock
    {
        get => hasBlockInternal;
        set => hasBlockInternal = value;
    }

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

    public event Action OnTargetableChanged;

    public Transform CachedTransform => transform;
    public DmgBarQueueHandler dmgTextQueue => dmgQueue;
    public DmgBarDisplay dmgBar => dmgBarDisplay;
    public TargetArrowDisplay tarArrow => targetArrow;

    // 상태효과 접근 (하위 호환성)
    public List<TickEffect> tickEffects =>
        new List<TickEffect>(statusEffectSystem?.TickEffects ?? new List<TickEffect>());

    public List<InstanceEffect> instantEffects =>
        new List<InstanceEffect>(statusEffectSystem?.InstantEffects ?? new List<InstanceEffect>());

    protected virtual void Awake()
    {
        statusDisplay = GetComponentInChildren<StatusDisplay>();
        statusEffectSystem = new StatusEffectSystem(this);
        statusEffectSystem.OnEffectsChanged += OnStatusEffectsChanged;

        InitializeAnimationHandler();
    }

    protected virtual void Start()
    {
        targetArrow?.Init(this);
    }

    protected virtual void OnDestroy()
    {
        if (statusEffectSystem != null)
        {
            statusEffectSystem.OnEffectsChanged -= OnStatusEffectsChanged;
        }
    }


    protected virtual void InitializeAnimationHandler()
    {
        var visualTransform = transform.Find("Visual");
        if (visualTransform == null)
        {
            visualTransform = transform; // Visual이 없으면 자기 자신 사용
        }
        animationHandler = new AnimationHandler(visualTransform, this);
    }

    /// <summary>
    /// 애니메이션 컨트롤러 설정
    /// </summary>
    protected void SetAnimatorController(RuntimeAnimatorController controller)
    {
        animationHandler?.SetAnimatorController(controller);
    }

    public virtual bool IsAlive() => currentHP > 0;

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void Die()
    {
        HideStatusUI();
        PlayDeathAnimation();
    }

    /// <summary>
    /// 일반 데미지 처리 (방어력 적용)
    /// </summary>
    public virtual float TakeDamage(float amount)
    {
        if (hasBlock)
        {
            hasBlock = false;
            return 0;
        }

        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f);
        reduced = Mathf.Max(reduced, GetMinimumDamage());
        reduced = ApplyStanceModifierToDamage(reduced);
        reduced = Mathf.Round(reduced);

        currentHP = Mathf.Max(0, currentHP - reduced);
        ShowDamageText(reduced);

        if (currentHP <= 0)
            Die();

        return reduced;
    }

    /// <summary>
    /// 트루 데미지 처리 (방어력 무시)
    /// </summary>
    public virtual void TakeTrueDamage(float damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
        ShowDamageText(damage);


        if (currentHP <= 0)
            Die();
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public virtual void Heal(float amount)
    {
        amount = ApplyHealModifiers(amount);
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    /// <summary>
    /// 최소 데미지 반환 (플레이어: 1, 적: 0)
    /// </summary>
    protected virtual float GetMinimumDamage() => 0f;

    /// <summary>
    /// 스탠스에 따른 데미지 배율 적용
    /// </summary>
    protected virtual float ApplyStanceModifierToDamage(float damage) => damage;

    /// <summary>
    /// 회복량 수정자 적용
    /// </summary>
    protected virtual float ApplyHealModifiers(float amount) => amount;

    /// <summary>
    /// 데미지 텍스트 표시
    /// </summary>
    protected virtual void ShowDamageText(float damage)
    {
        var dmg = new DmgTextData
        {
            Text = $"-{Mathf.RoundToInt(damage)}",
            type = DmgTextType.Normal,
            isCardEnhanced = false,
            isStanceEnhanced = false,
            isWeakened = false
        };
        dmgQueue?.Enqueue(dmg);
    }

    public virtual void UpdateHpStatus() { }

    public virtual void GrantBlock()
    {
        hasBlock = true;
        UpdateStatusUI();
    }

    public virtual void ClearBlock()
    {
        if (hasBlock)
        {
            hasBlock = false;
            UpdateStatusUI();
        }
    }

    public virtual void ApplyStatusEffect(StatusEffect effect)
    {
        statusEffectSystem?.ApplyEffect(effect);
    }

    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        return statusEffectSystem?.ModifyStat(statType, baseValue) ?? baseValue;
    }

    public bool HasEffect(BuffStatType type)
    {
        return statusEffectSystem?.HasEffect(type) ?? false;
    }

    public bool IsStunned() => statusEffectSystem?.IsStunned() ?? false;

    public float GetBuffAtk() => statusEffectSystem?.GetEffectValue(BuffStatType.Attack) ?? 0;

    public float GetBuffDef() => statusEffectSystem?.GetEffectValue(BuffStatType.Defense) ?? 0;

    /// <summary>
    /// 턴 시작 시 상태효과 처리
    /// </summary>
    public virtual void ProcessTurnStart()
    {
        statusEffectSystem?.ExecuteTrigger(EffectTriggerType.TurnStart);
    }

    /// <summary>
    /// 턴 종료 시 상태효과 처리
    /// </summary>
    public virtual void TickStatusEffects()
    {
        statusEffectSystem?.ExecuteTrigger(EffectTriggerType.TurnEnd);
    }

    /// <summary>
    /// 피격 시 상태효과 처리
    /// </summary>
    public virtual void OnHit()
    {
        statusEffectSystem?.ExecuteTrigger(EffectTriggerType.OnHit);
    }

    /// <summary>
    /// 공격 시 상태효과 처리
    /// </summary>
    public virtual void OnAttack()
    {
        statusEffectSystem?.ExecuteTrigger(EffectTriggerType.OnAttack);
    }

    /// <summary>
    /// 효과 발동 후 제거
    /// </summary>
    public void TriggerEffectOnce(BuffStatType type)
    {
        statusEffectSystem?.TriggerEffectOnce(type);
    }/// <summary>
     /// 빙결 감소량 반환
     /// </summary>
    public float GetFreezePenalty()
    {
        return statusEffectSystem?.GetFreezePenalty() ?? 0f;
    }

    /// <summary>
    /// 상처 보너스 반환 및 소모
    /// </summary>
    public float GetScarBonus()
    {
        return statusEffectSystem?.GetScarBonus() ?? 0f;
    }

    /// <summary>
    /// 수호 효과 확인
    /// </summary>
    public (bool shouldRedirect, float damageReduction) GetGuardEffect()
    {
        return statusEffectSystem?.GetGuardEffect() ?? (false, 0f);
    }

    /// <summary>
    /// 수호 소모
    /// </summary>
    public void ConsumeGuard()
    {
        statusEffectSystem?.ConsumeGuard();
    }

    /// <summary>
    /// 대기 중인 축복 소모
    /// </summary>
    public float ConsumePendingBless()
    {
        return statusEffectSystem?.ConsumePendingBless() ?? 0f;
    }

    /// <summary>
    /// 대기 중인 참회 소모
    /// </summary>
    public float ConsumePendingPenance()
    {
        return statusEffectSystem?.ConsumePendingPenance() ?? 0f;
    }

    /// <summary>
    /// 스턴 발동 시도
    /// </summary>
    public bool TryTriggerStun()
    {
        return statusEffectSystem?.TryTriggerStun() ?? false;
    }

    public virtual void ApplyBurnEffect()
    {
        ProcessTurnStart();
    }

    [Obsolete("GetFreezePenalty() 사용")]
    public float ApplyFreezePenalty(float baseDamage)
    {
        return baseDamage - GetFreezePenalty();
    }

    [Obsolete("GetScarBonus() 사용")]
    public float ApplyBleedBonus(float baseDamage)
    {
        return baseDamage + GetScarBonus();
    }

    [Obsolete("자동 처리됨")]
    public void ApplyActivateBonus(BuffStatType incoming)
    {
        // StatusEffectSystem에서 ApplyEffect 시 자동 처리
    }

    public virtual void ChangeStance(StancType stance) { }

    public virtual void PlayAttackAnimation(int attackType)
    {
        animationHandler?.PlayAttack(attackType);
    }

    public virtual void PlayHitAnimation()
    {
        animationHandler?.PlayHit();
    }

    public virtual void PlayDeathAnimation()
    {
        // TODO: 애니메이션 추가 시 활성화
        // animationHandler?.PlayDeath();

        OnDeathComplete();
    }

    /// <summary>
    /// 사망 애니메이션 완료 후 호출
    /// </summary>
    protected virtual void OnDeathComplete()
    {
        // 하위 클래스에서 구현
    }

    public virtual void CameraActionPlay()
    {
        // 하위 클래스에서 구현
    }


    public void BindHpBar(HpBarDisplay bar)
    {
        hpBarDisplay = bar;
        OnBindHpBar(bar);
    }

    /// <summary>
    /// HP바 바인딩 시 추가 처리 (하위 클래스에서 오버라이드)
    /// </summary>
    protected abstract void OnBindHpBar(HpBarDisplay bar);

    protected virtual void UpdateStatusUI()
    {
        // 하위 클래스에서 적절한 UI 업데이트 호출
    }

    protected virtual void OnStatusEffectsChanged()
    {
        UpdateStatusUI();
    }

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
}