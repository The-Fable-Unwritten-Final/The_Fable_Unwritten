using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Enemy의 턴마다 공격 또는 스킬을 실행하는 패턴 스크립트
/// CSV → EnemyParsed 데이터로부터 스탯, 스킬을 초기화
/// IStatusReceiver(PlayerController)의 ReceiveAttack/ApplyStatusEffect로 효과를 전달
/// </summary>
public class EnemyPattern : MonoBehaviour
{
    private enum PatternType { BasicAttack, SkillAttack }

    [Header("References")]
    [SerializeField] private PlayerController player;  // 전투 대상, 인스펙터에서 할당 가능

    private Enemy enemy;
    private EnemyData enemyData;
    private EnemyParsed parsed;
    private List<EnemySkill> skillList;
    private PatternType currentPattern;

    // StanceType 개수 (한 번만 계산)
    private static readonly int stanceCount =
        System.Enum.GetValues(typeof(PlayerData.StancType)).Length;

    /// <summary>
    /// Awake 단계에서 CSV 매니저를 초기화 (한번만)
    /// </summary>
    private void Awake()
    {
        EnemyParseManager.Initialize("Assets/Resources/ExternalFiles/EnemyData.csv");
    }

    /// <summary>
    /// Start 단계에서 컴포넌트/데이터 연결, 스탯 & 스킬 초기화, 초기 패턴을 설정
    /// </summary>
    private void Start()
    {
        // 1) 컴포넌트 & 레퍼런스 연결
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        enemy = GetComponent<Enemy>();
        enemyData = enemy?.enemyData;
        if (player == null || enemyData == null)
        {
            Debug.LogError("[EnemyPattern] PlayerController 또는 EnemyData 누락");
            enabled = false;
            return;
        }

        // 2) CSV 파싱 데이터 가져오기
        if (!EnemyParseManager.ParsedDict.TryGetValue(enemyData.IDNum, out parsed))
        {
            Debug.LogError($"[EnemyPattern] ID={enemyData.IDNum}에 해당하는 데이터 없음");
            enabled = false;
            return;
        }

        // 3) 스탯, 스킬 초기화
        InitializeStatsAndSkills();

        // 4) 초기 행동 패턴 무작위 설정
        currentPattern = (PatternType)Random.Range(0, 2);
    }

    /// <summary>
    /// EnemyData와 스킬 리스트를 CSV 파싱 데이터로 초기화
    /// </summary>
    private void InitializeStatsAndSkills()
    {
        // 스탯 적용
        enemyData.MaxHP = parsed.hp;
        enemyData.CurrentHP = parsed.hp;
        enemyData.ATK = Mathf.RoundToInt(parsed.atkBuff);
        enemyData.DEF = parsed.defBuff;

        // 스킬 리스트 구성
        skillList = new List<EnemySkill>();
        AddParsedSkill(parsed.skill0, parsed.damage0, parsed.percentage0);
        AddParsedSkill(parsed.skill1, parsed.damage1, parsed.percentage1);
        AddParsedSkill(parsed.skill2, parsed.damage2, parsed.percentage2);
    }

    /// <summary>
    /// 외부(턴 매니저 등)에서 호출해 몬스터의 다음 행동을 실행
    /// </summary>
    public void ExecutePattern()
    {
        // 1) 자세 랜덤 변경
        SetRandomStance();

        // 2) 행동 패턴 실행
        switch (currentPattern)
        {
            case PatternType.BasicAttack:
                Attack(enemyData.ATK);
                break;
            case PatternType.SkillAttack:
                Attack(enemyData.ATK);
                ExecuteRandomSkill();
                break;
        }

        // 3) 다음 턴을 위해 패턴 재선택
        currentPattern = (PatternType)Random.Range(0, 2);
    }

