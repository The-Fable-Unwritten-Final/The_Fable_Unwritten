[System.Serializable]
public class EnemySkill
{
    public int skillIndex;
    public float damage;
    public float percentage;

    public EnemySkill Clone()
    {
        return new EnemySkill
        {
            skillIndex = this.skillIndex,
            damage = this.damage,
            percentage = this.percentage
            // 필요한 필드 전부 복사
        };
    }
}