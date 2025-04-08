[System.Serializable]
public class StatusEffect
{
    public BuffStatType statType;       //어떤 버프이고   
    public float value;                   //얼마나 적용되고
    public int duration;                //몇턴 적용되는지 표시

    public bool isPositive => value >= 0;   // 버프가 적용 중인지 표시. 
}
