using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventEffects : ScriptableObject
{
    public int index; // 인덱스
    public string text; // 효과 설명
    public int eventType; // 이벤트 타입 0: 스탯 관련, 1: 카드 관련, 2: 전투 발생 ...
    public int duration; // 지속 시간 0: 즉시, 1: 다음 전투, 2: 이번 스테이지. 3: 모험 전체   

    public abstract void Apply(); // 효과 적용 메서드
    public abstract void UnApply(); // 효과 해제 메서드 (카드 위주의 사용)

    public abstract EventEffects Clone();
}
