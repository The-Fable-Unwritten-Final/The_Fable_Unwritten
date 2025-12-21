using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeStanceEffect : CardEffectBase
{
    private StancType inputStance;
   public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
   {
        caster.ChangeStance(inputStance);
   }

    public override string GetDescription()
    {
        return $"스탠스 {inputStance.ToString()}교체";
    }
}
