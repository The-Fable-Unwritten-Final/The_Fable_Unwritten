using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//카드 버리기 랜덤 아님. 턴 종료시 버리기와 함께 수정 필요
/// <summary>
/// 카드를 버리는 효과 적용 시
/// </summary>
[CreateAssetMenu(menuName = "CardEffect/DiscardCardEffect")]
public class DiscardCardEffect : CardEffectBase
{
    public int discardCount = 1;

    /// <summary>
    /// UI에서 선택한 카드들 버리기 처리 (패가 부족하면 전체 버림)
    /// </summary>
    public void Apply(IStatusReceiver caster, List<CardModel> selectedCards)
    {
        var deck = caster.Deck;

        if (deck.hand.Count > discardCount)     //버리는 카드가 핸드의 카드보다 많을 경우
        {
            deck.DiscardHand();
            Debug.Log("[카드 버리기] 패 전부 버리기");
        }
        else                                    //버리는 카드가 핸드의 카드보다 적을 경우
        {
            if(selectedCards.Count < discardCount)
            {
                Debug.LogWarning($"[DiscardCardEffect] 선택된 카드 수가 부족합니다. 필요: {discardCount}, 선택됨: {selectedCards.Count}");
            }

            foreach (var card in selectedCards)
            {
                if (deck.Hand.Contains(card))
                    deck.Discard(card);
            }

            Debug.Log($"[카드 버리기] {selectedCards.Count}장 선택적으로 버림");
        }
    }

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        Debug.LogWarning("[DiscardCardEffect] 선택된 카드 리스트를 넘겨주세요. 자동 버리기는 ApplyAuto()를 사용하세요.");
    }

    public override string GetDescription() => $"카드를 선택하여 최대 {discardCount}장 버립니다.";

    public override void ApplyAOE(IStatusReceiver caster, List<IStatusReceiver> targets) { }
    public override bool isAOE() => false;
}
