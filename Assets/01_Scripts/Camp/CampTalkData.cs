using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캠프 대화 데이터
/// 조건에 따라 특정 대화 시퀀스를 재생하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "CampTalkData_New", menuName = "Camp/CampTalkData", order = 1)]
public class CampTalkData : ScriptableObject
{
    public enum ConditionType
    {
        None,           // 조건 없음 (항상 가능)
        Stage,          // 특정 스테이지 조건 (2, 3, 4)
        BeforeEvent,    // 랜덤 이벤트 발생 전
        AfterEvent      // 랜덤 이벤트 발생 후
    }

    [Header("조건 설정")]
    public ConditionType conditionType = ConditionType.None;
    
    [Header("스테이지: 2,3,4 //// 랜덤 이벤트: 이벤트의 ID")]
    public int conditionValue;

    [Header("대화 텍스트 Key 순서 직접 입력")]
    public List<string> keyTextSerial = new();

    /// <summary>
    /// 현재 게임 상태에서 이 대화가 실행 가능한지 체크
    /// </summary>
    public bool IsValid()
    {
        ProgressDataManager pdm = ProgressDataManager.Instance;

        return conditionType switch
        {
            ConditionType.None => true,

            ConditionType.Stage => pdm.StageIndex == conditionValue,

            ConditionType.BeforeEvent => !pdm.HasUsedRandomEvent(conditionValue),

            ConditionType.AfterEvent => pdm.HasUsedRandomEvent(conditionValue),

            _ => false
        };
    }

    /// <summary>
    /// 디버그: 이 데이터의 조건을 문자열로 표현
    /// </summary>
    public string GetConditionDescription()
    {
        return conditionType switch
        {
            ConditionType.None => "조건 없음",
            ConditionType.Stage => $"스테이지 {conditionValue}",
            ConditionType.BeforeEvent => $"이벤트 {conditionValue} 이전",
            ConditionType.AfterEvent => $"이벤트 {conditionValue} 이후",
            _ => "알 수 없음"
        };
    }
}
