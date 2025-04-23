using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StatEventEffects : EventEffects
{
    // 적용 대상
    public bool sophia;
    public bool kyla;
    public bool leon;
    public bool enemy;

    // value 값
    public int hp; // 단순 증감
    public float hpPercent; // 퍼센트 변화
    public int atk; // 단순 증감
    public int def; // 단순 증감

    public override void Apply()
    {
        if (sophia)
        {
            if(hp != 0)
            {
                GameManager.Instance.playerDatas[1].currentHP += hp;
                GameManager.Instance.playerDatas[1].currentHP = Mathf.Clamp(GameManager.Instance.playerDatas[1].currentHP, 1, GameManager.Instance.playerDatas[1].MaxHP);// 변경된 hp 가 1~최대hp 가 유지되도록.
            }
            if (hpPercent != 0)
            {
                Debug.Log($"{name} : Sophia StatEvent HP Percent {hpPercent}");
            }
            if (atk != 0)
            {
                GameManager.Instance.playerDatas[1].GetComponent<IStatusReceiver>().ApplyStatusEffect(new StatusEffect
                {
                    statType = BuffStatType.Attack,
                    value = atk,
                    duration = 1000 // 전투 동안 유지되도록 높게
                });
            }
            if (def != 0)
            {
                GameManager.Instance.playerDatas[1].GetComponent<IStatusReceiver>().ApplyStatusEffect(new StatusEffect
                {
                    statType = BuffStatType.Defense,
                    value = def,
                    duration = 1000 // 전투 동안 유지되도록 높게
                });
            }
        }
        if (kyla)
        {
            if(hp != 0)
            {
                GameManager.Instance.playerDatas[0].currentHP += hp;
                GameManager.Instance.playerDatas[0].currentHP = Mathf.Clamp(GameManager.Instance.playerDatas[0].currentHP, 1, GameManager.Instance.playerDatas[0].MaxHP);// 변경된 hp 가 1~최대hp 가 유지되도록.
            }
            if (hpPercent != 0)
            {
                Debug.Log($"{name} : Kyla StatEvent HP Percent {hpPercent}");
            }
            if (atk != 0)
            {
                GameManager.Instance.playerDatas[0].GetComponent<IStatusReceiver>().ApplyStatusEffect(new StatusEffect
                {
                    statType = BuffStatType.Attack,
                    value = atk,
                    duration = 1000 // 전투 동안 유지되도록 높게
                });
            }
            if (def != 0)
            {
                GameManager.Instance.playerDatas[0].GetComponent<IStatusReceiver>().ApplyStatusEffect(new StatusEffect
                {
                    statType = BuffStatType.Defense,
                    value = def,
                    duration = 1000 // 전투 동안 유지되도록 높게
                });
            }
        }
        if (leon)
        {
            if(hp != 0)
            {
                GameManager.Instance.playerDatas[2].currentHP += hp;
                GameManager.Instance.playerDatas[2].currentHP = Mathf.Clamp(GameManager.Instance.playerDatas[2].currentHP, 1, GameManager.Instance.playerDatas[2].MaxHP);// 변경된 hp 가 1~최대hp 가 유지되도록.
            }
            if (hpPercent != 0)
            {
                Debug.Log($"{name} : Leon StatEvent HP Percent {hpPercent}");
            }
            if (atk != 0)
            {
                GameManager.Instance.playerDatas[2].GetComponent<IStatusReceiver>().ApplyStatusEffect(new StatusEffect
                {
                    statType = BuffStatType.Attack,
                    value = atk,
                    duration = 1000 // 전투 동안 유지되도록 높게
                });
            }
            if (def != 0)
            {
                GameManager.Instance.playerDatas[2].GetComponent<IStatusReceiver>().ApplyStatusEffect(new StatusEffect
                {
                    statType = BuffStatType.Defense,
                    value = def,
                    duration = 1000 // 전투 동안 유지되도록 높게
                });
            }
        }
        if (enemy)
        {
            if(hp != 0)
            {
                Debug.Log($"{name} : Enemy StatEvent HP {hp}");
            }
            if (hpPercent != 0)
            {
                foreach (var enemyData in GameManager.Instance.turnController.battleFlow.enemyParty)
                {
                    enemyData.currentHP = enemyData.maxHP * hpPercent;
                }
            }
            if (atk != 0)
            {
                Debug.Log($"{name} : Enemy StatEvent ATK {atk}");
            }
            if (def != 0)
            {
                Debug.Log($"{name} : Enemy StatEvent DEF {def}");
            }
        }
    }
    public override void UnApply()
    {
    }
    public override EventEffects Clone()
    {
        return Instantiate(this);
    }
}
