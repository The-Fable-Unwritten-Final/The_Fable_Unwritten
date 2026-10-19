using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ideal Realization")]
public class IdealRealizationData : ScriptableObject
{
    public IdealRealizationType type;
    public CharacterClass characterClass;
    public StancType requiredStance;

    public string realizationName;

    [TextArea]
    public string description;

    public Sprite illustration;

//    public IdealRealizationEffectBase activeEffect;
}