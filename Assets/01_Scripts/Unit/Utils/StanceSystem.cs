using UnityEngine;
using System;

/// <summary>
/// 플레이어의 자세(Stance)와 포텐셜 게이지를 관리하는 시스템
/// </summary>
public class StanceSystem
{
    private readonly PlayerData playerData;
    private readonly PotentialGauge potentialGauge;
    private readonly StanceEffectData stanceEffectData;

    public event Action<StancType> OnStanceChanged;
    public event Action OnStanceEffectTriggered;

    public StancType CurrentStance => playerData.currentStance;
    public PotentialGauge Gauge => potentialGauge;
    public StanceEffectData EffectData => stanceEffectData;

    // 이상실현 사용 여부
    public bool IsIdealUsed { get; private set; }

    public StanceSystem(PlayerData playerData)
    {
        this.playerData = playerData;
        this.potentialGauge = new PotentialGauge();
        this.stanceEffectData = new StanceEffectData();

        potentialGauge.OnGaugeFull += OnGaugeFull;
    }

    /// <summary>
    /// 자세 변경
    /// </summary>
    public void ChangeStance(StancType newStance)
    {
        if (playerData.currentStance != newStance)
        {
            playerData.currentStance = newStance;
            OnStanceChanged?.Invoke(newStance);
            Debug.Log($"[StanceSystem] 자세 변경: {newStance}");
        }
    }

    /// <summary>
    /// 현재 자세 문자열 반환
    /// </summary>
    public string GetStanceString() => playerData.currentStance.ToString();


    /// <summary>
    /// 아군 카드 사용 시 게이지 처리
    /// </summary>
    public void OnAllyUsedCard(bool isSelf)
    {
        if (isSelf)
        {
            potentialGauge.Decrease(1);
        }
        else
        {
            potentialGauge.Increase(1);
        }
    }

    /// <summary>
    /// 게이지 리셋
    /// </summary>
    public void ResetGauge()
    {
        potentialGauge.Reset();
    }

    private void OnGaugeFull()
    {
        OnStanceEffectTriggered?.Invoke();
        potentialGauge.ConsumeForEffect();
    }

    /// <summary>
    /// 스테이지 시작 시 이상실현 초기화
    /// </summary>
    public void ResetIdealForStage()
    {
        IsIdealUsed = false;
    }

    /// <summary>
    /// 이상실현 사용 표시
    /// </summary>
    public void MarkIdealUsed()
    {
        IsIdealUsed = true;
    }


    /// <summary>
    /// 공격 시 스탠스 보너스 적용
    /// </summary>
    public float ApplyAttackBonus(float baseDamage)
    {
        return StanceEffectHandler.ApplyAttackBonus(baseDamage, stanceEffectData);
    }

    /// <summary>
    /// 회복 시 스탠스 보너스 적용
    /// </summary>
    public float ApplyHealBonus(float baseHeal)
    {
        return StanceEffectHandler.ApplyHealBonus(baseHeal, stanceEffectData);
    }

    /// <summary>
    /// 카드 효과 2배 적용 여부 확인
    /// </summary>
    public bool ShouldDoubleCardEffect()
    {
        return StanceEffectHandler.ShouldDoubleCardEffect(stanceEffectData);
    }

    /// <summary>
    /// 턴 종료 시 스탠스 효과 처리
    /// </summary>
    public void OnTurnEnd()
    {
        StanceEffectHandler.OnTurnEnd(stanceEffectData);
    }


    /// <summary>
    /// 전투 종료 시 초기화
    /// </summary>
    public void OnBattleEnd()
    {
        potentialGauge.Reset();
        stanceEffectData.Reset();
    }

    /// <summary>
    /// 이벤트 해제
    /// </summary>
    public void Dispose()
    {
        potentialGauge.OnGaugeFull -= OnGaugeFull;
    }
}