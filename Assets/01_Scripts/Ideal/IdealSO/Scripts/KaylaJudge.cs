using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IdealData;

[CreateAssetMenu(menuName = "Fable/Ideal/Kayla_Judge")]
public class KaylaJudge : IdealSkillBase
{
    public override string DisplayName => "심판자";


    public override void Activate(PlayerController owner)
    {
        var turn = GameManager.Instance.turnController;
        var flow = turn.battleFlow;

        int activeTurn = flow.turn;
        bool active = true;

        //카드 생성 알고리즘 필요


        owner.MarkIdealThisStage();
    }
}