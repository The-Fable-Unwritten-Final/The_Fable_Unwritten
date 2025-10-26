using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IdealProgressCheck : MonoSingleton<IdealProgressCheck>
{
    private const int ARCANE_SAME_TYPE_PER_RUN = 30;   // 비전술사: 회차 중 같은 타입 30회
    private const int ALCHEMIST_TYPES_PER_BATTLE = 3;  // 연금술사: 전투에서 서로 다른 3타입
    private const float SANCTIFY_KAYLA_DMG_PER_RUN = 100f; // 성화: 회차 중 카일라 누적 100

    BattleLogManager log => BattleLogManager.Instance;
    ProgressDataManager pData => ProgressDataManager.Instance;

    // <summary>
    /// 전투 종료 시 호출: 배틀 단위 조건을 BattleLog에서 읽어 δ 산출 → PD에 전달
    /// </summary>
    public void ProcessBattleEnd()
    {
        var d = new IdealProgressDelta();

        // 연금술사: 전투에서 서로 다른 3타입
        var types = new HashSet<CardType>();
        foreach (var use in log.UsedCardsForBattle) types.Add(use.type);
        if (types.Count >= ALCHEMIST_TYPES_PER_BATTLE)
            d.Alchemist_BattleSuccess = 1;

        // 전투 단위 누적형
        d.Judge_Kills = log.KaylaKills;
        d.Lionheart_Guard = log.GuardTriggers;
        d.Shadow_Debuff = log.LeonDebuffs;

        var newly = pData.ApplyIdealDelta(d);
        // (선택) newly에 따라 해금 팝업/연출 호출

        log.ResetForBattle(); // 원자료 초기화(전투 범위)
        log.ResetBattleLog();
    }

    /// <summary>
    /// 회차 종료 시 호출: 회차 단위 조건을 BattleLog에서 읽어 δ 산출 → PD에 전달
    /// </summary>
    public void ProcessRunEnd()
    {
        var d = new IdealProgressDelta();

        // 비전술사: 회차 중 같은 타입 30회 이상
        var countByType = new Dictionary<CardType, int>();
        foreach (var use in log.UsedCardsForGame)
        {
            if (!countByType.TryGetValue(use.type, out var c)) c = 0;
            countByType[use.type] = c + 1;
        }
        if (countByType.Values.Any(v => v >= ARCANE_SAME_TYPE_PER_RUN))
            d.Arcane_RunSuccess = 1;


        // 성화: 카일라 피해 100
        if (log.KaylaDamage >= SANCTIFY_KAYLA_DMG_PER_RUN)
            d.Sanctify_RunSuccess = 1;

        var newly = pData.ApplyIdealDelta(d);
        // TODO: 해금 팝업/연출 (newly)

        log.ResetForRun(); // 원자료 초기화(회차 범위 + 전투 범위)
        log.ResetGameLog();
    }
}
