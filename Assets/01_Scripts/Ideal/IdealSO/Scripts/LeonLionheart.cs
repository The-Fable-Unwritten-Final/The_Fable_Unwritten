using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IdealData;

[CreateAssetMenu(menuName = "Fable/Ideal/Leon_Lionheart")]
public class LeonLionheart : IdealSkillBase
{

    public override string DisplayName => "라이온하트";

    public override void Activate(PlayerController owner)
    {
        owner.ApplyStatusEffect(new InstanceEffect
        {
            statType = BuffStatType.GuardRedirect,
            value = 50,
            isMaintain = true // 전투 지속
        });

        // 필요하면 Guard Redirect 최소 보장 훅 등록
        // Rules.RegisterGuardFloor(owner, 50);

        owner.MarkIdealThisStage();
    }
}