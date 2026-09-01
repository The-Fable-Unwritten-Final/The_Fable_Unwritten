
using System.Collections.Generic;

public class PotentialTriggeredCondition : TriggerCondition
{
    public override bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        if (caster is not PlayerController pc || pc.StanceSystem == null)
            return false;

        return pc.StanceSystem.PotentialTriggeredThisTurn;
    }

    public override string Description => "이번 턴 포텐셜이 발동했을 때";
}