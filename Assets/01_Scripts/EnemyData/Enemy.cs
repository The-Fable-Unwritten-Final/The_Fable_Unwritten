using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IStatusReceiver
{
    [SerializeField]
    private EnemyData enemyData;
    public EnemyData EnemyData { set { enemyData = value; } }

    private float currentHP;
    private bool isIgnited;

    public void WatchEnemyInfo()
    {
        Debug.Log("이름 :: " + enemyData.EnemyName);
        Debug.Log("체력 :: " + enemyData.HP);
        Debug.Log("공격력 :: " + enemyData.ATK);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHP = enemyData.HP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public CharacterClass CharacterClass => CharacterClass.Enemy;

    public DeckModel Deck => null;

    public bool IsIgnited => isIgnited;

    public string CurrentStance => enemyData.currentStance.ToString();

    public void ApplyStatusEffect(StatusEffect effect)
    {
        //버프 및 디버프 로직
    }

    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        // 예시로 방어력 보정 처리
        return baseValue;
    }

    public void TakeDamage(float amount)
    {
        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f); // 방어력으로 피해 감소
        reduced = Mathf.Max(reduced, 0);

        currentHP -= reduced;
        Debug.Log($"{enemyData.EnemyName}가 {reduced}의 피해를 받음! 현재 체력: {currentHP}");

        if (currentHP <= 0)
            Debug.Log($"{enemyData.EnemyName} 사망");
    }
    public void Heal(float amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, enemyData.HP);
        Debug.Log($"{enemyData.EnemyName}가 {amount}의 회복을 받음. 현재 체력: {currentHP}");
    }

    public bool IsAlive() => currentHP > 0;
}
