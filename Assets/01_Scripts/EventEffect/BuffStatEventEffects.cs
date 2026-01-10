using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffStatEventEffects : EventEffects
{
    public bool sophia;
    public bool kyla;
    public bool leon;
    public int buffStatType; // 버프/디버프 스탯 타입 (-1: 없음)
    public int buffStatValue; // 버프/디버프 수치
    public int buffStatDuration; // 버프/디버프 지속 시간

    /// <summary>
    /// 대상 캐릭터를 선택하는 메서드
    /// </summary>
    private bool ShouldApplyToCharacter(IStatusReceiver chars)
    {
        return (sophia && chars.ChClass == CharacterClass.Sophia) ||
               (kyla && chars.ChClass == CharacterClass.Kayla) ||
               (leon && chars.ChClass == CharacterClass.Leon);
    }

    public override void Apply()
    {
        // 버프/디버프 적용 로직
        if (buffStatType == -1) return;
        
        if (buffStatValue == 0) // 상태이상 저항 버프 제공
        {   
            foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
            {
                if (ShouldApplyToCharacter(chars))
                {
                    chars.hasResist = true; // 상태이상 저항 부여
                }
            }
            return;
        }

        StatusEffect effect;
        if (buffStatDuration > 0)
        {
            // TickEffect: 지속시간이 있는 효과
            effect = new TickEffect
            {
                statType = (BuffStatType)buffStatType,
                value = buffStatValue,
                duration = buffStatDuration
            };
        }
        else
        {
            // InstanceEffect: 즉시 적용되는 효과 (지속시간 없음)
            effect = new InstanceEffect
            {
                statType = (BuffStatType)buffStatType,
                value = buffStatValue,
                isMaintain = false
            };
        }

        foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
        {
            if (ShouldApplyToCharacter(chars))
            {
                chars.ApplyStatusEffect(effect); // 효과 적용
            }
        }
    }

    public override void UnApply()
    {
        // 버프/디버프 해제 로직 작성
        if (buffStatType == -1) return;
        if (buffStatValue == 0) // 상태이상 저항 버프
        {   
            foreach (var chars in GameManager.Instance.turnController.battleFlow.playerParty)
            {
                if (ShouldApplyToCharacter(chars))
                {
                    chars.hasResist = false; // 상태이상 저항 해제
                }
            }
            return;
        }
    }

    public override EventEffects Clone()
    {
        return new BuffStatEventEffects
        {
            index = this.index,
            text = this.text,
            eventType = this.eventType,
            duration = this.duration,
            buffStatType = this.buffStatType,
            buffStatValue = this.buffStatValue,
            buffStatDuration = this.buffStatDuration
        };
    }
}
