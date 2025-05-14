using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cards/Effects/RecycleCardEffect")]
public class RecycleCardEffect : CardEffectBase
{

    public int amount = 1;    //되돌릴 카드 수

    /// <summary>
    /// apply는 경고 문구 출력
    /// </summary>
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        Debug.LogWarning("[DiscardCardEffect] 선택된 카드 리스트를 넘겨주세요.");
    }

    // 카드 효과 실행 시 경고 문구 출력 이후 UI 쪽에서 ApplyWithSelection을 출력해주자!!!!!

    /// <summary>
    /// 외부에서 특정 카드들을 선택했을 경우 그 카드를 되돌리는 선택 기반 리사이클
    /// </summary>
    public void ApplyWithSelection(IStatusReceiver caster, List<CardModel> selectedCards)
    {
        var deck = caster.Deck;
        if (deck == null) return;

        int allowedCount = Mathf.Min(amount, selectedCards.Count);
        int added = 0;

        foreach (var card in selectedCards)
        {
            if (deck.GetUsedCards().Contains(card))
            {
                deck.AddToHand(card);
                deck.RemoveFromUsed(card);
                added++;

                if (added >= allowedCount) break;
            }
        }
        Debug.Log($"[Recycle] {added}장 손패로 되돌림");
    }

    public override string GetDescription() => $"사용한 카드 중 {amount}장까지 선택하여 손패로 되돌립니다.";
}