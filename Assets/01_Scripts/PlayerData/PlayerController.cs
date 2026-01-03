using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ReduceNextCardCostEffect;

public class PlayerController : MonoBehaviour, IStatusReceiver
{
    public PlayerData playerData;       //플레이어의 데이타
    public DeckModel deckModel;         //플레이어가 들고 있는 덱
    public bool hasBlock = false;           //방어막 획득 여부
    public bool hasResist { get; set; } = false;          //상태이상 디버프 저항 여부
    private bool isTargetable;              //타겟 가능 여부

    // 스테이지 한 번만 이상실현 사용
    public bool IsIdealUsed { get; private set; }

    // 스테이지 시작 시점(BattleFlowController.StartBattle 등)에서 호출
    public void ResetIdealForStage() => IsIdealUsed = false;
    public void MarkIdealThisStage() => IsIdealUsed = true;

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

    public event System.Action OnTargetableChanged; // 타겟 가능 여부 변경 이벤트



    [SerializeField] private HpBarDisplay hpBarDisplay;
    [SerializeField] private DmgBarDisplay dmgBarDisplay;
    [SerializeField] private TargetArrowDisplay targetArrow; 

    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public void OnClickRefineStance() => ChangeStance(PlayerData.StancType.refine);
    public void OnClickMixStance() => ChangeStance(PlayerData.StancType.mix);
    public void OnClickGraceStance() => ChangeStance(PlayerData.StancType.grace);
    public void OnClickJudgeStance() => ChangeStance(PlayerData.StancType.judge);
    public void OnClickGuardStance() => ChangeStance(PlayerData.StancType.guard);
    public void OnClickRushStance() => ChangeStance(PlayerData.StancType.rush);


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


    /// <summary>
    /// 상태이상 혹은 버프를 적용하여 리스트에 추가
    /// </summary>
    /// <param name="effect">적용할 효과</param>
    public void ApplyStatusEffect(StatusEffect effect)
    {
        // 디버프 적용시 저항 체크
        if (hasResist && Debuff.IsDebuff(effect.statType, effect.value)) return;
        
        //Debug.Log($"[버프 적용] {playerData.CharacterName} 에게 {effect.statType} +{effect.value} ({effect.duration}턴)");
        switch (effect)     
        {
            case TickEffect tick:   //턴 이펙트일 경우
                tickEffects.Add(new TickEffect
                {
                    statType = tick.statType,
                    value = tick.value,
                    duration = tick.duration
                });
                break;

            case InstanceEffect inst:       // 단일 적용인 경우
                var existing = instantEffects.Find(e => e.statType == inst.statType);
                if (existing != null)       
                {
                    // 기존 수치에 누적
                    existing.value = Mathf.Clamp(existing.value + inst.value, 0, 50);
                    existing.isMaintain = existing.isMaintain || inst.isMaintain; // 유지되는 버프가 들어오면 유지로 전환
                }
                else
                {
                    // 새로 추가
                    instantEffects.Add(new InstanceEffect
                    {
                        statType = inst.statType,
                        value = Mathf.Clamp(inst.value, 0, 50),
                        isMaintain = inst.isMaintain
                    });
                }
                break;

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
            //Debug.Log($"[Block] {playerData.CharacterClass.ToString()}의 블록으로 피해 {amount} 무효화");
            return 0;
        }
        // 문체 효과 적용
        amount = StyleManager.Instance.GetOnComingDamageModify(this, this, amount);

        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f);
        reduced = Mathf.Max(reduced, 1f);

        if(playerData.currentStance == PlayerData.StancType.guard)
        {
            reduced = reduced / 2;
        }
        else if (playerData.currentStance == PlayerData.StancType.rush)
        {
            reduced = reduced * 2;
        }

        reduced = Mathf.Round(reduced);

        var dmg = new DmgTextData
        {
            Text = $"{Mathf.RoundToInt(reduced)}",
            type = DmgTextType.Normal,
            isCardEnhanced = false,
            isStanceEnhanced = false,
            isWeakened = false
        };

        this.dmgTextQueue.Enqueue(dmg);

        playerData.currentHP = Mathf.Max(0, playerData.currentHP - reduced);
        //Debug.Log($"{playerData.CharacterName} 피해: {reduced}, 현재 체력: {playerData.currentHP}");

