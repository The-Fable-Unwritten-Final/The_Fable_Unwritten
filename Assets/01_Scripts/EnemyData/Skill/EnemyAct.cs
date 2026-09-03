using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyAct
{
    public int index;

    public TargetType targetType;
    public int targetNum;

    public bool target_front;
    public bool target_center;
    public bool target_back;

    // Effect 1
    public EnemyEffectType arg_effect1;
    public int arg1;
    public EnemyEffectTarget arg_target1;

    // Effect 2
    public EnemyEffectType arg_effect2;
    public int arg2;
    public EnemyEffectTarget arg_target2;

    // 이 스킬 자체를 선택할 수 있는 조건
    public EnemyUseCondition useCondition;
    public int useConditionValue;

    // 조건부 수치 계산
    public EnemyValueModifier valueModifier;

    // Scar, Hap 등 조건/계산에 참조할 상태
    // 적 전용 상태까지 포함하기 위해 EnemyEffectType을 사용
    public EnemyEffectType modifierStatus;

    // 배율 또는 고정 추가값
    public int modifierValue;

    // HP 50% 이하 등의 조건 기준값
    public int modifierConditionValue;

    // 일반 데이터만으로 처리하기 어려운 전용 기믹
    public EnemySpecialLogic specialLogic;

    public string skilleffect;

    public int soundIndex;
    public float soundDelay;


    // =========================================================
    // Legacy
    // EnemyPattern V1 제거 전까지만 유지
    // =========================================================

    public int atk_buff;
    public int def_buff;
    public int buff_time;
    public bool block;
    public int stun;
}


public enum EnemyEffectType
{
    None = 0,

    // 기본 수치 효과
    Damage,
    Heal,

    Attack,
    Defense,

    // 공통 상태
    Burn,
    Freeze,
    Activate,

    Bless,
    Crime,
    Penance,

    Scar,
    Stun,
    Guard,

    // 기타 공통 전투 효과
    Potential,
    Block,
    Resist,
    CleanseDebuff,
    Reflect,

    // 적 전용 상태 / 기믹
    Mark,
    AssaultReady,
    ShieldTactic,
    Combo,
    TargetMark,
    Formation,
    Hap
}


public enum EnemyEffectTarget
{
    None = 0,

    // EnemyAct의 targetType/targetNum/열 조건으로 뽑힌 대상
    SkillTarget,

    // 스킬 사용자 자신
    Self,

    // 스킬 사용자 측 아군 전체
    Allies,

    // 스킬 사용자 측 살아있는 아군 중 체력이 가장 낮은 대상
    LowestHpAlly
}


public enum EnemyUseCondition
{
    None = 0,

    // 자신의 현재 HP가 useConditionValue% 이하일 때만 선택 가능
    SelfHpBelow
}


public enum EnemyValueModifier
{
    None = 0,

    // 기본 값 + 대상이 가진 modifierStatus 수치
    // ex) 101: Damage 6 + 대상 Scar
    AddTargetStatusValue,

    // 기본 값 + 자신의 modifierStatus 수치 * modifierValue
    // ex) 433: Damage 10 + 자신의 Hap * 4
    AddSelfStatusValueMultiply,

    // 자신의 HP가 modifierConditionValue% 이하라면
    // 기본 값 + modifierValue
    // ex) 443: HP <= 50%이면 Damage 8 + 3
    AddValueIfSelfHpBelow
}


public enum EnemySpecialLogic
{
    None = 0,

    // 322: 보스인 경우에만 Potential +1
    PotentialIfBoss,

    // 421: 지목 우선 / 지목 대상이면 피해 9, 없으면 최고 HP 대상 피해 8
    CommanderMarkedStrike,

    // 422: 병사령에게만 Formation +2 / Attack +1
    CommanderReorganize,

    // 423: 대상에게 Mark 또는 TargetMark가 있으면 피해 +3
    CommanderCharge,

    // 433: 사용 후 Hap = 0
    ConsumeHap,

    // 434: 다음 행동을 일섬으로 고정
    ForceNextIlsum,

    // 442: 도전자 수에 따른 대상 및 피해 변경
    SamuraiSweep,

    // 462: 강습 준비 전용 후속 행동/취소 판정
    AssaultReady,

    // 472: 단일 지정 공격 대상 제한
    ShieldTactic,

    // 482: 1 + Combo 중첩 횟수만큼 반복 공격
    ComboStrike,

    // 492: 이 스킬 자신의 피해에는 새 TargetMark 추가피해를 발동시키지 않음
    ApplyTargetMarkWithoutTrigger
}