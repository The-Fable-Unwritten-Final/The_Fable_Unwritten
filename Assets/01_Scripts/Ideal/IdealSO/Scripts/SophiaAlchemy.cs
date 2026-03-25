using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IdealData;

[CreateAssetMenu(menuName = "Fable/Ideal/Sophia_Alchemy")]
public class SophiaAlchemy : IdealSkillBase
{
    public override string DisplayName => "연금술사";

    public override void Activate(PlayerController owner)
    {
        var flow = GameManager.Instance.turnController.battleFlow;

        // 소유자 핸드 스냅샷 자동 사용 (타겟 자동)
        var snapshot = new System.Collections.Generic.List<CardModel>(owner.Deck.Hand);
        foreach (var card in snapshot)
        {
            var targets = flow.AutoChooseTargets(card.targetType, card.characterClass, Mathf.Max(1, card.targetCount), null);
            //flow.UseCard(card, owner, targets.Count > 0 ? targets[0] : null);
        }

        // 다음 턴 드로우 2장 고정 (전원)
        //foreach (var p in flow.playerParty)
        //    p.Deck.SetNextDrawOverride(2);
        
        owner.MarkIdealThisStage();
    }
}