using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/CharacterData")]


public class PlayerData : ScriptableObject
{
    [Header("Basic Stat")]
    public string IDNum; //고유번호
    public string CharacterName; //캐릭터이름
    public Sprite Icon; //Icon
    public float MaxHP; //체력
    public float ATK; //공격력
    public float DEF; //방어력


    [Header("Stat Value")]
    public float[] HPValue; //체력변화
    public float[] ATKValue; //공격력 변화

    public enum StancType
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
        public StancType stencType; //StencType 표시
        public Sprite stanceIcon; //자세 움직임 Sprite
    }

    public List<StancValue> allStances; //임시 작성
    public StancValue currentStance;


}
