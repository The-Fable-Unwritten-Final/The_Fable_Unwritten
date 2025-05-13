using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DuplicateCardEffect : CardEffectBase
{
    public int duplicateNum = 1;
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if (caster is not PlayerController player) return;

        // 마지막으로 사용한 카드 ID 확인
        int lastCardID = BattleLogManager.Instance.GetLastCardUsed(caster);
        if (lastCardID == -1) 
        {
            Debug.LogWarning("[복제 실패] 최근 사용한 카드가 없습니다.");
            return; 
        }

        if (DataManager.Instance.CardLookup.TryGetValue(lastCardID, out var original))
        {
            for(int i = 0; i < duplicateNum; i++)
            {
                CardModel clone = Object.Instantiate(original);
                clone.name = original.name + "_Copy";
                clone.isOneUse = true;
                clone.isMaintain = false;

                player.Deck.AddToHand(clone);
            }

            Debug.Log($"[복제] {original.cardName} 카드가 {duplicateNum}장 복제되어 핸드에 추가됨");
        }
    }

    public override string GetDescription() => "hi";
}
