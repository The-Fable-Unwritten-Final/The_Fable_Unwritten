using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 공통 enum(효과 타입, 캐릭터, 스킬 타입, 버프) 
/// </summary>
public enum CardEffectType { Damage, Heal, Buff, Debuff, Conditional, Chain }
public enum CharacterClass { Sophia, Kayla, Leon, Enemy }
public enum SkillType { Fire, Ice, Electric, Nature, Buff, Debuff, Holy, Heal, Slash, Strike, Pierce, Defense }
public enum BuffStatType { None, Attack, Defense, ManaRegen, blind, stun }
public enum TargetType { None = 0, Ally = 1, Enemy = 2 }

public enum CardType
{
    Fire = 0,
    Ice = 1,
    Electric = 2,
    Nature = 3,
    Buff = 4,
    Debuff = 5,
    Holy = 6,
    Heal = 7,
    Slash = 8,
    Strike = 9,
    Pierce = 10,
    Defense = 11
}

/// <summary>
/// 전투에서 효과를 받는 대상 (플레이어 / 적) 공통 인터페이스
/// </summary>
public interface IStatusReceiver
{
    CharacterClass CharacterClass { get; set; }          //자신의 캐릭터 이름을 가져옴
    DeckModel Deck { get; }                         //캐릭터의 덱을 가져옴
    void ApplyStatusEffect(StatusEffect effect);     // 버프, 디버프 적용
    float ModifyStat(BuffStatType statType, float baseValue); // 버프 기반 수치 계산
    void TakeDamage(float amount);                     // 데미지 적용
    void Heal(float amount);                           // 힐 적용
    bool IsAlive();                                  // 생존 여부 체크
    bool IsIgnited { get; }                         //각성 상태 확인
    string CurrentStance { get; }                   //현재 자세 확인
    bool IsStunned();                          //스턴 상태 여부 확인

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

    // 광역 지정용 오버로드 (필요한 경우만 override)
    public virtual void ApplyAOE(IStatusReceiver caster, List<IStatusReceiver> targets) { }

    /// <summary>
    /// 카드 효과 설명 반환
    /// </summary>
    /// <returns></returns>
    public abstract string GetDescription();
    
    /// <summary>
    /// 광역기인지 확인 
    /// </summary>
    /// <returns></returns>
    public virtual bool isAOE() => false;
}
