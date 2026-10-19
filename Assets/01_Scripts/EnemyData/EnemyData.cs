using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public event Action<float, float> OnHpChanged; // 체력 변경 시 호출할 이벤트 추가

    [SerializeField] private int idNum;
    public int IDNum { get => idNum; set => idNum = value; }

    [SerializeField] private string enemyName;
    public string EnemyName { get => enemyName; set => enemyName = value; }

    public int exp;

    [SerializeField] private int maxHP;

    public int MaxHP
    {
        get => maxHP;
        set
        {
            maxHP = value;
            OnHpChanged?.Invoke(currentHP, maxHP); // 최대 체력 변경 시 알림
        }
    }

    [SerializeField] private int currentHP;
    public int CurrentHP
    {
        get => currentHP;
        set
        {
            currentHP = Mathf.Clamp(value, 0, MaxHP);
            OnHpChanged?.Invoke(currentHP, MaxHP); // 현재 체력 변경 시 알림
        }
    }

    [SerializeField] private float aTKValue;        //기본 공방 버프로 써야겠다
    public float ATKValue { get => aTKValue; set => aTKValue = value; }

    [SerializeField] private float dEFValue;
    public float DEFValue { get => dEFValue; set => dEFValue = value; }

    public bool stun;

    public bool blind;

    public bool block;

    public string note;

    public CharacterAnchorOffsetData anchorData;

    public EnemyType type;

    [Header("스킬 목록")]
    [SerializeField] private List<EnemySkill> skillList = new();
    public List<EnemySkill> SkillList
    {
        get => skillList;
        set => skillList = value ?? new List<EnemySkill>();
    }

    public string illust;       //캐릭터 이미지 이름

    public StancValue.EStancType currentStance;


    public RuntimeAnimatorController animationController;

    public string AttackSkillEffect;
    public string AllySkillEffect;

    public float TopStance, MiddleStance, BottomStance;

    public List<int> loot = new();

    // 스킬 추가
    public void AddSkill(EnemySkill skill)
    {
        if (skill != null && !skillList.Contains(skill))
            skillList.Add(skill);
    }

    // 스킬 모두 삭제
    public void ClearSkills() => skillList.Clear();


    [Header("Stage Scaling")]
    [SerializeField] private float hpScale2 = 1f;
    [SerializeField] private float damageHealScale2 = 1f;
    [SerializeField] private float hpScale3 = 1f;
    [SerializeField] private float damageHealScale3 = 1f;

    public float HpScale2
    {
        get => hpScale2;
        set => hpScale2 = value;
    }

    public float DamageHealScale2
    {
        get => damageHealScale2;
        set => damageHealScale2 = value;
    }

    public float HpScale3
    {
        get => hpScale3;
        set => hpScale3 = value;
    }

    public float DamageHealScale3
    {
        get => damageHealScale3;
        set => damageHealScale3 = value;
    }

    [System.NonSerialized]
    public float CurrentDamageHealScale = 1f;

    public void ApplyStageScale(int stage)
    {
        float hpScale = 1f;
        CurrentDamageHealScale = 1f;

        switch (stage)
        {
            case 3:
                hpScale = HpScale2;
                CurrentDamageHealScale = DamageHealScale2;
                break;

            case 4:
                hpScale = HpScale3;
                CurrentDamageHealScale = DamageHealScale3;
                break;
        }

        MaxHP = Mathf.RoundToInt(MaxHP * hpScale);
    }
}

[System.Serializable]
public class StancValue
{
    public enum EStancType
    {
        High,
        Middle,
        Low
    }

    public float defenseBonus;
    public float attackBonus;
}

public enum EnemyType
{
    normal,
    elite,
    boss
}

