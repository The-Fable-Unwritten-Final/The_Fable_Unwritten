using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class PlayerStyleState {
    public string styleId;
    public int plusTier;
    public int minusTier;
}

// StyleManager.cs (skeleton)
public class StyleManager : MonoSingleton<StyleManager>
{
    public PlayerStyleState CurrentState { get; private set; } = new PlayerStyleState();

    // registry
    private Dictionary<string, StyleDefinition> defs = new();

    // per-combat runtime flags
    private bool firstCardUsedThisBattle;

    public event Action<PlayerStyleState> OnStyleChanged;
    public event Action<PlayerStyleState> OnStyleUpgraded;

    public void Initialize()
    {
        // 모든 문체 정보 로드 및 딕셔너리에 저장
        // 저장되어 있는, 종료 전의 문체 정보 로드
    }

    public void SetState(PlayerStyleState state)
    {
        CurrentState = state ?? new PlayerStyleState();
        OnStyleChanged?.Invoke(CurrentState);
    }

    public void ChangeStyle(string id) // 문체 변경
    {
        if (!defs.ContainsKey(id)) { Debug.LogWarning("Key for  current Style has not found"); return; }
        CurrentState.styleId = id;
        CurrentState.plusTier = Mathf.Clamp(CurrentState.plusTier, 0, defs[id].maxPlusTier);
        CurrentState.minusTier = Mathf.Clamp(CurrentState.minusTier, 0, defs[id].maxMinusTier);
        OnStyleChanged?.Invoke(CurrentState);
    }

    public bool UpgradePlus() // 문체 강화
    {
        if (CurrentState.styleId == null) return false;
        var def = defs[CurrentState.styleId];
        if (CurrentState.plusTier >= def.maxPlusTier) return false;
        CurrentState.plusTier++;
        OnStyleUpgraded?.Invoke(CurrentState);
        ProgressDataManager.Instance.SaveProgress(true);
        return true;
    }
}
