
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
    public bool isMaintain = false;
}