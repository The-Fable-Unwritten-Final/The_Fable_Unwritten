using UnityEngine;

/// <summary>
/// 적 유닛 클래스 (UnitBase 상속)
/// </summary>
public class Enemy : UnitBase
{
    [Header("Enemy Data")]
    public EnemyData enemyData;
    private CharacterClass characterClass = CharacterClass.Enemy;

    public override float maxHP
    {
        get => enemyData.MaxHP;
        set => enemyData.MaxHP = value;
    }

    public override float currentHP
    {
        get => enemyData.CurrentHP;
        set => enemyData.CurrentHP = value;
    }

    public override CharacterClass ChClass
    {
        get => characterClass;
        set => characterClass = value;
    }

    public override DeckModel Deck => null;

    public override bool IsIgnited => false;

    public override string CurrentStance => enemyData.currentStance.ToString();

    // Animator/SpriteRenderer 직접 접근 (하위 호환성)
    public Animator animator => GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
    public SpriteRenderer spriteRenderer => GetComponentInChildren<SpriteRenderer>() ?? GetComponent<SpriteRenderer>();

    protected override void Awake()
    {
        base.Awake();

        // 애니메이션 컨트롤러 설정
        if (enemyData?.animationController != null)
        {
            SetAnimatorController(enemyData.animationController);
        }
        else
        {
            Debug.LogWarning($"[{name}] enemyData 또는 AnimationController가 누락되었습니다.");
        }
    }

    /// <summary>
    /// 적 데이터 설정
    /// </summary>
    public void SetData(EnemyData data)
    {
        enemyData = data;
        enemyData.CurrentHP = enemyData.MaxHP;

        // 애니메이션 컨트롤러 설정
        if (enemyData.animationController != null)
        {
            SetAnimatorController(enemyData.animationController);
        }

        // HP바 바인딩
        if (hpBarDisplay != null)
        {
            hpBarDisplay.BindEnemyData(enemyData);
        }

        // 보스 및 엘리트의 HP바 크기 조절
        AdjustHpBarSize();
    }

    private void AdjustHpBarSize()
    {
        if (hpBarDisplay == null) return;

        var rt = hpBarDisplay.GetComponent<RectTransform>();
        if (rt == null) return;

        var scale = rt.localScale;
        scale.x = (enemyData.type == EnemyType.elite || enemyData.type == EnemyType.boss)
            ? 1.8f
            : 1.1f;
        rt.localScale = scale;
    }
    protected override float GetMinimumDamage() => 0f;

    protected override void OnBindHpBar(HpBarDisplay bar)
    {
        bar?.BindEnemyData(enemyData);
    }

    protected override void UpdateStatusUI()
    {
        statusDisplay?.EnemyUpdateUI();
    }

    public override void TickStatusEffects()
    {
        base.TickStatusEffects();
        statusDisplay?.EnemyUpdateUI();
    }

    public override void UpdateHpStatus()
    {
        // enemyData에서 직접 읽으므로 별도 처리 불필요
    }

    /// <summary>
    /// HP바 위치 조절용
    /// </summary>
    public void UpdateHpBarFollowTarget()
    {
        if (hpBarDisplay != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                hpBarDisplay.FollowTarget(sr);
        }
    }

    protected override void Die()
    {
        base.Die();

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
    }

    protected override void OnDeathComplete()
    {
        gameObject.SetActive(false);
    }
}