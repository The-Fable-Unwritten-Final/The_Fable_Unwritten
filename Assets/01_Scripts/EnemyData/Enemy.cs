using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IStatusReceiver
{
    public EnemyData enemyData;

    public bool hasBlock = false;


    private List<StatusEffect> activeEffects = new List<StatusEffect>();  //현재 가지고 있는 상태이상 및 버프

    void Start()
    {
        //enemyData.CurrentHP = enemyData.MaxHP; //전투 시작시 EnemyHP 풀로 채우기 
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

    
    public bool IsStunned() => false;           //스턴 상태 확인

    private CharacterClass characterClass = CharacterClass.Enemy;
    public CharacterClass ChClass
    {
        get => characterClass;
        set => characterClass = value;
    }

    public DeckModel Deck => null;

    public bool IsIgnited => false;

    public string CurrentStance => enemyData.currentStance.ToString();



    public void TakeDamage(float amount)
    {
        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f); // 방어력으로 피해 감소
        reduced = Mathf.Max(reduced, 0);

        enemyData.CurrentHP -= reduced;
        Debug.Log($"{enemyData.EnemyName}가 {reduced}의 피해를 받음! 현재 체력: {enemyData.CurrentHP}");

        if (enemyData.CurrentHP <= 0)
            Debug.Log($"{enemyData.EnemyName} 사망");
    }

    public void CameraActionPlay()
    {
        GameManager.Instance.combatCameraController.CameraZoomInAction(transform);
    }
}