    /// <summary>
    /// 랜덤한 StanceType을 설정하고 로그를 출력
    /// </summary>
    private void SetRandomStance()
    {
        var stance = (PlayerData.StancType)Random.Range(0, stanceCount);
        enemyData.currentStance = (EnemyData.StancValue.EStancType)stance;
        Debug.Log($"[EnemyPattern] {enemyData.EnemyName} 자세 → {stance}");
    }

    /// <summary>
    /// 지정된 데미지를 플레이어에게 전달
    /// ReceiveAttack 내부에서 자세별 데미지 배율/회피 처리가 일어난다.
    /// </summary>
    /// <param name="damage">전달할 데미지</param>
    private void Attack(float damage)
    {
        player.ReceiveAttack(
            (PlayerData.StancType)enemyData.currentStance,
            damage);
        Debug.Log($"[EnemyPattern] 공격: {damage} 피해");
    }

    /// <summary>
    /// 스킬 리스트에서 랜덤한 스킬을 선택해 추가 효과를 적용합니다.
    /// </summary>
    private void ExecuteRandomSkill()
    {
        if (skillList.Count == 0)
        {
            Debug.Log("[EnemyPattern] 적용할 스킬이 없습니다.");
            return;
        }

        var skill = skillList[Random.Range(0, skillList.Count)];
        ApplyParsedSkill(skill);
    }

    /// <summary>
    /// parsedData에 정의된 스킬 정보를 EnemySkill로 변환해 리스트에 추가
    /// </summary>
    /// <param name="idx">스킬 인덱스</param>
    /// <param name="dmg">기본 데미지 값</param>
    /// <param name="pct">퍼센티지 보정 값</param>
    private void AddParsedSkill(int idx, float dmg, float pct)
    {
        if (idx <= 0) return;
        skillList.Add(new EnemySkill
        {
            skillIndex = idx,
            damage = dmg,
            percentage = pct
        });
    }

    /// <summary>
    /// 선택된 EnemySkill을 기반으로 추가 데미지를 계산하여 공격
    /// </summary>
    /// <param name="sk">적의 스킬 데이터</param>
    private void ApplyParsedSkill(EnemySkill sk)
    {
        float extraDamage = sk.damage * sk.percentage;
        Attack(extraDamage);
        Debug.Log($"[EnemyPattern] 스킬#{sk.skillIndex} 추가 데미지: {extraDamage}");
    }
}


//public void ExecuteNextPattern()
//{
//    int stanceCount = System.Enum.GetValues(typeof(PlayerData.StancType)).Length;
//    enemy.enemyData.currentStance = (EnemyData.StancValue.EStancType)(PlayerData.StancType)Random.Range(0, stanceCount);
//    Debug.Log($"[적 자세 변경] 현재 자세: {enemy.enemyData.currentStance}");

//    switch (nextPattern)
//    {
//        case PatternType.None:
//            Debug.Log("적이 아무 행동도 하지 않음");
//            break;

//        case PatternType.Attack:
//            BasicAttack();
//            break;

//        case PatternType.Skill:
//            SkillAttack();
//            break;
//    }

//    nextPattern = (PatternType)Random.Range(0, 3); // 다음 행동을 랜덤으로 정함
//}

//void BasicAttack()
//{
//    Debug.Log("적이 일반 공격을 했습니다.");
//    float damage = enemy.enemyData.ATK;
//    PlayerData.StancType enemyStance = (PlayerData.StancType)enemy.enemyData.currentStance;
//    if (player != null)
//    {
//        player.ReceiveAttack(enemyStance, damage);
//    }
//}

//void SkillAttack()
//{
//    Debug.Log("적이 스킬 공격을 시전합니다!");

//    float baseDamage = enemy.enemyData.ATK * 1.5f;
//    PlayerData.StancType enemyStance = (PlayerData.StancType)enemy.enemyData.currentStance;

//    if (player != null)
//    {
//        player.ReceiveAttack(enemyStance, baseDamage);
//    }
//}



