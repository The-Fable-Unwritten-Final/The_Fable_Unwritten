using System;
using UnityEngine;

/// <summary>
/// 포텐셜 게이지 시스템
/// - 10칸 게이지 (0~10)
/// - 아군 카드 사용 시 +1, 자신 카드 사용 시 -1
/// - 10 도달 시 스탠스 고유 효과 발동
/// </summary>
[Serializable]
public class PotentialGauge
{
    public const int MAX_GAUGE = 10;
    public const int MIN_GAUGE = 0;

    [SerializeField] private int _currentGauge = 0;

    public int CurrentGauge
    {
        get => _currentGauge;
        private set => _currentGauge = Mathf.Clamp(value, MIN_GAUGE, MAX_GAUGE);
    }

    public event Action<int> OnGaugeChanged;
    public event Action OnGaugeFull;

    public bool IsFull => CurrentGauge >= MAX_GAUGE;
    public bool IsEmpty => CurrentGauge <= MIN_GAUGE;
    public float FillPercent => (float)CurrentGauge / MAX_GAUGE;

    /// <summary>
    /// 게이지 증가 (아군이 카드 사용 시)
    /// </summary>
    public bool Increase(int amount = 1)
    {
        if (amount <= 0) return false;

        int prev = CurrentGauge;
        CurrentGauge += amount;

        if (prev != CurrentGauge)
            OnGaugeChanged?.Invoke(CurrentGauge);

        if (CurrentGauge >= MAX_GAUGE)
        {
            OnGaugeFull?.Invoke();
            return true; // 만충 발생
        }
        return false;
    }

    /// <summary>
    /// 게이지 감소 (자신이 카드 사용 시)
    /// </summary>
    public void Decrease(int amount = 1)
    {
        if (amount <= 0) return;

        int prev = CurrentGauge;
        CurrentGauge -= amount;

        if (prev != CurrentGauge)
            OnGaugeChanged?.Invoke(CurrentGauge);
    }

    /// <summary>
    /// 게이지 초기화 (전투 종료 시)
    /// </summary>
    public void Reset()
    {
        CurrentGauge = 0;
        OnGaugeChanged?.Invoke(CurrentGauge);
    }

    /// <summary>
    /// 효과 발동 후 게이지 소모
    /// </summary>
    public void ConsumeForEffect()
    {
        CurrentGauge = 0;
        OnGaugeChanged?.Invoke(CurrentGauge);
    }

    /// <summary>
    /// 직접 게이지 설정 (카드 효과 등)
    /// </summary>
    public void SetGauge(int value)
    {
        int prev = CurrentGauge;
        CurrentGauge = value;

        if (prev != CurrentGauge)
            OnGaugeChanged?.Invoke(CurrentGauge);

        if (CurrentGauge >= MAX_GAUGE)
            OnGaugeFull?.Invoke();
    }

    /// <summary>
    /// 게이지를 최대로 채움
    /// </summary>
    public void Fill()
    {
        SetGauge(MAX_GAUGE);
    }
}