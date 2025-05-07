using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleLogManager : MonoSingleton<BattleLogManager>
{
    /// <summary>
    /// 카드 로그 확인을 위한 CardUseLog 클래스
    /// </summary>
    public class CardUseLog
    {
        public int cardID;
        public CharacterClass user;
        public int cost;
        public CardType type;

        public CardUseLog(int id, CharacterClass user, int cost, CardType type)
        {
            cardID = id;
            this.user = user;
            this.cost = cost;
            this.type = type;
        }
    }

    public LinkedList<CardUseLog> UsedCardsForGame = new();         //단일 게임 전체 카드 사용 정보
    public LinkedList<CardUseLog> UsedCardsForStage = new();        //단일 스테이지 카드 사용 정보
    public LinkedList<CardUseLog> UsedCardsForBattle = new();       //단일 전투 카드 사용 정보
    public LinkedList<CardUseLog> UsedCardsForPrevious = new();     //전턴 카드 사용 정보
    public LinkedList<CardUseLog> UsedCardsForCurrent = new();      //현재 턴 카드 사용 정보
    
    /// <summary>
    /// 사용 한 카드를 위 리스트에 저장
    /// </summary>
    /// <param name="user"></param>
    /// <param name="card"></param>
    public void RegisterCardUse(IStatusReceiver user, CardModel card)
    {
        if (card == null) return;

        var log = new CardUseLog(card.index, user.ChClass, card.manaCost, card.type);
        UsedCardsForGame.AddLast(log);
        UsedCardsForStage.AddLast(log);
        UsedCardsForBattle.AddLast(log);
        UsedCardsForCurrent.AddLast(log);
    }

    /// <summary>
    /// 턴 종료시 current리스트를 previous 리스트로 이동
    /// </summary>
    public void OnTurnEnd()
    {
        UsedCardsForPrevious.Clear();
        foreach (var log in UsedCardsForCurrent)
        {
            UsedCardsForPrevious.AddLast(log);
        }
        UsedCardsForCurrent.Clear();
    }

    /// <summary>
    /// 현재턴 마지막 사용 카드 가져오기
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public int GetLastCardUsed(IStatusReceiver user)
    {
        foreach (var log in UsedCardsForBattle.Reverse())
        {
            if (log.user == user.ChClass)
                return log.cardID;
        }
        return -1;
    }

    /// <summary>
    /// 스테이지 사용 정보 초기화
    /// </summary>
    public void ResetStageLog()
    {
        UsedCardsForStage.Clear();
        ResetBattleLog();
    }

    /// <summary>
    /// 현재 배틀 사용 로그 정보 초기화
    /// </summary>
    public void ResetBattleLog()
    {
        UsedCardsForBattle.Clear();
        UsedCardsForPrevious.Clear();
        UsedCardsForCurrent.Clear();
    }

    /// <summary>
    /// 게임 전체 로그 정보 초기화
    /// </summary>
    public void ResetGameLog()
    {
        UsedCardsForGame.Clear();
        ResetStageLog();
    }

    /// <summary>
    /// 전턴 사용 카드 타입을 돌려주는 함수
    /// </summary>
    /// <returns></returns>
    public List<CardType> GetPreviousTurnCardTypes()
    {
        var set = new HashSet<CardType>();
        foreach (var log in UsedCardsForPrevious)
            set.Add(log.type);
        return new List<CardType>(set);
    }

    /// <summary>
    /// 현재 턴 사용 카드 타입을 돌려주는 함수
    /// </summary>
    /// <returns></returns>
    public List<CardType> GetCurrentTurnCardTypes()
    {
        var set = new HashSet<CardType>();
        foreach (var log in UsedCardsForCurrent)
            set.Add(log.type);
        return new List<CardType>(set);
    }
}

