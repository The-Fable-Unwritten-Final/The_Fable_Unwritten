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
    public event Action<int> OnGaugeChanged;

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
        potentialGauge.OnGaugeChanged += (value) => OnGaugeChanged?.Invoke(value);
    }

    /// <summary>
    /// 자세 변경
    /// </summary>
    public void ChangeStance(StancType newStance)
    {
        if (playerData.currentStance != newStance)
        {
            var oldStance = playerData.currentStance;
            playerData.currentStance = newStance;
            OnStanceChanged?.Invoke(newStance);
            Debug.Log($"[StanceSystem] 자세 변경: {oldStance} → {newStance}");
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
    /// 게이지 직접 증가 (카드 효과 등)
    /// </summary>
    public void IncreaseGauge(int amount)
    {
        potentialGauge.Increase(amount);
    }

    /// <summary>
    /// 게이지 직접 감소 (카드 효과 등)
    /// </summary>
    public void DecreaseGauge(int amount)
    {
        potentialGauge.Decrease(amount);
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
        StanceEffectHandler.OnBattleEnd(stanceEffectData);
    }

    /// <summary>
    /// 이벤트 해제
    /// </summary>
    public void Dispose()
    {
        potentialGauge.OnGaugeFull -= OnGaugeFull;
    }

    /// <summary>
    /// 현재 자세의 설명 반환 (UI 툴팁용)
    /// </summary>
    public string GetStanceDescription(CharacterClass charClass)
    {
        var stance = playerData.currentStance;

        return (charClass, stance) switch
        {
            (CharacterClass.Sophia, StancType.Seek) => "탐구: 게이지 만충 시 카드 1장 드로우, 1턴간 비용 0",
            (CharacterClass.Sophia, StancType.Insight) => "통찰: 게이지 만충 시 다음 카드 효과 2회 적용",
            (CharacterClass.Kayla, StancType.Mercy) => "자비: 게이지 만충 시 카드 1장 드로우, 이번 턴 회복량 +30%",
            (CharacterClass.Kayla, StancType.Discipline) => "규율: 게이지 만충 시 다음 공격의 정화 수치만큼 아군 공방 증가",
            (CharacterClass.Leon, StancType.Rush) => "돌진: 게이지 만충 시 다음 공격 피해 +100%",
            (CharacterClass.Leon, StancType.Defense) => "수비: 게이지 만충 시 체력 +20%, 수호 +15",
            _ => "알 수 없는 스탠스"
        };
    }
}