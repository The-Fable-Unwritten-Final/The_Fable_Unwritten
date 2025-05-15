using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "EffectAnimationDatabase", menuName = "Effect/EffectAnimationDatabase")]
public class EffectAnimationDatabase : ScriptableObject
{
    public List<EffectAnimation> allAnimations = new();

    public Dictionary<string, EffectAnimation> ToDictionary()
    {
        return allAnimations.ToDictionary(a => a.animationName, a => a);
    }

    public List<EffectAnimation> GetByType(AnimationType type)
    {
        return allAnimations.FindAll(a => a.animationType == type);
    }
}