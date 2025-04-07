using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/CharacterData")]
public class PlayerData : ScriptableObject
{
    public string IDNum; //고유번호
    public string CharacterName; //캐릭터이름
    public Sprite Icon; //Icon
    public int Health; //체력
    public int ATK; //공격력

    [Header("statValue")]
    public int[] HealthValue; //체력변화
    public int[] ATKValue; //공격력 변화


}
