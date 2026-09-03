using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Ideal Realization")]
public class IdealRealizationCardEffect : CardEffectBase
{
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if (caster is not PlayerController player)
        {
            Debug.LogWarning("[IdealRealizationCardEffect] caster가 PlayerController가 아님");
            return;
        }

        CardModel currentCard = BattleLogManager.Instance.card;

        if (currentCard == null)
        {
            Debug.LogWarning("[IdealRealizationCardEffect] 현재 사용 중인 카드를 찾을 수 없음");
            return;
        }

        bool success = IdealRealizationManager.Instance.TryUseIdeal(player, currentCard);

        if (!success)
        {
            Debug.LogWarning($"[IdealRealizationCardEffect] " + $"{player.playerData.CharacterName} 이상실현 발동 실패");
        }
    }

    public override string GetDescription()
    {
        return "이상실현을 발동합니다.";
    }
}