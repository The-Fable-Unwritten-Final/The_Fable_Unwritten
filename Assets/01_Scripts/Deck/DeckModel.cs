using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class DeckModel
{
    public List<CardModel> unusedDeck = new();           //사용 안한 카드
    public List<CardModel> usedDeck = new();            //사용한 카드
    public List<CardModel> hand = new();                //들고 있는 카드

    public IReadOnlyList<CardModel> Hand => hand;
    public const int maxSize = 200;
    public const int startSize = 3;

    //덱 삽입하기
    public void Initialize(List<CardModel> cards)
    {
        unusedDeck = new List<CardModel>(cards);
        Shuffle(unusedDeck);
        usedDeck.Clear();
        hand.Clear();

        // 문체 효과 적용 => 첫 카드 코스트 변환
        foreach (var card in unusedDeck)
        {
            int modifiedCost = StyleManager.Instance.GetFirstCardCostModifier(card, 0); // 디폴트 값 0
            card.ApplyTemporaryDiscount(modifiedCost);
        }
    }

    private Dictionary<int, int> evolvedCardIds = new();

    /// <summary>
    /// 카드 드로우 시 행동
    /// </summary>
    /// <param name="count">얼마나 드로우 할지 결정</param>
    public void Draw(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (hand.Count >= maxSize) break;   //5장 모두 들고 있을 때 드로우 시도 시 처리 안함

            if (unusedDeck.Count == 0)          //모든 카드 사용 시
                ReshuffleDiscardIntoDraw();
                
            var card = unusedDeck[0];           //미사용 덱의 가장 앞의 카드를
            unusedDeck.RemoveAt(0);
            hand.Add(card);                     //핸드에 넣기
            GameManager.Instance.combatUIController.DrawCard(card);
            BattleLogManager.Instance.RegisterDrawnCard(card);
        }
    }

    /// <summary>
    /// 특정 카드 버리기
    /// </summary>
    /// <param name="card">버릴 카드</param>
    public void Discard(CardModel card)
    {
        if (hand.Remove(card))
        {
            if (card.isOneUse)
            {
                GameObject.Destroy(card);
                Debug.Log($"[Deck] 일회용 카드 {card.cardName} 파괴됨");
            }
            else
            {
                usedDeck.Add(ResolveEvolvedCard(card));
            }
        }
    }

    /// <summary>
    /// 카드를 4장 이상으로 가지고 있을 경우 3장으로 맞추기
    /// </summary>
    public void DiscardHandToThree(List<CardModel> cardsToDiscard)
    {
        int limit = startSize;
        int discardCountNeeded = hand.Count - limit;

        if (discardCountNeeded <= 0) return;

        int discarded = 0;
        foreach (var card in cardsToDiscard)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
                usedDeck.Add(ResolveEvolvedCard(card));
                discarded++;

                if (discarded >= discardCountNeeded)
                    break;
            }
        }
        Debug.Log($"[Discard] 선택 카드 {discarded}장 버림, 현재 손패 {hand.Count}장");
    }

    /// <summary>
    /// 핸드 모두 버리기
    /// </summary>
    public void DiscardHand()
    {
        foreach (var card in hand)
            usedDeck.Add(ResolveEvolvedCard(card));

        hand.Clear();
    }

    /// <summary>
    /// 카드를 전부 사용했을 때 사용한 덱을 미사용 덱으로 이동 후 셔플하기
    /// </summary>
    public void ReshuffleDiscardIntoDraw()
    {
        unusedDeck.AddRange(usedDeck);
        usedDeck.Clear();
        Shuffle(unusedDeck);
    }

    /// <summary>
    /// 덱 셔플
    /// </summary>
    /// <param name="list">셔플할 리스트</param>
    private void Shuffle(List<CardModel> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// 사용 카드 수를 반환
    /// </summary>
    /// <returns>사용된 카드 수</returns>
    public int UsedCount() => usedDeck.Count;

    /// <summary>
    /// 사용 카드 덱의 복사본 반환
    /// </summary>
    /// <returns>사용 카드 덱</returns>
    public List<CardModel> GetUsedCards()
    {
        return new List<CardModel>(usedDeck); // 복사본 반환
    }

    /// <summary>
    /// 사용 덱에서 제거
    /// </summary>
    /// <param name="card">제거할 카드</param>
    public void RemoveFromUsed(CardModel card)
    {
        usedDeck.Remove(card);
    }

    /// <summary>
    /// 손패에 특정 카드를 추가
    /// </summary>
    /// <param name="card">손패에 추가할 카드</param>
    public void AddToHand(CardModel card)
    {
        if (hand.Count < maxSize)
        {
            hand.Add(card);
        }
    }

    /// <summary>
    /// 덱 자체에 일시적 할인 부여
    /// </summary>
    /// <param name="amount">할인 량</param>
    public void ApplyTemporaryDiscountToAllCards(int amount)
    {
        foreach (var card in hand)
            card.ApplyTemporaryDiscount(amount);
        foreach (var card in unusedDeck)
            card.ApplyTemporaryDiscount(amount);
        foreach (var card in usedDeck)
            card.ApplyTemporaryDiscount(amount);
    }
    /// <summary>
    /// 덱에 일시적 할인 삭제
    /// </summary>
    public void ClearAllTemporaryDiscounts()
    {
        foreach (var card in hand)
            card.ClearTemporaryDiscount();
        foreach (var card in unusedDeck)
            card.ClearTemporaryDiscount();
        foreach (var card in usedDeck)
            card.ClearTemporaryDiscount();
    }

    

    /// <summary>
    /// 게임 시작 시 전체 덱 상태 초기화
    /// (핸드와 사용 덱 모두 미사용 덱으로 병합 후 셔플)
    /// </summary>
    public void ResetDeckState()
    {
        foreach (var card in hand)
            unusedDeck.Add(ResolveEvolvedCard(card));

        foreach (var card in usedDeck)
            unusedDeck.Add(ResolveEvolvedCard(card));

        hand.Clear();
        usedDeck.Clear();

        Shuffle(unusedDeck);

        //Debug.Log($"[DeckModel] 덱 초기화 완료. 카드 수: {unusedDeck.Count}");
    }


    /// <summary>
    /// 카드가 지속적 할인을 받을 자격이 있는지 판단
    /// (특정 키워드를 가진 카드는 제외)
    /// </summary>
    private bool ShouldApplyPersistentDiscount(CardModel card)
    {
        // 'copy' 키워드를 가진 카드는 할인 적용 안함
        return !card.HasKeyword("copy");
    }

    /// <summary>
    /// '모든 카드' 대상 amount 만큼 할인 적용
    /// </summary>
    public void ApplyPersistentDiscountToAllCards(int amount)
    {
        foreach (var card in hand)
            if (ShouldApplyPersistentDiscount(card))
                card.ApplyPersistentDiscount(amount);
        foreach (var card in unusedDeck)
            if (ShouldApplyPersistentDiscount(card))
                card.ApplyPersistentDiscount(amount);
        foreach (var card in usedDeck)
            if (ShouldApplyPersistentDiscount(card))
                card.ApplyPersistentDiscount(amount);
    }
    /// <summary>
    ///  '특정 타입'의 카드 대상 할인 적용
    /// </summary>
    public void ApplyPersistentDiscountByCardType(CardType targetType, int amount)
    {
        foreach (var card in hand)
            if (card.type == targetType && ShouldApplyPersistentDiscount(card))
                card.ApplyPersistentDiscount(amount);
        
        foreach (var card in unusedDeck)
            if (card.type == targetType && ShouldApplyPersistentDiscount(card))
                card.ApplyPersistentDiscount(amount);
        
        foreach (var card in usedDeck)
            if (card.type == targetType && ShouldApplyPersistentDiscount(card))
                card.ApplyPersistentDiscount(amount);
    }

    public void DiscardUnmaintainedCardsAtTurnEnd()
    {
        List<CardModel> toRemove = new();

        // 핸드 검사
        foreach (var card in hand)
        {
            if (!card.isMaintain)
            {
                toRemove.Add(card);
            }
        }

        foreach (var card in toRemove)
        {
            GameManager.Instance.combatUIController.ThrowCard(card);
            Debug.Log($"[Deck] 유지되지 않는 카드 {card.cardName} 핸드에서 제거");
        }

        // 사용 덱 검사
        toRemove.Clear();
        foreach (var card in usedDeck)
        {
            if (!card.isMaintain)
                toRemove.Add(card);
        }

        foreach (var card in toRemove)
        {
            usedDeck.Remove(card);
            GameObject.Destroy(card);
            Debug.Log($"[Deck] 유지되지 않는 카드 {card.cardName} 사용 덱에서 제거");
        }

    }

    public bool AddToHandRightmost(CardModel card, bool updateUI = true)
    {
        if (card == null)
            return false;

        if (hand.Count >= maxSize)
            return false;

        hand.Add(card);

        if (updateUI && GameManager.Instance != null && GameManager.Instance.combatUIController != null)
        {
            GameManager.Instance.combatUIController.DrawCard(card);
        }

        return true;
    }

    public List<CardModel> DrawAndReturn(int count)
    {
        List<CardModel> drawn = new();

        for (int i = 0; i < count; i++)
        {
            if (hand.Count >= maxSize)
                break;

            if (unusedDeck.Count == 0)
                ReshuffleDiscardIntoDraw();

            if (unusedDeck.Count == 0)
                break;

            var card = unusedDeck[0];
            unusedDeck.RemoveAt(0);

            hand.Add(card);
            drawn.Add(card);

            if (GameManager.Instance != null && GameManager.Instance.combatUIController != null)
                GameManager.Instance.combatUIController.DrawCard(card);

            if (BattleLogManager.Instance != null)
                BattleLogManager.Instance.RegisterDrawnCard(card);
        }

        return drawn;
    }

    public bool HasCardInHandByIndex(int cardIndex)
    {
        foreach (var card in hand)
        {
            if (card != null && card.index == cardIndex)
                return true;
        }

        return false;
    }

    public CardModel FindHandCardByIndex(int cardIndex)
    {
        foreach (var card in hand)
        {
            if (card != null && card.index == cardIndex)
                return card;
        }

        return null;
    }

    public bool RemoveFromHand(CardModel card)
    {
        if (card == null)
            return false;

        return hand.Remove(card);
    }

    public void ApplyDiscountToRandomCards(int amount, int count)
    {
        List<CardModel> pool = new();
        pool.AddRange(hand);
        pool.AddRange(unusedDeck);

        if (pool.Count == 0 || count <= 0)
            return;

        Shuffle(pool);

        int applyCount = Mathf.Min(count, pool.Count);
        for (int i = 0; i < applyCount; i++)
        {
            pool[i].ApplyTemporaryDiscount(amount);
        }
    }



    public void EvolveAllCards(int cardIndex, int evolveTarget)
    {
        var evolvedBase = DataManager.Instance.AllCards.Find(c => c.index == evolveTarget);

        if (evolvedBase == null)
        {
            Debug.LogWarning($"[CardEvolve] 진화 대상 없음: {cardIndex} -> {evolveTarget}");
            return;
        }

        evolvedCardIds[cardIndex] = evolveTarget;

        ReplaceEvolvedCards(unusedDeck, cardIndex, evolvedBase);
        ReplaceEvolvedCards(usedDeck, cardIndex, evolvedBase);

        Debug.Log($"[CardEvolve] {cardIndex} -> {evolveTarget} 진화 완료");
    }

    private void ReplaceEvolvedCards(List<CardModel> deck, int cardIndex, CardModel evolvedBase)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i] == null || deck[i].index != cardIndex)
                continue;

            deck[i] = evolvedBase.Clone();
        }
    }

    private CardModel ResolveEvolvedCard(CardModel card)
    {
        if (card == null)
            return null;

        if (!evolvedCardIds.TryGetValue(card.index, out int evolveTarget))
            return card;

        var evolvedBase = DataManager.Instance.AllCards.Find(c => c.index == evolveTarget);

        if (evolvedBase == null)
        {
            Debug.LogWarning($"[CardEvolve] 진화 대상 없음: {card.index} -> {evolveTarget}");
            return card;
        }

        return evolvedBase.Clone();
    }
}