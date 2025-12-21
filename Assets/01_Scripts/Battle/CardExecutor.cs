using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카드 사용 로직 처리
/// </summary>
public class CardExecutor
{
    private readonly Func<int> getCurrentMana;
    private readonly Action<int> setCurrentMana;
    private readonly Func<List<IStatusReceiver>> getPlayerParty;

    public event Action<CardModel, IStatusReceiver> OnCardUsed;

    public CardExecutor(
        Func<int> getCurrentMana,
        Action<int> setCurrentMana,
        Func<List<IStatusReceiver>> getPlayerParty)
    {
        this.getCurrentMana = getCurrentMana;
        this.setCurrentMana = setCurrentMana;
        this.getPlayerParty = getPlayerParty;
    }

    /// <summary>
    /// 카드 사용
    /// </summary>
    public bool Execute(CardModel card, IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        int currentMana = getCurrentMana();

        if (!card.IsUsable(currentMana) || !caster.IsAlive())
        {
            return false;
        }

        // 마나 소모
        int actualCost = card.GetEffectiveCost();
        setCurrentMana(currentMana - actualCost);

        // 일회성 할인 처리
        if (card.ConsumesDiscountOnce)
        {
            ClearAllTemporaryDiscounts();
        }

        // 카드 효과 실행
        int attackType = (int)card.type % 3;

        // 효과 2배 적용 확인 (소피아-통찰)
        int repeatCount = GetRepeatCount(caster);
        if (repeatCount > 1)
        {
            Debug.Log($"[통찰] {card.cardName} 효과 {repeatCount}회 적용!");
        }

        // 카드 플레이
        card.Play(caster, targets, attackType);

        // 카메라 액션
        caster.CameraActionPlay();

        // 핸드에서 사용 덱으로
        caster.Deck.Discard(card);

        // 이벤트 발생
        OnCardUsed?.Invoke(card, caster);

        // 덱 상태 출력
        if (caster is PlayerController pc)
            pc.PrintDeckState();

        return true;
    }

    /// <summary>
    /// 카드 사용 가능 여부 확인
    /// </summary>
    public bool CanUse(CardModel card, IStatusReceiver caster, IStatusReceiver target, int currentMana)
    {
        if (caster == null || target == null || card == null)
            return false;
        if (!card.IsUsable(currentMana))
            return false;
        if (!card.CanBeUsedBy(caster.ChClass))
            return false;
        if (!card.IsTargetValid(caster, target))
            return false;
        if (caster.IsStunned())
            return false;

        // 버릴 카드 부족하면 사용 불가
        foreach (var effect in card.effects)
        {
            if (effect is DiscardCardEffect discard)
            {
                if (caster.Deck.Hand.Count <= discard.discardCount)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private int GetRepeatCount(IStatusReceiver caster)
    {
        if (caster is PlayerController casterPC && casterPC.ShouldDoubleCardEffect())
        {
            return 2;
        }
        return 1;
    }

    private void ClearAllTemporaryDiscounts()
    {
        foreach (var player in getPlayerParty())
        {
            player.Deck.ClearAllTemporaryDiscounts();
        }
    }
}