using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DuplicateCardEffect : CardEffectBase
{
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if (caster is not PlayerController player) return;

        // 마지막으로 사용한 카드 ID 확인
        int lastCardID = BattleLogManager.Instance.GetLastCardUsed(caster);
        if (lastCardID == -1) return;

        if (CardSystemInitializer.Instance.cardLookup.TryGetValue(lastCardID, out var original))
        {
            CardModel clone = Object.Instantiate(original);
            clone.name = original.name + "_Copy";
            clone.isOneUse = true;

            player.Deck.AddToHand(clone);

            Debug.Log($"[복제] {original.cardName} 카드가 복제되어 핸드에 추가됨");
        }
    }

    public override string GetDescription() => "hi";
}
