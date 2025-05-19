using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class AnalyticsLogger : MonoBehaviour
{
    /// <summary>
    /// 선택한 노드의 정보를 저장
    /// </summary>
    /// <param name="stage">현재 스테이지</param>
    /// <param name="nodeSelected">선택한 노드 종류</param>
    public void LogNodeInfo(int stage, int nodeSelected)
    {
        string nodeLabel = nodeSelected switch
        {
            0 => "Start",
            1 => "Normal",
            2 => "Elite",
            3 => "Boss",
            4 => "Event",
            5 => "Camp",
            _ => "Unknown"
        };

        CustomEvent eventData = new CustomEvent("NodeInfo");
        eventData.Add("Stage", stage);
        eventData.Add("NodeSelected", nodeLabel);

        AnalyticsService.Instance.RecordEvent(eventData);

    }
    /// <summary>
    /// 전투 중 사용한 카드 정보
    /// </summary>
    /// <param name="index">카드의 index</param>
    public void LogUseCardInfo(int index)
    {
        CustomEvent eventData = new CustomEvent("PlayCardInfo");
        eventData.Add("CardUsed", index);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 덱에 추가된 카드 정보
    /// </summary>
    /// <param name="index">카드의 index</param>
    public void LogAddedCardInfo(int index)
    {
        CustomEvent eventData = new CustomEvent("AddCardInfo");
        eventData.Add("CardAdded", index);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 캠프에서 하는 행동
    /// </summary>
    /// <param name="action">0:휴식, 1:소피아, 2:카일라, 3:레온</param>
    public void LogCampActInfo(int action)
    {
        string actionLabel = action switch
        {
            0 => "Rest",
            1 => "Sofia",
            2 => "Kaila",
            3 => "Leon",
            _ => "Unknown"
        };

        CustomEvent eventData = new CustomEvent("CampActInfo");
        eventData.Add("CampActionType", actionLabel);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 선택한 랜덤 이벤트 정보 (선택지 선택정보)
    /// </summary>
    /// <param name="eventIndex">선택지 인덱스</param>
    /// <param name="choice">실제 선택한 정보</param>
    public void LogRandomEventInfo(int eventIndex, int choice)
    {
        CustomEvent eventData = new CustomEvent("EventInfo");
        eventData.Add("EventEncounted", eventIndex);
        eventData.Add("Choice", choice);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 카드 해금으로 선택된 타입
    /// </summary>
    /// <param name="cardType">카드 타입 번호</param>
    public void LogUnlockTypeInfo(int cardType)
    {
        string cardTypeLabel = cardType switch
        {
            0 => "Fire",
            1 => "Ice",
            2 => "Electric",
            3 => "Nature",
            4 => "Buff",
            5 => "Debuff",
            6 => "Holy",
            7 => "Heal",
            8 => "Slash",
            9 => "Strike",
            10 => "Pierce",
            11 => "Defense",
            _ => "Unknown"
        };

        CustomEvent eventData = new CustomEvent("UnlockTypeInfo");
        eventData.Add("SelectedType", cardTypeLabel);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 처음하기 버튼을 클릭시
    /// </summary>
    public void LogReplayInfo()
    {
        CustomEvent eventData = new CustomEvent("PlayInfo");
        eventData.Add("Replay", 1);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 덱 확인 버튼 클릭시
    /// </summary>
    /// <param name="deckIndex">0:소피아, 1:카일라, 2:레온</param>
    public void LogDeckButtonClick(int deckIndex)
    {
        string deckLabel = deckIndex switch
        {
            0 => "Sofia",
            1 => "Kaila",
            2 => "Leon",
            _ => "Unknown"
        };

        CustomEvent eventData = new CustomEvent("DeckButtonInfo");
        eventData.Add("ClickedDeck", deckLabel);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 도로시의 책 버튼 클릭시
    /// </summary>
    public void LogBookButtonClick()
    {
        CustomEvent eventData = new CustomEvent("BookButtonInfo");
        eventData.Add("BookClicked", 1);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 스테이지를 실패한 시점
    /// </summary>
    /// <param name="stageInfo">실패한 지점 스테이지</param>
    /// <param name="columnIndex">실패한 지점 노드 열 번호</param>
    public void LogStageFailInfo(int stageInfo, int columnIndex)
    {
        CustomEvent eventData = new CustomEvent("StageEndInfo");
        eventData.Add("StageNum", stageInfo);
        eventData.Add("ColumnIndex", columnIndex);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
}
