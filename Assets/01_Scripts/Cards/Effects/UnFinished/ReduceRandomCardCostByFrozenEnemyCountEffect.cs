using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Effects/ReduceRandomCardCostByFrozenEnemyCountEffect")]
public class ReduceRandomCardCostByFrozenEnemyCountEffect : CardEffectBase
{
    public int amountPerEnemy = 1;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (caster is not PlayerController pc)
            return;

        int frozenEnemyCount = GameManager.Instance.turnController.battleFlow.enemyParty
            .OfType<Enemy>()
            .Count(e => e.HasEffect(BuffStatType.Freeze));

        if (frozenEnemyCount <= 0)
            return;

        pc.Deck.ApplyDiscountToRandomCards(amountPerEnemy, frozenEnemyCount);
    }

    public override string GetDescription() => $"빙결 적 수만큼 무작위 카드 비용 감소";
}