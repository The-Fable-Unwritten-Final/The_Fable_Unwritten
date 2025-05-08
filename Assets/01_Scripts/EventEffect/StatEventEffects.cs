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
    public int hp; // 힐 사용.
    public float hpPercent; // 퍼센트 변화
    public int atk; // 단순 증감
    public int def; // 단순 증감

    public override void Apply()
    {
        if (sophia)
        {
            if(hp != 0)
            {
                ProgressDataManager.Instance.PlayerDatas[1].currentHP += hp;
                ProgressDataManager.Instance.PlayerDatas[1].currentHP = Mathf.Clamp(ProgressDataManager.Instance.PlayerDatas[1].currentHP, 1, ProgressDataManager.Instance.PlayerDatas[1].MaxHP);
            }
            if (hpPercent != 0)
            {
                
            }
            if (atk != 0)
            {
                foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (chars.ChClass == CharacterClass.Sophia)
                    {
                        chars.ApplyStatusEffect(new StatusEffect
                        {
                            statType = BuffStatType.Attack,
                            value = atk,
                            duration = 1000 // 전투 동안 유지되도록 높게
                        });
                        break;
                    }
                }
            }
            if (def != 0)
            {
                foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (chars.ChClass == CharacterClass.Sophia)
                    {
                        chars.ApplyStatusEffect(new StatusEffect
                        {
                            statType = BuffStatType.Defense,
                            value = def,
                            duration = 1000 // 전투 동안 유지되도록 높게
                        });
                        break;
                    }
                }
            }
        }
        if (kyla)
        {
            if(hp != 0)
            {
                ProgressDataManager.Instance.PlayerDatas[0].currentHP += hp;
                ProgressDataManager.Instance.PlayerDatas[0].currentHP = Mathf.Clamp(ProgressDataManager.Instance.PlayerDatas[0].currentHP, 1, ProgressDataManager.Instance.PlayerDatas[0].MaxHP);
            }
            if (hpPercent != 0)
            {
                
            }
            if (atk != 0)
            {
                foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (chars.ChClass == CharacterClass.Kayla)
                    {
                        chars.ApplyStatusEffect(new StatusEffect
                        {
                            statType = BuffStatType.Attack,
                            value = atk,
                            duration = 1000 // 전투 동안 유지되도록 높게
                        });
                        break;
                    }
                }
            }
            if (def != 0)
            {
                foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (chars.ChClass == CharacterClass.Kayla)
                    {
                        chars.ApplyStatusEffect(new StatusEffect
                        {
                            statType = BuffStatType.Defense,
                            value = def,
                            duration = 1000 // 전투 동안 유지되도록 높게
                        });
                        break;
                    }
                }
            }
        }
        if (leon)
        {
            if(hp != 0)
            {
                ProgressDataManager.Instance.PlayerDatas[2].currentHP += hp;
                ProgressDataManager.Instance.PlayerDatas[2].currentHP = Mathf.Clamp(ProgressDataManager.Instance.PlayerDatas[2].currentHP, 1, ProgressDataManager.Instance.PlayerDatas[2].MaxHP);
            }
            if (hpPercent != 0)
            {
                
            }
            if (atk != 0)
            {
                foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (chars.ChClass == CharacterClass.Leon)
                    {
                        chars.ApplyStatusEffect(new StatusEffect
                        {
                            statType = BuffStatType.Attack,
                            value = atk,
                            duration = 1000 // 전투 동안 유지되도록 높게
                        });
                        break;
                    }
                }
            }
            if (def != 0)
            {
                foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
                {
                    if (chars.ChClass == CharacterClass.Leon)
                    {
                        chars.ApplyStatusEffect(new StatusEffect
                        {
                            statType = BuffStatType.Defense,
                            value = def,
                            duration = 1000 // 전투 동안 유지되도록 높게
                        });
                        break;
                    }
                }
            }
        }
        if (enemy)
        {
            if(hp != 0)
            {
                
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
                
            }
            if (def != 0)
            {
                
            }
        }
    }
    public override void UnApply()
    {
    }
    public override EventEffects Clone()
    {
        return new StatEventEffects
        {
            index = this.index,
            text = this.text,
            eventType = this.eventType,
            duration = this.duration,
            sophia = this.sophia,
            kyla = this.kyla,
            leon = this.leon,
            enemy = this.enemy,
            hp = this.hp,
            hpPercent = this.hpPercent,
            atk = this.atk,
            def = this.def
        };
    }
}
