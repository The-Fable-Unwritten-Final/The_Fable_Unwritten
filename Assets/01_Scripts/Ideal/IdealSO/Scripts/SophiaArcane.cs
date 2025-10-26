using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IdealData;

[CreateAssetMenu(menuName = "Fable/Ideal/Sophia_Arcane")]
public class SophiaArcane : IdealSkillBase
{

    public override string DisplayName => "비전술사";

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