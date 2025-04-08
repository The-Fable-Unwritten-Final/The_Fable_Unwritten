using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private EnemyData enemyData;
    public EnemyData EnemyData { set { enemyData = value; } }

    public void WatchEnemyInfo()
    {
        Debug.Log("이름 :: " + enemyData.EnemyName);
        Debug.Log("체력 :: " + enemyData.MaxHP);
        Debug.Log("공격력 :: " + enemyData.ATK);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
