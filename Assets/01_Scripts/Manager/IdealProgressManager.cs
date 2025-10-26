using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IdealData;

public partial class ProgressDataManager
{
    private Dictionary<string, int> _idealCounter = new();
    private HashSet<int> _unlockedIdealIds = new();

    // 임계값
    private const int ARCANE_RUNS_TO_UNLOCK = 2;
    private const int ALCHEMIST_BATTLES_TO_UNLOCK = 30;
    private const int SANCTIFY_RUNS_TO_UNLOCK = 2;
    private const int JUDGE_KILLS_TO_UNLOCK = 50;
    private const int LION_GUARD_TO_UNLOCK = 20;
    private const int SHADOW_DEBUFF_TO_UNLOCK = 40;

    public bool IsIdealUnlocked(IdealId id) => _unlockedIdealIds.Contains((int)id);
    public int GetCounter(string key) => _idealCounter.TryGetValue(key, out var v) ? v : 0;

    private void Bump(string key, int add)
    {
        if (add == 0) return;
        _idealCounter[key] = GetCounter(key) + add;
    }

    // 커넥터가 보내주는 δ 적용 + 해금 판정
    public List<IdealId> ApplyIdealDelta(IdealProgressDelta d)
    {
        if (d.IsEmpty) return s_empty;

        // 1) 누적
        Bump(IdealCounterKeys.Arcane_RunSuccess, d.Arcane_RunSuccess);
        Bump(IdealCounterKeys.Alchemist_BattleSuccess, d.Alchemist_BattleSuccess);
        Bump(IdealCounterKeys.Sanctify_RunSuccess, d.Sanctify_RunSuccess);
        Bump(IdealCounterKeys.Judge_Kills, d.Judge_Kills);
        Bump(IdealCounterKeys.Lionheart_Guard, d.Lionheart_Guard);
        Bump(IdealCounterKeys.Shadow_Debuff, d.Shadow_Debuff);

        // 2) 해금 판정
        var newly = new List<IdealId>(2);
        TryUnlock(IdealId.Sophia_ArcaneMage, IdealCounterKeys.Arcane_RunSuccess, ARCANE_RUNS_TO_UNLOCK, newly);
        TryUnlock(IdealId.Sophia_Alchemist, IdealCounterKeys.Alchemist_BattleSuccess, ALCHEMIST_BATTLES_TO_UNLOCK, newly);
        TryUnlock(IdealId.Kayla_Sanctify, IdealCounterKeys.Sanctify_RunSuccess, SANCTIFY_RUNS_TO_UNLOCK, newly);
        TryUnlock(IdealId.Kayla_Judge, IdealCounterKeys.Judge_Kills, JUDGE_KILLS_TO_UNLOCK, newly);
        TryUnlock(IdealId.Leon_Lionheart, IdealCounterKeys.Lionheart_Guard, LION_GUARD_TO_UNLOCK, newly);
        TryUnlock(IdealId.Leon_ShadowWarrior, IdealCounterKeys.Shadow_Debuff, SHADOW_DEBUFF_TO_UNLOCK, newly);

        SaveProgress(false); // 변경 저장
        return newly;
    }

    private static readonly List<IdealId> s_empty = new();
    private void TryUnlock(IdealId id, string key, int threshold, List<IdealId> outNew)
    {
        if (_unlockedIdealIds.Contains((int)id)) return;
        if (GetCounter(key) >= threshold)
        {
            _unlockedIdealIds.Add((int)id);
            outNew.Add(id);
        }
    }
}
