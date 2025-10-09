using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fable/Ideal/Sophia_Arcane")]
public class SophiaArcane : IdealSkillBase
{
    
    public override string DisplayName => "비전술사";
    public override IdealAvailability GetUseInfo(PlayerController owner)
    {
        if (isUnlocked(Id)) return IdealAvailability.Locked;
        if (owner.IsIdealUsed) return IdealAvailability.Used;
        if (!owner.IsAlive() || owner.IsStunned()) return IdealAvailability.Unavailable;
        return IdealAvailability.Available;
    }

    public override void Activate(PlayerController owner)
    {
        // 카드 비용 -1 (당신 프로젝트의 임시할인 API에 맞춰 조정)
        foreach (var c in owner.Deck.Hand)
            c.ApplyTemporaryDiscount(1); // 없으면 구현 or 대체 API 사용

        // 단일 허가 모드/보라 하이라이트가 있다면 여기서 on
        // CardRules.SetSinglePlayableMode(true); CardRules.PickAndAllowOneRandomCard();

        owner.MarkIdealThisStage();

        // 연출/사운드 호출 등
    }
}
