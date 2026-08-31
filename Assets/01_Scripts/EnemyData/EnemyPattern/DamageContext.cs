public class DamageContext
{
    public IStatusReceiver Attacker;
    public IStatusReceiver Target;

    public CardModel SourceCard;

    public float RawDamage;
    public float FinalDamage;

    public bool IsAttackDamage;
    public bool IsTrueDamage;

    public int HitIndex;
}