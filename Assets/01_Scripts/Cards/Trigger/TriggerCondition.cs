using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerCondition : ScriptableObject
{
    public abstract bool IsConditionMet(IStatusReceiver caster, List<IStatusReceiver> targets);
    public abstract string Description { get; }
}
