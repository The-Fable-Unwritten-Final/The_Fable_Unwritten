using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DeckModel
{
    public List<CardModel> unusedDeck = new();           //사용 안한 카드
    public List<CardModel> usedDeck = new();            //사용한 카드
    public List<CardModel> hand = new();                //들고 있는 카드

    public IReadOnlyList<CardModel> Hand => hand;
    public const int maxSize = 5;
    public const int startSize = 3;

    //덱 삽입하기
    public void Initialize(List<CardModel> cards)
    {
        unusedDeck = new List<CardModel>(cards);
        Shuffle(unusedDeck);
        usedDeck.Clear();
        hand.Clear();
    }


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
        }
        
    }

    /// <summary>
    /// 특정 카드 버리기
    /// </summary>
    /// <param name="card">버릴 카드</param>
    public void Discard(CardModel card)
    {
        if (hand.Remove(card))                  //핸드에 특정 카드 버리고
            usedDeck.Add(card);                 //사용한 카드 쪽에 버린 카드 넣기
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
                usedDeck.Add(card);
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
        usedDeck.AddRange(hand);                //들고 있는 카드 전부 사용 덱으로
        hand.Clear();                           //핸드를 비움
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
        unusedDeck.AddRange(hand);
        unusedDeck.AddRange(usedDeck);

        hand.Clear();
        usedDeck.Clear();

        Shuffle(unusedDeck);

        Debug.Log($"[DeckModel] 덱 초기화 완료. 카드 수: {unusedDeck.Count}");
    }


    public void ApplyPersistentDiscountToAllCards(int amount)
    {
        foreach (var card in hand)
            card.ApplyPersistentDiscount(amount);
        foreach (var card in unusedDeck)
            card.ApplyPersistentDiscount(amount);
        foreach (var card in usedDeck)
            card.ApplyPersistentDiscount(amount);
    }

}