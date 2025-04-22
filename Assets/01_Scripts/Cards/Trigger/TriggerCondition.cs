using UnityEngine;

public abstract class TriggerCondition : ScriptableObject
{
    public abstract bool IsConditionMet(IStatusReceiver caster, IStatusReceiver target);
    public abstract string Description { get; }
}
