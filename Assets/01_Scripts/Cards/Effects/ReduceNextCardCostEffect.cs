using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/ReduceNextCardCostEffect")]
public class ReduceNextCardCostEffect : CardEffectBase
{
    public int amount;
    public bool onlyOneCard = true;

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        var flow = GameManager.Instance?.turnController.battleFlow;
        if (flow == null) return;

        if (target.ChClass == CharacterClass.Enemy)
        {
            foreach (var ally in flow.playerParty)      //파티 전체 코스트 감소
            {
                ApplyDiscountToHand(ally);
            }
        }
        else
        {
            ApplyDiscountToHand(target);            //한 사람만 코스트 감소
        }
    }

    private void ApplyDiscountToHand(IStatusReceiver receiver)
    {
        if (onlyOneCard)
        {
            receiver.Deck.ApplyTemporaryDiscountToAllCards(amount);
        }
        else
        {
            receiver.Deck.ApplyPersistentDiscountToAllCards(amount);
        }
    }

    public override string GetDescription() => onlyOneCard
        ? $"대상의 다음 카드 1장의 코스트를 {amount} 감소시킵니다."
        : $"대상의 모든 카드 코스트를 {amount} 감소시킵니다.";

}