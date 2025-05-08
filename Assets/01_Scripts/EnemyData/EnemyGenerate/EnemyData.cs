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

    [SerializeField] private float maxHP;
    public float MaxHP
    {
        get => maxHP;
        set
        {
            maxHP = value;
            OnHpChanged?.Invoke(currentHP, maxHP); // 최대 체력 변경 시 알림
        }
    }

    [SerializeField] private float currentHP;
    public float CurrentHP
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

/*    /// <summary>
    /// 스테이지에 따라 체력 및 스킬 공격력 변화 함수.
    /// </summary>
    /// <param name="stage"></param>
    public void UpgradeEnemybyStage(int stage)
    {                                           //현재 코드가 원래의 enemydata를 그대로 사용하는 것이 아니라 복제 후 사용하는 것이기 때문에 본래 데이터를 가지고 사용하지 않도록 만들었다.
        MaxHP = MaxHP * (1 + (stage - 1) * hpScale);

        foreach(var skill in skillList)
        {
            skill.damage = skill.damage * (1 + (stage - 1) * damageScale);
        }

    }*/
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

