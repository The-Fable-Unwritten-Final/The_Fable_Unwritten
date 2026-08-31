using System.Collections.Generic;

public abstract class EnemyMechanicBase : IEnemyMechanic
{
    protected Enemy owner;
    protected BattleFlowController battleFlow;

    public virtual void Initialize(
        Enemy owner,
        BattleFlowController battleFlow)
    {
        this.owner = owner;
        this.battleFlow = battleFlow;
    }

    public virtual void OnBattleStart() { }

    public virtual void OnPlayerTurnStart() { }
    public virtual void OnPlayerTurnEnd() { }

    public virtual void OnEnemyTurnStart() { }
    public virtual void OnEnemyTurnEnd() { }

    public virtual void OnDamaged(DamageContext context) { }

    public virtual void OnCardUsed(
        PlayerController caster,
        CardModel card,
        IReadOnlyList<IStatusReceiver> targets,
        int activationDiscount)
    {
    }

    public virtual void OnBurnDamageTaken(float damage) { }

    public virtual EnemySkill GetForcedSkill()
    {
        return null;
    }

    public virtual float ModifyStat(BuffStatType statType, float value)
    {
        return value;
    }
}