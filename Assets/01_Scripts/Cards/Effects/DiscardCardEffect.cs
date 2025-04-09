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
    // 실제 선택은 외부에서 UI로 받고 여기서 처리
    public void Apply(IStatusReceiver caster, List<CardModel> selectedCards)
    {
        var deck = caster.Deck;

        foreach (var card in selectedCards)
        {
            if (deck.Hand.Contains(card))
                deck.Discard(card);
        }

        Debug.Log($"[카드 버리기] {selectedCards.Count}장 선택적으로 버림");
    }

    // 기존 Apply는 경고만
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        Debug.LogWarning("[DiscardCardEffect] 직접 타겟 지정이 필요 없는 효과입니다. 선택된 카드 리스트를 넘겨주세요.");
    }

    public override string GetDescription() => $"카드를 {discardCount}장 버립니다 (선택).";
}
