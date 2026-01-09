using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 캐릭터 컨트롤러 (UnitBase 상속)
/// </summary>
public class PlayerController : UnitBase
{

    [Header("Player Data")]
    [SerializeField] private PlayerData playerData;

    [Header("Stance UI")]
    [SerializeField] private Button stanceToggleButton;

    private StanceSystem stanceSystem;
    private CombatInteractionHandler combatHandler;
    private DeckModel deckModel;

    public override float maxHP
    {
        get => playerData.MaxHP;
        set => playerData.MaxHP = value;
    }

    public override float currentHP
    {
        get => playerData.currentHP;
        set => playerData.currentHP = value;
    }

    public override CharacterClass ChClass { get; set; }

    public override DeckModel Deck => deckModel;

    public override bool IsIgnited => false; // 추후 확장

    public override string CurrentStance => stanceSystem?.GetStanceString() ?? playerData.currentStance.ToString();

    public PlayerData PlayerData => playerData;

    // 스탠스 시스템 접근
    public bool IsIdealUsed => stanceSystem?.IsIdealUsed ?? false;
    public PotentialGauge potentialGauge => stanceSystem?.Gauge;
    public StanceEffectData stanceEffectData => stanceSystem?.EffectData;

    // Animator/SpriteRenderer 직접 접근 (하위 호환성)
    public Animator animator => transform.Find("Visual")?.GetComponent<Animator>();
    public SpriteRenderer spriteRenderer => transform.Find("Visual")?.GetComponent<SpriteRenderer>();


    protected override void Awake()
    {
        base.Awake();

        // 애니메이션 컨트롤러 설정
        if (playerData?.animationController != null)
        {
            SetAnimatorController(playerData.animationController);
        }
        else
        {
            Debug.LogWarning($"[{name}] PlayerData 또는 AnimationController가 누락되었습니다.");
        }
    }

    private void OnEnable()
    {
        if (stanceToggleButton != null)
            stanceToggleButton.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (stanceToggleButton != null)
            stanceToggleButton.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        stanceSystem?.Dispose();
    }

    /// <summary>
    /// 플레이어 설정 (전투 시작 시 호출)
    /// </summary>
    public void Setup(PlayerData data)
    {
        playerData = data;
        ChClass = data.CharacterClass;

        // 덱 초기화
        InitializeDeck(data);

        // 시스템 초기화
        InitializePlayerSystems();

        // 부활 처리
        if (!IsAlive())
            playerData.ReviveIfDead();

        // HP바 바인딩
        if (hpBarDisplay != null)
            hpBarDisplay.BindPlayerData(playerData);
    }

    /// <summary>
    /// 기존 Initialize 메서드 (하위 호환성)
    /// </summary>
    public void Initialize(PlayerData data, CharacterClass charClass)
    {
        playerData = data;
        ChClass = charClass;
        deckModel = new DeckModel();

        InitializePlayerSystems();
    }

    private void InitializeDeck(PlayerData data)
    {
        deckModel = new DeckModel();

        if (data.currentDeck == null || data.currentDeck.Count != 5)
            data.ResetDeckIndexesToDefault();

        data.LoadDeckFromIndexes(DataManager.Instance.AllCards);
        deckModel.Initialize(data.currentDeck);
    }

    private void InitializePlayerSystems()
    {
        // StanceSystem 초기화
        stanceSystem = new StanceSystem(playerData);
        stanceSystem.OnStanceEffectTriggered += OnStanceEffectTriggered;

        // CombatInteractionHandler 초기화
        combatHandler = new CombatInteractionHandler(
            statusEffectSystem,
            () => GameManager.Instance?.turnController?.battleFlow,
            ChClass
        );
    }

    protected override float GetMinimumDamage() => 1f;

    protected override float ApplyStanceModifierToDamage(float damage)
    {
        return playerData.currentStance switch
        {
            StancType.Defense => damage / 2f,
            StancType.Rush => damage * 2f,
            _ => damage
        };
    }

    protected override float ApplyHealModifiers(float amount)
    {
        // 스탠스 보너스 적용
        amount = stanceSystem?.ApplyHealBonus(amount) ?? amount;
        return amount;
    }
    protected override void OnBindHpBar(HpBarDisplay bar)
    {
        bar?.BindPlayerData(playerData);
    }

    protected override void UpdateStatusUI()
    {
        statusDisplay?.PlayerUpdateUI();
    }

    public override void TickStatusEffects()
    {
        base.TickStatusEffects();
        stanceSystem?.OnTurnEnd();
        statusDisplay?.PlayerUpdateUI();
    }

  

    public override void ChangeStance(StancType newStance)
    {
        stanceSystem?.ChangeStance(newStance);
    }

    public void ResetIdealForStage() => stanceSystem?.ResetIdealForStage();

    public void MarkIdealThisStage() => stanceSystem?.MarkIdealUsed();

    public float GetStanceModifiedDamage(float baseDamage)
    {
        return stanceSystem?.ApplyAttackBonus(baseDamage) ?? baseDamage;
    }

    public bool ShouldDoubleCardEffect()
    {
        return stanceSystem?.ShouldDoubleCardEffect() ?? false;
    }

    public void OnAllyUsedCard(PlayerController cardUser)
    {
        stanceSystem?.OnAllyUsedCard(cardUser == this);
    }

  
    public bool TryTriggerGuardRedirect(out PlayerController redirectTarget)
    {
        redirectTarget = null;
        return combatHandler?.TryGuardRedirect(out redirectTarget) ?? false;
    }

    /// <summary>
    /// 축복 적용
    /// </summary>
    public void ApplyBless(float blessValue)
    {
        var effect = new InstanceEffect
        {
            statType = BuffStatType.Bless,
            value = blessValue
        };
        statusEffectSystem?.ApplyEffect(effect);
    }
    /// <summary>
    /// 참회 적용
    /// </summary>
    public void ApplyPenance(float penanceValue)
    {
        var effect = new InstanceEffect
        {
            statType = BuffStatType.Penance,
            value = penanceValue
        };
        statusEffectSystem?.ApplyEffect(effect);
    }

    public void OnBattleEnd()
    {
        stanceSystem?.OnBattleEnd();
    }

    public override void CameraActionPlay()
    {
        if (GameManager.Instance?.combatCameraController == null)
        {
            Debug.LogError("CameraController is not initialized.");
            return;
        }

        // GameManager.Instance.combatCameraController.CameraZoomInAction(transform);
    }

    public void PrintDeckState()
    {
        Debug.Log($"[{playerData.CharacterName}] Hand: {Deck.Hand.Count}, Used: {Deck.UsedCount()}");
    }

    private void OnStanceEffectTriggered()
    {
        var battleFlow = GameManager.Instance?.turnController?.battleFlow;
        if (battleFlow != null)
        {
            StanceEffectHandler.TriggerStanceEffect(this, battleFlow);
        }
    }

    protected override void Die()
    {
        base.Die();

        if (stanceToggleButton != null)
            stanceToggleButton.gameObject.SetActive(false);
    }

    protected override void OnDeathComplete()
    {
        // 오브젝트 비활성화 (삭제가 아닌 비활성화 - 부활 가능성 대비)
        gameObject.SetActive(false);
    }
}