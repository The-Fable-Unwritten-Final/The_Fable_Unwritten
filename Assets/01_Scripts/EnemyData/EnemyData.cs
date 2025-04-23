using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy Data", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    private int idNum;
    public int IDNum { get { return idNum; } set { idNum = value; } } //고유번호

    [SerializeField]
    private string enemyName;
    public string EnemyName { get { return enemyName; } set { enemyName = value; } } //이름

    [SerializeField]
    private float maxHP;
    public float MaxHP { get { return maxHP; } set { maxHP = value; } } //체력

    [SerializeField]
    private float currentHP;
    public float CurrentHP { get { return currentHP; } set { currentHP = value; } } //현재 체력

    [SerializeField]
    private int aTK;
    public int ATK { get { return aTK; } set { aTK = value; } } //공격력

    [SerializeField]
    private float aTKValue;
    public float ATKValue { get { return aTKValue; } set { aTKValue = value; } } //공격력 변화

    [SerializeField]
    private float dEF;
    public float DEF { get { return dEF; } set { dEF = value; } } // 방어력

    [SerializeField]
    private float dEFValue;
    public float DEFValue { get { return dEFValue; } set { dEFValue = value; } } // 방어력 변화

    private Dictionary<int, EnemySkill> skillDict = new();
    public Dictionary<int, EnemySkill> SkillDict
    {
        get => skillDict;
        set => skillDict = value;
    }

    [System.Serializable]
    public class StancValue
    {
        // StanceType.cs 라는 파일 따로 생성
        public enum EStancType
        {
            High,
            Middle,
            Low
        }

        public float defenseBonus; //방어력 보너스
        public float attackBonus; //공격력 보너스
    }

    public StancValue.EStancType currentStance;

    public RuntimeAnimatorController animationController;


    //스킬 추가
    public void AddSkill(int index, EnemySkill skill)
    {
        if (skillDict == null)
            skillDict = new Dictionary<int, EnemySkill>();

        skillDict[index] = skill;
    }

    //스킬 모두 삭제
    public void ClearSkills()
    {
        skillDict?.Clear();
    }

    //스킬 전부 가져오기
    public IEnumerable<EnemySkill> GetSkills() => skillDict?.Values;
}