        return reduced;
    }

    /// <summary>
    /// 체력 회복 (grace 수치 만큼 추가)
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        var grace = instantEffects.Find(e => e.statType == BuffStatType.Grace);
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

    
    public void ChangeStance(PlayerData.StancType newStance) //StancUI 함수
    {
        PlayerData.StancType stance = newStance;
        playerData.currentStance = stance;
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
    public void PlayAttackAnimation(int attackType)
    {
        if (animator != null)
        {
            animator.SetInteger("Attack", attackType);
            GameManager.Instance.StartCoroutine(ResetAttackParam(1.5f));
        }
    }

    // 일정 시간 후 Attack 파라미터를 기본값으로 되돌림
    private IEnumerator ResetAttackParam(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetInteger("Attack", -1);
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

    /// <summary>
    /// 빙결 처리
    /// </summary>
    /// <param name="baseDamage"></param>
    /// <returns></returns>
    public float ApplyFreezePenalty(float baseDamage)
    {
        var freeze = instantEffects.Find(e => e.statType == BuffStatType.Freeze);
        if (freeze != null && freeze.value > 0)
        {
            float multiplier = Mathf.Clamp01(1f - freeze.value / 100f);
            float reduced = baseDamage * multiplier;

            Debug.Log($"[Freeze] {playerData.CharacterName} 공격력 {freeze.value}% 감소 → {reduced}");

            // 빙결 효과는 발동 시 수치 초기화
            freeze.value = 0;

            return reduced;
        }

        return baseDamage;
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
            .Where(e => Debuff.IsDebuff(e.statType, e.value) == false &&
                        e.statType != BuffStatType.Bless &&
                        e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetBuff != null)
        {
            targetBuff.value = Mathf.Min(50, targetBuff.value + blessValue);
            Debug.Log($"[Bless] {targetBuff.statType} → +{blessValue} 증가됨");
        }
        else
        {
            // 적용할 버프가 없으면 Bless 수치만 저장
            ApplyStatusEffect(new InstanceEffect
            {
                statType = BuffStatType.Bless,
                value = blessValue,
                isMaintain = false
            });
        }
    }
    /// <summary>
    /// 정화 디버프 처리
    /// </summary>
    /// <param name="purifyValue"></param>
    public void TryApplyPurifyBonus(float purifyValue)
    {
        var targetDebuff = instantEffects
            .Where(e => Debuff.IsDebuff(e.statType, e.value) &&
                        e.statType != BuffStatType.Purify &&
                        e.value > 0)
            .OrderByDescending(e => e.value)
            .FirstOrDefault();

        if (targetDebuff != null)
        {
            float reduction = Mathf.Min(targetDebuff.value, purifyValue);
            targetDebuff.value -= reduction;
            Debug.Log($"[Purify] {targetDebuff.statType} → -{reduction} 감소됨");
        }
        else
        {
            // 적용할 디버프가 없으면 Purify 수치만 저장
            ApplyStatusEffect(new InstanceEffect
            {
                statType = BuffStatType.Purify,
                value = purifyValue,
                isMaintain = false
            });
        }
    }


    /// <summary>
    /// 존재하는 축복 수치 적용
    /// </summary>
    private void TryApplyStoredBlessIfExists(InstanceEffect incoming)
    {
        var bless = instantEffects.Find(e => e.statType == BuffStatType.Bless && !e.isMaintain);
        if (bless != null)
        {
            incoming.value = Mathf.Min(50, incoming.value + bless.value);
            Debug.Log($"[Bless Triggered] {incoming.statType} 수치 증가 +{bless.value}");
            instantEffects.Remove(bless); // 일회성
        }
    }

    /// <summary>
    /// 존재하는 정화 수치 적용
    /// </summary>
    /// <param name="incoming"></param>
    private void TryApplyStoredPurifyIfExists(InstanceEffect incoming)
    {
        var purify = instantEffects.Find(e => e.statType == BuffStatType.Purify && !e.isMaintain);
        if (purify != null)
        {
            incoming.value = Mathf.Max(0, incoming.value - purify.value);
            Debug.Log($"[Purify Triggered] {incoming.statType} 수치 감소 -{purify.value}");
            instantEffects.Remove(purify); // 일회성
        }
    }

    /// <summary>
    /// 출혈 수치 적용
    /// </summary>
    /// <param name="baseDamage"></param>
    /// <returns></returns>
    public float ApplyBleedBonus(float baseDamage)
    {
        var bleed = instantEffects.Find(e => e.statType == BuffStatType.Bleed);
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

    /// <summary>
    /// 스턴 적용
    /// </summary>
    /// <returns></returns>
    public bool TryTriggerStun()
    {
        var stun = instantEffects.Find(e => e.statType == BuffStatType.Stun);
        if (stun != null && stun.value > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= stun.value)
            {
                Debug.Log($"[Stun] {playerData.CharacterName} 기절 발동 → 1턴 행동 불가");
                ApplyStatusEffect(new TickEffect
                {
                    statType = BuffStatType.Stun,
                    value = 1,
                    duration = 1
                });
                stun.value = 0;
                return true;
            }
        }
        return false;
    }

    public bool TryTriggerGuardRedirect(out PlayerController redirectTarget)
    {
        redirectTarget = null;

        var guard = instantEffects.Find(e => e.statType == BuffStatType.GuardRedirect);
        if (guard != null && guard.value > 0)
        {
            float roll = Random.Range(0f, 100f);
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

}
