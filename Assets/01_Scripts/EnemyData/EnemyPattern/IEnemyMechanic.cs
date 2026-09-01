using System.Collections.Generic;

public interface IEnemyMechanic
{
    void Initialize(Enemy owner, BattleFlowController battleFlow);

    void OnBattleStart();

    void OnPlayerTurnStart();
    void OnPlayerTurnEnd();

    void OnEnemyTurnStart();
    void OnEnemyTurnEnd();

    void OnDamaged(DamageContext context);

    void OnBurnDamageTaken(float damage);

    void OnCardUsed(PlayerController caster, CardModel card, IReadOnlyList<IStatusReceiver> targets, int activationDiscount);

    EnemySkill GetForcedSkill();

    float ModifyStat(BuffStatType statType, float value);

    bool SkipNormalActionThisTurn { get; }

    int ModifySkillEffectValue(IStatusReceiver target, EnemyEffectType effectType, EnemyEffectTarget targetType, int value);
}