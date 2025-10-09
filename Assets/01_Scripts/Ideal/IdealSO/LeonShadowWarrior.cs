using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fable/Ideal/Leon_ShadowWarrior")]
public class LeonShadowWarrior : IdealSkillBase
{
    
    public override string DisplayName => "그림자전사";
    public override IdealAvailability GetUseInfo(PlayerController owner)
    {
        if (isUnlocked(Id)) return IdealAvailability.Locked;
        if (owner.IsIdealUsed) return IdealAvailability.Used;
        if (!owner.IsAlive() || owner.IsStunned()) return IdealAvailability.Unavailable;
        return IdealAvailability.Available;
    }

    public override void Activate(PlayerController owner)
    {
        var flow = GameManager.Instance.turnController.battleFlow;
        float overflow = 0f;

/*        // 아군: 버프만 x2
        foreach (var ally in flow.playerParty)
        {
            if (ally is not PlayerController pc || !pc.IsAlive()) continue;
            foreach (var eff in pc.instantEffects)
            {
                if (Debuff.IsDebuff(eff.statType, eff.value)) continue;
                float doubled = eff.value * 2f;
                if (doubled > 50) overflow += (doubled - 50);
                eff.value = Mathf.Min(50, doubled);
            }
        }*/

        // 적: 디버프만 x3
        foreach (var e in flow.enemyParty)
        {
            if (e is not PlayerController epc || !e.IsAlive()) continue; // 적 클래스 타입에 맞게 조정
            foreach (var eff in epc.instantEffects)
            {
                if (!Debuff.IsDebuff(eff.statType, eff.value)) continue;
                float doubled = eff.value * 3f;
                if (doubled > 50) overflow += (doubled - 50);
                eff.value = Mathf.Min(50, doubled);
            }
        }

        if (overflow > 0f) owner.TakeTrueDamage(overflow);
        owner.MarkIdealThisStage();
    }
}
