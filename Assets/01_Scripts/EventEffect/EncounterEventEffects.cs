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
        return Instantiate(this);
    }
}
