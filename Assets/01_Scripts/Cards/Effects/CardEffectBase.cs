using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 카드 효과 베이스 및 확장
/// </summary>
public abstract class CardEffectBase : ScriptableObject
{
    /// <summary>
    /// 카드 효과 적용 메서드
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public abstract void Apply(IStatusReceiver caster, List<IStatusReceiver> targets);       //타겟(적, 플레이어)에게 어떤 효과를 주는 지

    /// <summary>
    /// 카드 효과 설명 반환
    /// </summary>
    /// <returns></returns>
    public abstract string GetDescription();
    
    public virtual bool isTriggerHitAnim => false;
}
