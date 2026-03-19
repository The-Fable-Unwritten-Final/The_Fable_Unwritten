using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/IdealRealizationEffect")]
public class IdealRealizationEffect : CardEffectBase
{
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (targets == null || targets.Count == 0)
            return;

        foreach (var target in targets)
        {
            if (target is not PlayerController pc)
                continue;

            if (!IdealRealizationManager.Instance.TryUseIdeal(pc))
            {
                Debug.Log($"[IdealRealizationEffect] {pc.playerData.CharacterName} 은(는) 발동 가능한 이상실현이 없음");
                continue;
            }

            switch (pc.playerData.currentStance)
            {
                case StancType.Seek:
                    TriggerSeekIdeal(pc);
                    break;

                case StancType.Insight:
                    TriggerInsightIdeal(pc);
                    break;

                case StancType.Mercy:
                    TriggerMercyIdeal(pc);
                    break;

                case StancType.Discipline:
                    TriggerDisciplineIdeal(pc);
                    break;

                case StancType.Rush:
                    TriggerRushIdeal(pc);
                    break;

                case StancType.Defense:
                    TriggerDefenseIdeal(pc);
                    break;

                default:
                    Debug.LogWarning($"[IdealRealizationEffect] 알 수 없는 스탠스: {pc.playerData.currentStance}");
                    break;
            }
        }
    }

    public override string GetDescription()
    {
        return "지정한 아군의 현재 스탠스에 대응하는 이상실현 효과를 발동합니다.";
    }

    private void TriggerSeekIdeal(PlayerController pc)
    {
        Debug.Log("[Ideal] 탐구 이상실현 발동");
        // TODO: 실제 효과 구현
    }

    private void TriggerInsightIdeal(PlayerController pc)
    {
        Debug.Log("[Ideal] 통찰 이상실현 발동");
        // TODO: 실제 효과 구현
    }

    private void TriggerMercyIdeal(PlayerController pc)
    {
        Debug.Log("[Ideal] 자비 이상실현 발동");
        // TODO: 실제 효과 구현
    }

    private void TriggerDisciplineIdeal(PlayerController pc)
    {
        Debug.Log("[Ideal] 규율 이상실현 발동");
        // TODO: 실제 효과 구현
    }

    private void TriggerRushIdeal(PlayerController pc)
    {
        Debug.Log("[Ideal] 돌진 이상실현 발동");
        // TODO: 실제 효과 구현
    }

    private void TriggerDefenseIdeal(PlayerController pc)
    {
        Debug.Log("[Ideal] 수비 이상실현 발동");
        // TODO: 실제 효과 구현
    }
}