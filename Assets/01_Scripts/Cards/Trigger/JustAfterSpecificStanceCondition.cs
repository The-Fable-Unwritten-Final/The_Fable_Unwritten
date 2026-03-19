using System.Collections.Generic;
using UnityEngine;
using static PlayerData;

[CreateAssetMenu(menuName = "CardEffects/Conditions/JustAfterSpecificStanceCondition")]
public class JustAfterSpecificStanceCondition : TriggerCondition
{
    public StancType requiredStance;

    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> target)
    {
        if (caster is PlayerController pc)
        {
            return pc.justTriggeredStanceThisTurn && pc.playerData.currentStance == requiredStance;
        }

        return false;
    }

    public override string Description => $"{requiredStance} 스탠스 발동 직후일 때";
}