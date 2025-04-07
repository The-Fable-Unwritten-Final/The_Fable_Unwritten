using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "X", menuName = "Enemy/X")]


public class X : ScriptableObject
{
    public string IDNum; //고유번호
    public string EnemyName; //캐릭터이름
    public Sprite Icon; //Icon
    public float EnemyHealth; //체력
    public float EnemyATK; //공격력
    public float EnemyDEF; //방어력

    [Header("statValue")]
    public float[] HealthValue; //체력변화
    public float[] ATKValue; //공격력 변화
    public float[] DEFValue; //방어력 변화

    public enum StencType
    {
        High, //상단
        Middle, //중단
        Low //하단
    }

    [System.Serializable]
    public class StancValue
    {
        public float defenseBonus; //방어력 보너스
        public float attackBonus; //공격력 보너스
        public List<float> floatValue; //임시 작성
        public StencType stencType; //StencType 표시
        public Sprite stanceIcon; //자세 움직임 Sprite
    }

    public List<StancValue> allStances; //임시 작성
    public StancValue currentStance;
}
