using System;
using UnityEngine;

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

    public bool Increase(int amount = 1)
    {
        if (amount <= 0) return false;

        int prev = CurrentGauge;
        CurrentGauge += amount;

        if (prev != CurrentGauge)
            OnGaugeChanged?.Invoke(CurrentGauge);

        bool becameFull = prev < MAX_GAUGE && CurrentGauge >= MAX_GAUGE;
        if (becameFull)
        {
            OnGaugeFull?.Invoke();
            return true;
        }

        return false;
    }

    public void Decrease(int amount = 1)
    {
        if (amount <= 0) return;

        int prev = CurrentGauge;
        CurrentGauge -= amount;

        if (prev != CurrentGauge)
            OnGaugeChanged?.Invoke(CurrentGauge);
    }

    public void Reset()
    {
        CurrentGauge = 0;
        OnGaugeChanged?.Invoke(CurrentGauge);
    }

    public void ConsumeForEffect()
    {
        CurrentGauge = 0;
        OnGaugeChanged?.Invoke(CurrentGauge);
    }

    public void SetGauge(int value)
    {
        int prev = CurrentGauge;
        CurrentGauge = value;

        if (prev != CurrentGauge)
            OnGaugeChanged?.Invoke(CurrentGauge);

        bool becameFull = prev < MAX_GAUGE && CurrentGauge >= MAX_GAUGE;
        if (becameFull)
            OnGaugeFull?.Invoke();
    }

    public void Fill()
    {
        SetGauge(MAX_GAUGE);
    }
}