using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterEventEffects : EventEffects
{
    public override void Apply()
    {
        Debug.Log("EncounterEvent");
    }
    public override void UnApply()
    {
    }
    public override EventEffects Clone()
    {
        return new EncounterEventEffects
        {
            index = this.index,
            text = this.text,
            eventType = this.eventType,
            duration = this.duration
        };
    }
}
