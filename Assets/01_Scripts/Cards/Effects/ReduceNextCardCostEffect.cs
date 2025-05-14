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
    public int? target;  //0 : sophia, 1 : kalya, 2 : leon, 3 : target, 4 : drawCard

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        var flow = GameManager.Instance?.turnController.battleFlow;
        if (flow == null) return;

        List<IStatusReceiver> filteredTargets = new();

        switch (target)
        {
            case 0: // 소피아
                foreach (var p in flow.playerParty)
                    if (p.ChClass == CharacterClass.Sophia) filteredTargets.Add(p);
                break;

            case 1: // 카일라
                foreach (var p in flow.playerParty)
                    if (p.ChClass == CharacterClass.Kayla) filteredTargets.Add(p);
                break;

            case 2: // 레온
                foreach (var p in flow.playerParty)
                    if (p.ChClass == CharacterClass.Leon) filteredTargets.Add(p);
                break;

            case 3: // 모든 대상
                filteredTargets.AddRange(targets);
                break;

            case 4: // 타겟 한 명
            case null:
                if (targets.Count > 0) filteredTargets.Add(targets[0]);
                break;

            case 5: // 드로우한 카드에만 적용 (예외 처리 필요)
                    // 이 효과는 DrawCardEffect에서 동적으로 적용해야 하므로 여기선 무시
                Debug.LogWarning("[ReduceNextCardCostEffect] DrawCard 조건은 DrawCardEffect에서 처리되어야 합니다.");
                return;

            default:
                filteredTargets.AddRange(targets);
                break;
        }

        foreach (var t in filteredTargets)
        {
            ApplyDiscountToHand(t);
        }
    }

    public void ApplyDiscountToHand(IStatusReceiver receiver)
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