using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using UnityEngine;

public class Enemy : MonoBehaviour, IStatusReceiver
{
    public EnemyData enemyData;

    public bool hasBlock = false;
    private bool isTargetable;

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

    public event System.Action OnTargetableChanged;

    [SerializeField] private HpBarDisplay hpBarDisplay;
    [SerializeField] private DmgBarDisplay dmgBarDisplay;
    [SerializeField] private TargetArrowDisplay targetArrow;

    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private StatusDisplay statusDisplay;

    [SerializeField] public List<TickEffect> tickEffects = new();       // 턴마다 지속되는 효과
    [SerializeField] public List<InstanceEffect> instantEffects = new(); // 즉시 적용 효과

    private void Awake()
    {

        statusDisplay = GetComponentInChildren<StatusDisplay>();

        if (enemyData != null && enemyData.animationController != null)
        {
            animator.runtimeAnimatorController = enemyData.animationController;
        }
        else
        {
            Debug.LogWarning($"[{name}] enemyData 또는 AnimationController가 누락되었습니다.");
        }
    }
    void Start()
    {
        targetArrow.Init(this); // 옵저버 연결
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;

        enemyData.CurrentHP = enemyData.MaxHP;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyData.animationController != null && animator != null)
            animator.runtimeAnimatorController = enemyData.animationController;

        if (hpBarDisplay != null)
        {
            hpBarDisplay.BindEnemyData(enemyData);
        }
        
        // 보스 및 엘리트의 HP바 크기 조절
            if (enemyData.type == EnemyType.elite || enemyData.type == EnemyType.boss)
            {
                var rt = hpBarDisplay.GetComponent<RectTransform>();
                var s = rt.localScale;
                s.x = 1.8f;
                rt.localScale = s;
            }
            else
            {
                var rt = hpBarDisplay.GetComponent<RectTransform>();
                var s = rt.localScale;
                s.x = 1.1f;
                rt.localScale = s;
            }
    }

    /// <summary>
    /// 상태이상 혹은 버프를 적용하여 리스트에 추가
    /// </summary>
    /// <param name="effect">적용할 효과</param>
    public void ApplyStatusEffect(StatusEffect effect)
    {
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


    public void TakeTrueDamage(float damage)
    {
        //Debug.Log($"{enemyData.EnemyName}가 {damage}의 트루데미지를 받음! 현재 체력: {enemyData.CurrentHP}");
        currentHP -= damage;
    }

    /// <summary>
    /// 턴 종료 시 버프 감소 용
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

        /*for (int i = instantEffects.Count - 1; i >= 0; i--)
        {
            if (!instantEffects[i].isMaintain)
            {
                Debug.Log($"[InstanceEffect 제거] {instantEffects[i].statType}");
                instantEffects.RemoveAt(i);
            }
        }*/ //턴 종료 시 자동으로 사라지는 메서드이기 때문에 필요할 시 살리기

        statusDisplay?.EnemyUpdateUI();
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


    public void BindHpBar(HpBarDisplay bar)
    {
        hpBarDisplay = bar;
        hpBarDisplay.BindEnemyData(enemyData);
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
    /// 체력 회복
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        enemyData.CurrentHP = Mathf.Min(enemyData.MaxHP, enemyData.CurrentHP + amount);
        Debug.Log($"{enemyData.EnemyName} 회복: {amount}, 현재 체력: {enemyData.CurrentHP}");
    }


    /// <summary>
    /// 생존 여부 확인
    /// </summary>
    /// <returns>체력이 0 초과인지 여부</returns>
    public bool IsAlive()
    {
        return enemyData.CurrentHP > 0;
    }


    public bool IsStunned()
    {
        return HasEffect(BuffStatType.Stun);
    }

    private CharacterClass characterClass = CharacterClass.Enemy;
    public CharacterClass ChClass
    {
        get => characterClass;
        set => characterClass = value;
    }

    public DeckModel Deck => null;

    public bool IsIgnited => false;

    public string CurrentStance => enemyData.currentStance.ToString();

    public Transform CachedTransform => transform;

    [SerializeField] private DmgBarQueueHandler queue;
    public DmgBarQueueHandler dmgTextQueue => queue;

    public float TakeDamage(float amount)
    {
        if (hasBlock)
        {
            hasBlock = false;
            //Debug.Log($"[Block] {enemyData.EnemyName}의 블록으로 피해 {amount} 무효화");
            return 0;
        }

        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f); // 방어력으로 피해 감소
        reduced = Mathf.Max(reduced, 0);

        enemyData.CurrentHP -= reduced;
        //Debug.Log($"{enemyData.EnemyName}가 {reduced}의 피해를 받음! 현재 체력: {enemyData.CurrentHP}");

        return reduced;
    }

    public void CameraActionPlay()
    {
        //GameManager.Instance.combatCameraController.CameraZoomInAction(transform);
    }

    // 최대 체력
    public float maxHP
    {
        get => enemyData.MaxHP;
        set => enemyData.MaxHP = value;
    }
    //현재 체력

    public float currentHP
    {
        get => enemyData.CurrentHP;
        set => enemyData.CurrentHP = value;
    }

    //체력 변화 시
    public void UpdateHpStatus()
    {
        maxHP = enemyData.MaxHP;
        currentHP = enemyData.CurrentHP;
    }

    //HP바 위치 조절용
    public void UpdateHpBarFollowTarget()
    {
        if (hpBarDisplay != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                hpBarDisplay.FollowTarget(sr);
        }
    }

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

    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("Hit", true);
            GameManager.Instance.StartCoroutine(ResetBool("Hit", 1f));
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
    /// 현재 적용 중인 공격력 버프 총합 반환
    /// </summary>
    public float GetBuffAtk()
    {
        return ModifyStat(BuffStatType.Attack, 0f);
    }

    /// <summary>
    /// 현재 적용 중인 방어력 버프 총합 반환
    /// </summary>
    public float GetBuffDef()
    {
        return ModifyStat(BuffStatType.Defense, 0f);
    }

    public DmgBarDisplay dmgBar => dmgBarDisplay;
    public TargetArrowDisplay tarArrow => targetArrow;

}
