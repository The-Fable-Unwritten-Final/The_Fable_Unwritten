using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fable/Ideal/Kalya_Sanctify")]
public class KalyaSanctify : IdealSkillBase
{
    
    public override string DisplayName => "성화";

    public override void Activate(PlayerController owner)
    {
        var flow = GameManager.Instance.turnController.battleFlow;
        foreach (var ally in flow.playerParty)
        {
            if (ally is not PlayerController pc || !pc.IsAlive()) continue;

            float max = pc.maxHP;
            float cur = pc.currentHP;

            // 최대체력이면 즉사 아님 → HP = 1
            if (Mathf.Approximately(cur, max))
            {
                pc.currentHP = 1f;
                pc.UpdateHpStatus();

                // 회복/피해 텍스트 등 표시를 원하면 여기에 큐잉
                continue;
            }

            // 반전
            float inverted = max - cur;
            bool decreased = inverted < cur;
            float loss = decreased ? (cur - inverted) : 0f;

            pc.currentHP = inverted;
            pc.UpdateHpStatus();

            // 감소했다면 지속 회복(10턴) 부여: loss * 0.1 (내림/반올림은 팀 규칙대로)
            if (loss > 0f)
            {
                int perTurn = Mathf.Max(1, Mathf.FloorToInt(loss * 0.1f)); // 최소 1 보장
                pc.ApplyStatusEffect(new TickEffect
                {
                    statType = BuffStatType.SustainRegen, // 새 상태 (아래 2) 참고)
                    value = perTurn,
                    duration = 10
                });
                // UI로 턴당 회복량 표시 필요 시, 상태 UI에서 TickEffect 표시하도록 연동
            }
        }

        owner.MarkIdealThisStage();
    }
}
