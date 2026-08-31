
using System;
using System.Net;
using UnityEngine;

/// <summary>
/// 공통 enum(효과 타입, 캐릭터, 스킬 타입, 버프) 
/// </summary>
public enum CardEffectType { Damage, Heal, Buff, Debuff, Conditional, Chain }
public enum CharacterClass { Sophia, Kayla, Leon, Enemy }
public enum StancType { None, Seek, Insight, Mercy, Discipline, Rush, Defense }
public enum TargetType { None = 0, Ally = 1, Enemy = 2 }
public enum IdealRealizationType
{
    SophiaVisionary,
    SophiaAlchemist,

    KaylaSacredFlame,
    KaylaJudge,

    LeonLionheart,
    LeonShadowWarrior
}
public enum CardType
{
    Fire = 0,
    Ice = 1,
    Nature = 2,
    Pray = 3,
    Holy = 4,
    baptism = 5,
    Slash = 6,
    Strike = 7,
    Defense = 8,
    Taboo = 9
}

public enum BuffStatType
{
    None,                   // 기본값

    Attack,                 // 공격력 증가, 감소
    Defense,                // 방어력 증가, 감소

    Burn,                   // 화상 (턴 시작 시 피해)
    Freeze,                 // 빙결 (공격력 % 감소)
    Activate,              // 활성 (속성 상태이상 증폭)

    Bless,                  // 축복 (이로운 효과 증폭)
    Crime,                  // 죄악
    Penance,                // 참회

    // 레온 - 물리
    Scar,       // 상처
    Stun,       // 기절
    Guard,       // 수호

    CantAttackInStance,     // 특정 자세에서 공격 불가
    Blind,                   // 실명 (명중률 저하 등, 필요 시)
    SustainRegen,           //지속 회복
    IronBlood,              //철혈 (레온 이상실현)(피해 전가, 받는 피해 반)
    Exposed,                 //노출 (레온 이상실현)(받피증 2배)
    Undying,                //체력 1 남기기

    // Enemy V2
    Reflect,
    Mark,
    AssaultReady,
    ShieldTactic,
    Combo,
    TargetMark,
    Formation,
    Hap
}

// ===== 카드 키워드 (CSV index 10~18) =====
public enum CardKeyword
{
    None = 0,
    Exhaust,    // 소멸 (10) - 사용 시 덱에서 제외
    Retain,     // 보존 (11) - 턴 종료 시 패에 유지
    Temporary,  // 증발 (12) - 미사용 시 덱에서 제외
    Copy,       // 복사 (13) - 사용 시 동일한 카드 생성
    Innate,     // 개전 (14) - 전투 시작 시 반드시 드로우
    Kill,       // 결정타 (15) - 적 처치 시 영구 강화
    Grow,       // 성장 (16) - N회 사용 시 카드 진화
    Critical,   // 강타 (17) - 상처만큼 추가 피해
    Switch      // 전환 (18) - 스탠스 변경
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
    void ApplyStatusEffect(StatusEffect effect);     // 버프, 디버프 적용
    float ModifyStat(BuffStatType statType, float baseValue); // 버프 기반 수치 계산
    float TakeDamage(float amount);                     // 데미지 적용
    void TakeTrueDamage(float amount);                  //방무뎀 적용
    void Heal(float amount);                           // 힐 적용
    bool IsAlive();                                  // 생존 여부 체크
    bool IsIgnited { get; }                         //각성 상태 확인
    string CurrentStance { get; }                   //현재 자세 확인
    bool IsStunned();                          //스턴 상태 여부 확인
    bool hasResist { get; set; }         //상태이상 디버프 저항 여부
    bool HasEffect(BuffStatType type);   //해당 버프/디버프 있는지 확인
    float GetEffectValue(BuffStatType type); //해당 버프/디버프 얼마나 있는지 확인
    bool IsDeathPending { get; }
    void TryFinalizeDeath();

    void ChangeStance(StancType stance);

    // 💥 애니메이션 및 GUI 관련 추가
    void PlayAttackAnimation(int input, Action onHitTiming = null);
    void PlayHitAnimation();
    void CameraActionPlay();                   //행동시 카메라의 줌인 액션 연출.
    float ApplyScarAttackBonus(float baseDamage);

    void ClearScarBurst();

    Transform CachedTransform { get; }
    DmgBarDisplay dmgBar { get; }
    TargetArrowDisplay tarArrow { get; }          // 카드 사용시의 시전 대상 타겟 화살표
    bool IsTargetable { get; set; }                     // 타겟 가능 여부
    event Action OnTargetableChanged;       // 타겟 가능 여부 변경 이벤트
    public DmgBarQueueHandler dmgTextQueue { get; }

    public Transform FootPoint { get; }
    public Transform BodyPoint { get; }
    public Transform HeadPoint { get; }
    public Transform OverheadPoint { get; }
    public Transform AheadPoint { get; }


}

public static class StatusReceiverExtentions
{
    /// <summary>
    /// 플레이어인지 확인
    /// </summary>
    public static bool IsPlayer(this IStatusReceiver receiver)
    {
        return receiver.ChClass != CharacterClass.Enemy;
    }

    /// <summary>
    /// 적인지 확인
    /// </summary>
    public static bool IsEnemy(this IStatusReceiver receiver)
    {
        return receiver.ChClass == CharacterClass.Enemy;
    }

    /// <summary>
    /// PlayerController로 캐스팅 (안전)
    /// </summary>
    public static PlayerController AsPlayer(this IStatusReceiver receiver)
    {
        return receiver as PlayerController;
    }

    /// <summary>
    /// Enemy로 캐스팅 (안전)
    /// </summary>
    public static Enemy AsEnemy(this IStatusReceiver receiver)
    {
        return receiver as Enemy;
    }
}