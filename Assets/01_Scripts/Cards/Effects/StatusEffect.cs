
[System.Serializable]
public abstract class StatusEffect
{
    public BuffStatType statType;       //어떤 버프이고   
    public float value;                   //얼마나 적용되고
}
[System.Serializable]
public class TickEffect:StatusEffect
{
    public int duration;                //몇턴 적용되는지 표시
}
[System.Serializable]
public class InstanceEffect:StatusEffect
{
    public bool isMaintain = false;     //효과 발동 이후 유지할지 표시
}

/// <summary>
/// 효과 발동 시점
/// </summary>
public enum EffectTriggerType
{
    None,
    TurnStart,      // 턴 시작
    TurnEnd,        // 턴 종료
    OnHit,          // 피격 시
    OnAttack,       // 공격 시
    OnApply,        // 효과 적용 시
    OnHeal          // 회복 시
}