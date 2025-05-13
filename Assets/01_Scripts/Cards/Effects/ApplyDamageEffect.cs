using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/ApplyDamageEffect")]

public class ApplyDamageEffect : CardEffectBase
{
    public int value;
    public int index;

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        // 현재 턴 중 이 카드가 몇 번 사용되었는지 확인
        int useCount = 0;
        foreach (var log in BattleLogManager.Instance.UsedCardsForCurrent)
        {
            if (log.cardID == index)
                useCount++;
        }

        float totalDamage = value * (useCount-1);

        foreach (var target in targets)
        {
            if (!target.IsAlive()) continue;

            target.TakeDamage(totalDamage);

            Debug.Log($"[ApplyDamageEffect] {index}번 카드를 {useCount}회 사용하여 {totalDamage} 추가 피해");
        }
    }
    public override string GetDescription() => $"이 카드 사용 횟수 × {value}만큼 피해를 줍니다.";
}
