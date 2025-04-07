using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]

public class EnemyData : ScriptableObject
{
    public string IDNum; //고유번호
    public string EnemyName; //캐릭터이름
    public Sprite Icon; //Icon
    public int EnemyHealth; //체력
    public int EnemyTK; //공격력

    [Header("statValue")]
    public int[] HealthValue; //체력변화
    public int[] ATKValue; //공격력 변화
}
