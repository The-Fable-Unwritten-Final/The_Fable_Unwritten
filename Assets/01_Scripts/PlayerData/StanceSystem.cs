using System;
using UnityEngine;

public class StanceSystem
{
    private readonly PlayerController owner;
    private readonly PlayerData playerData;
    private readonly PotentialGauge potentialGauge;

    public event Action<StancType> OnStanceChanged;
    public event Action OnStanceEffectTriggered;
    public event Action<int> OnGaugeChanged;

    public StancType CurrentStance => playerData.currentStance;
    public PotentialGauge Gauge => potentialGauge;
    public bool PotentialTriggeredThisTurn { get; private set; }


    public StanceSystem(PlayerController owner, PlayerData playerData)
    {
        this.owner = owner;
        this.playerData = playerData;
        this.potentialGauge = new PotentialGauge();

        potentialGauge.OnGaugeFull += OnGaugeFull;
        potentialGauge.OnGaugeChanged += value => OnGaugeChanged?.Invoke(value);
    }

    public void ChangeStance(StancType newStance)
    {
        if (playerData.currentStance != newStance)
        {
            var old = playerData.currentStance;
            playerData.currentStance = newStance;
            OnStanceChanged?.Invoke(newStance);
            Debug.Log($"[StanceSystem] 자세 변경: {old} -> {newStance}");
        }
    }

    public void OnAllyUsedCard(bool isSelf)
    {
        if (isSelf)
            potentialGauge.Decrease(1);
        else
            potentialGauge.Increase(1);
    }

    public void IncreaseGauge(int amount) => potentialGauge.Increase(amount);
    public void DecreaseGauge(int amount) => potentialGauge.Decrease(amount);
    public void ResetGauge() => potentialGauge.Reset();

    private void OnGaugeFull()
    {
        PotentialTriggeredThisTurn = true;
        OnStanceEffectTriggered?.Invoke();
        potentialGauge.ConsumeForEffect();
    }

    public void OnTurnEnd()
    {
        PotentialTriggeredThisTurn = false;
    }

    public void OnBattleEnd()
    {
        PotentialTriggeredThisTurn = false;
        potentialGauge.Reset();
        owner?.stanceEffectData?.Reset();
    }

    public void Dispose()
    {
        potentialGauge.OnGaugeFull -= OnGaugeFull;
    }
}