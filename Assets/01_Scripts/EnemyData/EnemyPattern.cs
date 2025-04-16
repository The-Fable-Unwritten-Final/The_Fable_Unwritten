using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions.Tweens;

public class EnemyPattern : MonoBehaviour
{
    private enum PatternType { None, Attack, Skill }
    private PatternType nextPattern;

    private PlayerController player;
    private Enemy enemy;


    private void Start()
    {
        nextPattern = (PatternType)Random.Range(0, 3); // 처음 패턴도 랜덤 지정

        player = FindObjectOfType<PlayerController>(); // 단일 플레이어 대상
        enemy = GetComponent<Enemy>();
    }
    


    public void ExecuteNextPattern()
    {
        int stanceCount = System.Enum.GetValues(typeof(PlayerData.StancType)).Length;
        enemy.enemyData.currentStance = (EnemyData.StancValue.EStancType)(PlayerData.StancType)Random.Range(0, stanceCount);
        Debug.Log($"[적 자세 변경] 현재 자세: {enemy.enemyData.currentStance}");

        switch (nextPattern)
        {
            case PatternType.None:
                Debug.Log("적이 아무 행동도 하지 않음");
                break;

            case PatternType.Attack:
                BasicAttack();
                break;

            case PatternType.Skill:
                SkillAttack();
                break;
        }

        nextPattern = (PatternType)Random.Range(0, 3); // 다음 행동을 랜덤으로 정함
    }

    void BasicAttack()
    {
        Debug.Log("적이 일반 공격을 했습니다.");
        float damage = enemy.enemyData.ATK;
        PlayerData.StancType enemyStance = (PlayerData.StancType)enemy.enemyData.currentStance;
        if (player != null)
        {
            player.ReceiveAttack(enemyStance, damage);
        }
    }

    void SkillAttack()
    {
        Debug.Log("적이 스킬 공격을 시전합니다!");

        float baseDamage = enemy.enemyData.ATK * 1.5f;
        PlayerData.StancType enemyStance = (PlayerData.StancType)enemy.enemyData.currentStance;

        if (player != null)
        {
            player.ReceiveAttack(enemyStance, baseDamage);
        }
    }


}
