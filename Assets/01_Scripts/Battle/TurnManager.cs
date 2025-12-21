using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn }

/// <summary>
/// 턴 진행 및 상태 관리
/// </summary>
public class TurnManager
{
    public TurnState CurrentTurn { get; private set; }
    public int TurnCount { get; private set; } = 1;
    public bool IsBattleEnded { get; private set; } = true;

    public event Action OnPlayerTurnStart;
    public event Action OnEnemyTurnStart;
    public event Action OnTurnEnd;

    /// <summary>
    /// 전투 시작
    /// </summary>
    public void StartBattle()
    {
        IsBattleEnded = false;
        TurnCount = 1;
        CurrentTurn = TurnState.PlayerTurn;
    }

    /// <summary>
    /// 전투 종료
    /// </summary>
    public void EndBattle()
    {
        IsBattleEnded = true;
    }

    /// <summary>
    /// 플레이어 턴 시작
    /// </summary>
    public void BeginPlayerTurn()
    {
        if (IsBattleEnded) return;

        CurrentTurn = TurnState.PlayerTurn;
        OnPlayerTurnStart?.Invoke();
    }

    /// <summary>
    /// 플레이어 턴 종료 → 적 턴으로
    /// </summary>
    public void EndPlayerTurn()
    {
        CurrentTurn = TurnState.EnemyTurn;
        OnEnemyTurnStart?.Invoke();
    }

    /// <summary>
    /// 적 턴 종료 → 다음 턴으로
    /// </summary>
    public void EndEnemyTurn()
    {
        TurnCount++;
        OnTurnEnd?.Invoke();

        // 다음 플레이어 턴 시작
        BeginPlayerTurn();
    }

    /// <summary>
    /// 현재 플레이어 턴인지 확인
    /// </summary>
    public bool IsPlayerTurn() => CurrentTurn == TurnState.PlayerTurn && !IsBattleEnded;

    /// <summary>
    /// 강제 전투 종료
    /// </summary>
    public void ForceEnd()
    {
        IsBattleEnded = true;
    }
}