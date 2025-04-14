using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy Data", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    private int idNum;
    public int IDNum { get { return idNum; } } //고유번호

    [SerializeField]
    private string enemyName;
    public string EnemyName { get { return enemyName; } } //이름

    [SerializeField]
    private int maxHP;
    public int MaxHP { get { return maxHP; } } //체력

    [SerializeField]
    private float hpValue;
    public float HPValue { get { return hpValue; } } //체력변화

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
}
