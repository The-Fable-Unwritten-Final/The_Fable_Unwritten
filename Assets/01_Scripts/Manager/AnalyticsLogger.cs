using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class AnalyticsLogger : MonoBehaviour
{
    private static AnalyticsLogger instance;

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            Application.logMessageReceived += OnLogMessageReceived;
        }
    }
    private void OnDisable()
    {
        if (instance == this)
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            instance = null;
        }
    }
    // 클라이언트에 문제가 생겼을 경우의 정보를 자동으로 수집하여 전송하는 기능
    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        // Error와 Exception만 자동 전송
        if (type == LogType.Error || type == LogType.Exception)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string issueDesc = $"[{type}] {logString} | Scene: {sceneName}";
            LogClientIssue((int)type, issueDesc);
        }
    }
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
        /*
        CustomEvent eventData = new CustomEvent("AddCardInfo");
        eventData.Add("CardAdded", index);

        AnalyticsService.Instance.RecordEvent(eventData);*/
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
            1 => "Kyla",
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
    public void LogStageClearInfo(int stageInfo, int columnIndex)
    {
        CustomEvent eventData = new CustomEvent("StageClearInfo");
        eventData.Add("ClearStage", stageInfo);
        eventData.Add("ClearColumn", columnIndex);

        AnalyticsService.Instance.RecordEvent(eventData);
    }

    ////// 얼리 기준 추가 애널리틱스 코드 //////

    /// <summary>
    /// 진행한 튜토리얼 단계 기록
    /// 튜토리얼 도중의 이탈률 확인을 위함
    /// </summary>
    /// <param name="index"></param>
    public void LogTutorialStep(int index)
    {
        CustomEvent eventData = new CustomEvent("tutorial_step");
        eventData.Add("step_id", index);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 전투 종료 시점 기록
    /// </summary>
    /// <param name="enemySetIndex"></param>
    /// <param name="result">0: win, 1: lose, 2: leave</param>
    /// <param name="turnCount"></param>
    /// <param name="cardUseCount"></param>
    public void LogBattleEndInfo(int enemySetIndex, int result, int turnCount, int cardUseCount)
    {
        string resultLabel = result switch
        {
            0 => "win",
            1 => "lose",
            2 => "leave", // 새로하기 를 누를때 기록
            _ => "Unknown"
        };

        CustomEvent eventData = new CustomEvent("battle_end");
        eventData.Add("encounter_id", enemySetIndex); // 적 세트 인덱스
        eventData.Add("result", resultLabel); // 승/패/이탈 결과
        eventData.Add("turn_count", turnCount); // 턴 수
        eventData.Add("card_use", cardUseCount); // 카드 사용 회수

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 카드 제거 기록
    /// </summary>
    /// <param name="cardIndex"></param>
    public void LogCardRemoveInfo(int cardIndex)
    {
        CustomEvent eventData = new CustomEvent("card_removed");
        eventData.Add("card_id", cardIndex);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 전투 노드 종료 시점에서의 기록
    /// </summary>
    /// <param name="result"></param>
    /// <param name="stageNum"></param>
    /// <param name="battleCount"></param>
    public void LogRunEndInfo(int result, int stageNum, int battleCount)
    {
        string resultLabel = result switch
        {
            0 => "win",
            1 => "lose",
            2 => "leave", // 새로하기 를 누를때 기록
            _ => "Unknown"
        };

        CustomEvent eventData = new CustomEvent("run_end");
        eventData.Add("result", resultLabel);
        eventData.Add("last_stage_number", stageNum);
        eventData.Add("battle_count", battleCount);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 클라이언트 에러/예외 자동 감지 및 보고
    /// </summary>
    /// <param name="issueType">로그 타입 (0:Log, 1:Error, 2:Warning, 3:Exception)</param>
    /// <param name="issueDesc">에러 메시지</param>
    public void LogClientIssue(int issueType, string issueDesc)
    {
        CustomEvent eventData = new CustomEvent("client_issue");
        eventData.Add("issue_type", issueType);
        eventData.Add("game_state", issueDesc);

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 카드 선택 시점에서의 기록
    /// </summary>
    /// <param name="cardIndex">선택한 카드 인덱스</param
    /// param name="currentPos">선택한 카드 현재 보유량</param>
    public void LogSelectCardInfo(int cardIndex, int currentPos)
    {
        CustomEvent eventData = new CustomEvent("get_Card");
        eventData.Add("card_id", cardIndex);
        eventData.Add("current_pos", currentPos); // 해당 카드 현재 보유량

        AnalyticsService.Instance.RecordEvent(eventData);
    }
    /// <summary>
    /// 전투 패배 or 이탈 시점에서의 캐릭터 최대 체력 정보 기록
    /// </summary>
    /// <param name="sophHp">소피아 최대 체력</param>
    /// <param name="kylaHp">카일라 최대 체력</param>
    /// <param name="leonHp">레온 최대 체력</param>
    public void LogCharMaxHPInfo(int sophHp, int kylaHp, int leonHp)
    {
        CustomEvent eventData = new CustomEvent("char_MaxHp");
        eventData.Add("soph_Hp", sophHp);
        eventData.Add("kyla_Hp", kylaHp);
        eventData.Add("leon_Hp", leonHp);
        
        AnalyticsService.Instance.RecordEvent(eventData);
    }
}
