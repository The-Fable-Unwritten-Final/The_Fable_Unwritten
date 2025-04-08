using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy Data", menuName = "Enemy/EnemyData")] //제일 아래 배치
public class EnemyData : ScriptableObject
{
    [SerializeField]
    public string iDNum;
    public string IDNum { get { return iDNum; } } //고유번호
    [SerializeField]
    private string enemyName;
    public string EnemyName { get { return enemyName; } } //이름
    [SerializeField]
    private int hP;
    public int HP { get { return hP; } } //체력
    [SerializeField]
    private float healthValue;
    public float HealthValue { get { return healthValue; } } //체력변화
    [SerializeField]
    private int aTK;
    public int ATK { get { return aTK; } } //공격력
    [SerializeField]
    private int aTKValue;
    public int ATKValue { get { return aTKValue; } } //공격력 변화
    [SerializeField]
    private float dEF;
    public float DEF { get { return dEF; } } // 방어력
    [SerializeField]
    private float dEFValue;
    public float DEFValue { get { return dEFValue; } } // 방어력 변화



    [System.Serializable]
    public class StancValue
    {
        public enum StencType
        {
            High, //상단
            Middle, //중단
            Low //하단
        }

        public float defenseBonus; //방어력 보너스
        public float attackBonus; //공격력 보너스
    }

    public StancValue.StencType currentStance;
}
