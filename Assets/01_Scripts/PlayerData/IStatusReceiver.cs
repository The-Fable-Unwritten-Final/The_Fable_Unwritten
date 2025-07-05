
using UnityEngine;
using System;

/// <summary>
/// 공통 enum(효과 타입, 캐릭터, 스킬 타입, 버프) 
/// </summary>
public enum CardEffectType { Damage, Heal, Buff, Debuff, Conditional, Chain }
public enum CharacterClass { Sophia, Kayla, Leon, Enemy }
public enum SkillType { Fire, Ice, Electric, Nature, Buff, Debuff, Holy, Heal, Slash, Strike, Pierce, Defense }
public enum BuffStatType
{
    None,                   // 기본값

    Attack,                 // 공격력 증가, 감소
    Defense,                // 방어력 증가, 감소

    Bless,                  // 축복 (이로운 효과 증폭)
    Grace,                  // 은총 (회복량 증가)
    Purify,                 // 정화 (상대 상태이상 감소)

    Burn,                   // 화상 (턴 시작 시 피해)
    Freeze,                 // 빙결 (공격력 % 감소)
    Activate,              // 활성 (속성 상태이상 증폭)

    Bleed,                  // 출혈 (공격받을 때 추가 피해)
    Stun,                   // 기절 (행동 불가)
    GuardRedirect,          // 수호 (공격 유도)

    CantAttackInStance,     // 특정 자세에서 공격 불가
    Blind                   // 실명 (명중률 저하 등, 필요 시)
}

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
    CharacterClass ChClass { get; set; }          //자신의 캐릭터 이름을 가져옴
    DeckModel Deck { get; }                         //캐릭터의 덱을 가져옴
    float maxHP { get; set; }                            //최대 체력
    float currentHP { get; set; }                        //현재 체력
    void UpdateHpStatus();                        //체력 상태 업데이트 (currentHp, maxHp 변수에 실제 데이터값 받아오기)
    void CameraActionPlay();                   //행동시 카메라의 줌인 액션 연출.
    void ApplyStatusEffect(StatusEffect effect);     // 버프, 디버프 적용
    float ModifyStat(BuffStatType statType, float baseValue); // 버프 기반 수치 계산
    float TakeDamage(float amount);                     // 데미지 적용
    void TakeTrueDamage(float amount);                  //방무뎀 적용
    void Heal(float amount);                           // 힐 적용
    bool IsAlive();                                  // 생존 여부 체크
    bool IsIgnited { get; }                         //각성 상태 확인
    string CurrentStance { get; }                   //현재 자세 확인
    bool IsStunned();                          //스턴 상태 여부 확인

    // 💥 애니메이션 및 GUI 관련 추가
    void PlayAttackAnimation();
    void PlayHitAnimation();
    Transform CachedTransform { get; }
    DmgBarDisplay dmgBar { get; }
    TargetArrowDisplay tarArrow { get; }          // 카드 사용시의 시전 대상 타겟 화살표
    bool IsTargetable { get; set; }                     // 타겟 가능 여부
    event Action OnTargetableChanged;       // 타겟 가능 여부 변경 이벤트
    public DmgBarQueueHandler dmgTextQueue { get; }

}
