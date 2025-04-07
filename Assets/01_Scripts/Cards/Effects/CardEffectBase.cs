using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공통 enum(효과 타입, 캐릭터, 스킬 타입, 버프) 
/// </summary>
public enum CardEffectType { Damage, Heal, Buff, Debuff, Conditional, Chain }
public enum CharacterClass { Sophia, Kayla, Leon }
public enum SkillType { Fire, Ice, Electric, Nature, Buff, Debuff, Holy, Heal, Slash, Strike, Pierce, Defense }
public enum BuffStatType { None, Attack, Defense, ManaRegen }

/// <summary>
/// 상태 이상 및 데미지 등 전투 중 처리되는 효과를 수신할 수 있는 대상에 대한 인터페이스
/// 플레이어나 적 효과 공통 처리
/// </summary>
public interface IStatusReceiver
{
    /// <summary>
    /// 버프/디버프 등 상태 이상 적용
    /// </summary>
    void ApplyStatusEffect(StatusEffect effect);

    /// <summary>
    /// 최종 수치를 계산 (예: 버프에 따라 공격력 증가)
    /// </summary>
    int ModifyStat(BuffStatType statType, int baseValue);

    /// <summary>
    /// 대상에게 피해
    /// </summary>
    void TakeDamage(int amount);

    /// <summary>
    /// 대상 체력 회복
    /// </summary>
    void Heal(int amount);
}

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
    public abstract void Apply(IStatusReceiver caster, IStatusReceiver target);       //타겟(적, 플레이어)에게 어떤 효과를 주는 지

    /// <summary>
    /// 카드 효과 설명 반환
    /// </summary>
    /// <returns></returns>
    public virtual string GetDescription() => name;
}
