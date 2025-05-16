using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IStatusReceiver
{
    public EnemyData enemyData;

    public bool hasBlock = false;

    [SerializeField] private HpBarDisplay hpBarDisplay;

    private Animator animator;
    private StatusDisplay statusDisplay;

    [SerializeField]public List<StatusEffect> activeEffects = new List<StatusEffect>();  //현재 가지고 있는 상태이상 및 버프

    private void Awake()
    {

        animator = GetComponent<Animator>();
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
        
        //enemyData.CurrentHP = enemyData.MaxHP; //전투 시작시 EnemyHP 풀로 채우기  //예외 처리를 해주면 된다.
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;

        enemyData.CurrentHP = enemyData.MaxHP;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyData.animationController != null && animator != null)
            animator.runtimeAnimatorController = enemyData.animationController;

        if (hpBarDisplay != null){
            hpBarDisplay.BindEnemyData(enemyData);
        }
    }

    /// <summary>
    /// 상태이상 혹은 버프를 적용하여 리스트에 추가
    /// </summary>
    /// <param name="effect">적용할 효과</param>
    public void ApplyStatusEffect(StatusEffect effect)
    {
        Debug.Log($"[버프 적용] {enemyData.EnemyName} 에게 {effect.statType} +{effect.value} ({effect.duration}턴)");
        activeEffects.Add(new StatusEffect
        {
            statType = effect.statType,
            value = effect.value,
            duration = effect.duration
        });

        statusDisplay?.EnemyUpdateUI();
    }

    public void TakeTrueDamage(float damage)
    {
        Debug.Log($"{enemyData.EnemyName}가 {damage}의 트루데미지를 받음! 현재 체력: {enemyData.CurrentHP}");
        currentHP -= damage;
    }

    /// <summary>
    /// 턴 종료 시 버프 감소 용
    /// </summary>
    public void TickStatusEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].duration--;
            if (activeEffects[i].duration <= 0)
            {
                Debug.Log($"[버프 종료] {enemyData.EnemyName} 의 {activeEffects[i].statType} 효과 종료");
                activeEffects.RemoveAt(i);
            }
        }
        statusDisplay?.EnemyUpdateUI();
    }
    /// <summary>
    /// 특정 타입의 버프/디버프가 있는지 확인
    /// </summary>
    /// <param name="type">스탯 타입</param>
    /// <returns>존재 여부</returns>
    public bool HasEffect(BuffStatType type)
    {
        return activeEffects.Exists(e => e.statType == type && e.duration > 0);
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
        float modifiedValue = baseValue;
        foreach (var effect in activeEffects)
        {
            if (effect.statType == statType)
                modifiedValue += effect.value;
        }
        return modifiedValue;
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
        return activeEffects.Exists(e => e.statType == BuffStatType.stun && e.duration > 0);
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

    public void TakeDamage(float amount)
    {
        if (hasBlock)
        {
            hasBlock = false;
            Debug.Log($"[Block] {enemyData.EnemyName}의 블록으로 피해 {amount} 무효화");
            return;
        }

        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f); // 방어력으로 피해 감소
        reduced = Mathf.Max(reduced, 0);

        enemyData.CurrentHP -= reduced;
        Debug.Log($"{enemyData.EnemyName}가 {reduced}의 피해를 받음! 현재 체력: {enemyData.CurrentHP}");

        if (enemyData.CurrentHP <= 0)
        {
            Debug.Log($"{enemyData.EnemyName} 사망");

            gameObject.SetActive(false); // ▶ 사망 시 비활성화

            ProgressDataManager.Instance.CurrentExp += enemyData.exp;
            GameManager.Instance.turnController.battleFlow.totalExp += enemyData.exp;

            // 💡 전투 종료 체크
            if (GameManager.Instance != null && GameManager.Instance.turnController.battleFlow != null)
            {
                GameManager.Instance.turnController.battleFlow.CheckBattleEnd();
            }
        }
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

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("Attack", true);
            GameManager.Instance.StartCoroutine(ResetBool("Attack", 1f));
        }
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
    /// 해당 자세로 공격 가능한지 확인
    /// </summary>
    /// <param name="stance">공격 가능한지 확인하는 함수</param>
    /// <returns></returns>
    public bool IsAttackBlockedByStance(StancValue.EStancType stance)
    {
        var blockEffects = activeEffects.FindAll(e => e.statType == BuffStatType.CantAttackInStance);

        foreach (var effect in blockEffects)
        {
            if ((int)stance == (int)effect.value)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 현재 적용 중인 공격력 버프 총합 반환
    /// </summary>
    public float GetBuffAtk()
    {
        float atkTotal = 0;
        foreach (var effect in activeEffects)
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
        foreach (var effect in activeEffects)
        {
            if (effect.statType == BuffStatType.Defense)
                defTotal += effect.value;
        }
        return defTotal;
    }
}
