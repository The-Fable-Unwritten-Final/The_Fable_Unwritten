using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EffectTarget {
    FirstCardCost,
    CardCost,            
    EnemyMaxHP,
    IncomingDamageFlat,
    IncomingDamagePercent,
    OutgoingDamageFlat,
    OutgoingDamagePercent,
    HealAmount,
    RandomDamageOffset,
    ApplyRandomDebuffToAlly,
    OnApplyBuffValue,
    OnApplyDebuffValue,
    // ... 추가 가능
}

public enum EffectOperation {
    Add,        // 추가
    MulPercent, // 배율
    Set,        // ~ 값 으로 설정
    RandomRange // 랜덤한 범위 내 값 설정
}

public enum EffectScope {
    OnStartOfBattle,
    OnStartOfTurn,
    OnEnterCombat,
    OnCardUse,
    OnFirstCardInBattle,
    OnApplyBuff,
    OnApplyDebuff
}

[Serializable]
public class StyleEffect {
    
    public EffectTarget target; // 적용 대상
    public EffectOperation operation; // 연산 방식
    public float value;       // (배율 시 +20% 면 1.2f 입력)
    public Vector2 valueRange; // 랜덤한 범위 사용 시 쓰는 value 값
    public EffectScope scope; // 효과 발동 지점 (전투 시작시... 턴 시작시... 등..)
    public string effectDescription; // 효과 설명 텍스트 (로컬라이제이션을 적용 시 사용하는 key 값 설명 => ~~~ n 만큼 증가 같은 포멧 대응 가능하도록, 로컬라이제이션 쪽 별도 처리 필요)
    public int rank = 1;  // 등급 (1 == 상급, 2 == 중급, 3 == 하급 문체)
}
