using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EffectTarget
{
    FirstCardCost, // 전투의 첫 카드 코스트 변환
    CardCost, // 나머지 카드 코스트 관련 변환
    EnemyMaxHP,
    Mana, // 보유 마나 관련 변환
    IncomingDamage,
    DamageGive,
    HealAmount,
    InComingDamageRandomOffset,
    OnApplyBuffValue,
    OnApplyDebuffValue,

    // value 변환 없이 바로 호출 효과
    ApplyRandomDebuffToSingleAlly,
    ApplyStunToAllAllies,
}

public enum EffectOperation
{
    Add,        // 추가
    Minus,      // 빼기
    MulPercent, // 배율 증가 (예: +20% 면 1.2f 입력)
    Set,        // ~ 값 으로 설정
    RandomRange, // 랜덤한 범위 내 값 설정
    Instant, // 즉시 효과 발동
    none // 수치의 의미가 없는 효과 적용 방식인 경우
}

public enum EffectCallTime
{
    OnStartOfBattle,
    OnStartOfTurn,
    OnCardUse,
    OnFirstCardUseInBattle,
    OnGettingDamage,
    OnApplyBuff,
    OnApplyDebuff
}

[Serializable]
public class StyleEffect {
    
    public EffectTarget target; // 적용 대상
    public EffectOperation operation; // 연산 방식
    public float value;       // (배율 시 +20% 면 1.2f 입력)
    public Vector2 valueRange; // 랜덤한 범위 사용 시 쓰는 value 값
    public EffectCallTime callTime; // 효과 발동 지점 (전투 시작시... 턴 시작시... 등..)
}
