using System.Collections.Generic;
using UnityEngine;
using static PlayerData;

[CreateAssetMenu(menuName = "Cards/Effects/TriggerOppositeStanceEffect")]
public class TriggerOppositeStanceEffect : CardEffectBase
{
    public int value = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (caster is not PlayerController pc) return;
        if (GameManager.Instance == null || GameManager.Instance.turnController == null || GameManager.Instance.turnController.battleFlow == null)
            return;

        StancType current = pc.playerData.currentStance;
        StancType opposite = current switch
        {
            StancType.Seek => StancType.Insight,
            StancType.Insight => StancType.Seek,
            StancType.Mercy => StancType.Discipline,
            StancType.Discipline => StancType.Mercy,
            StancType.Rush => StancType.Defense,
            StancType.Defense => StancType.Rush,
            _ => current
        };

        pc.playerData.currentStance = opposite;
        StanceEffectHandler.TriggerStanceEffect(pc, GameManager.Instance.turnController.battleFlow);
        pc.playerData.currentStance = current;
    }

    public override string GetDescription()
    {
        return "현재 스탠스의 반대 효과를 발동합니다.";
    }
}