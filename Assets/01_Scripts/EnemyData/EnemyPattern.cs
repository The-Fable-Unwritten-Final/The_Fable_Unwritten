using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections;
public class EnemyPattern : MonoBehaviour
{
    // 행동 패턴 종류: 기본 공격과 스킬 공격
    private enum PatternType { BasicAttack, SkillAttack }
    private PatternType currentPattern;

    private PlayerController player;    // 단일 플레이어 대상
    private Enemy enemy;                // 자기 자신에 붙은 Enemy 스크립트
    private EnemyData enemyData;


    private EnemyAct[] globalSkillEffects; //전역 스킬 효과 에셋 (Resources/Enemy/Skills)
    private void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("[EnemyBattlePattern] Enemy 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        enemyData = enemy.enemyData;
        if (enemyData == null)
        {
            Debug.LogError("[EnemyBattlePattern] EnemyData가 할당되지 않았습니다.");
            return;
        }
        player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("[EnemyBattlePattern] PlayerController를 찾지 못했습니다.");
        }

        // 전역 스킬 효과 에셋 로드 
        globalSkillEffects = Resources.LoadAll<EnemyAct>("Enemy/Skills");
        if (globalSkillEffects == null || globalSkillEffects.Length == 0)
            Debug.LogWarning("[EnemyBattlePattern] 전역 EnemyAct 에셋이 없습니다.");

        // 행동 패턴 초기 선택: 기본 공격과 스킬 공격 중 무작위로 결정
        currentPattern = (PatternType)Random.Range(0, 2);
    }

    /// <summary>
    /// 외부(예: 턴 매니저)에서 호출해 적의 다음 행동을 실행합니다.
    /// </summary>

    public void ExecutePattern()
    {
        // 턴마다 적의 자세를 무작위로 변경합니다.
        int stanceCount = System.Enum.GetValues(typeof(PlayerData.StancType)).Length;
        enemyData.currentStance = (EnemyData.StancValue.EStancType)(PlayerData.StancType)Random.Range(0, stanceCount);
        Debug.Log($"[EnemyBattlePattern] {enemyData.EnemyName}의 현재 자세: {enemyData.currentStance}");

        // 선택된 행동 패턴에 따라 행동 실행
        switch (currentPattern)
        {
            case PatternType.BasicAttack:
                ExecuteBasicAttack();
                break;
            case PatternType.SkillAttack:
                ExecuteSkillAttack();
                break;
        }

        currentPattern = (PatternType)Random.Range(0, 2);
    }


    /// <summary>
    /// 기본 공격: EnemyData에 정의된 ATK 값을 그대로 플레이어에게 전달
    /// PlayerController의 ReceiveAttack 메서드는 내부에서 플레이어 자세에 따라 데미지를 변환
    /// </summary>
    private void ExecuteBasicAttack()
    {
        float damage = enemyData.ATK;
        player.ReceiveAttack((PlayerData.StancType)enemyData.currentStance, damage);
        Debug.Log($"[EnemyBattlePattern] 기본 공격 실행, 데미지: {damage}");
    }

    /// <summary>
    /// 스킬 공격: 기본 공격과 동일한 ATK 값을 적용하고, 추가로 상태 효과(디버프/스턴)를 플레이어에게 적용합니다.
    /// </summary>
    private void ExecuteSkillAttack()
    {
        float damage = enemyData.ATK;
        player.ReceiveAttack((PlayerData.StancType)enemyData.currentStance, damage);
        Debug.Log($"[EnemyBattlePattern] 스킬 공격 실행, 기본 데미지: {damage}");
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


}
