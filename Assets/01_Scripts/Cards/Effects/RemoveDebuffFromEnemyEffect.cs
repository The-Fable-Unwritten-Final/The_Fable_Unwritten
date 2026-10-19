using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/RemoveDebuffFromEnemyEffect")]
public class RemoveDebuffFromEnemyEffect : CardEffectBase
{
    public int value = 1;

    public int LastRemovedSum { get; private set; }

    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        LastRemovedSum = 0;

        foreach (var t in targets)
        {
            if (t is Enemy enemy)
            {
                for (int i = enemy.instantEffects.Count - 1; i >= 0; i--)
                {
                    var eff = enemy.instantEffects[i];

                    if (Debuff.IsDebuff(eff.statType, eff.value))
                    {
                        LastRemovedSum += Mathf.Abs(Mathf.RoundToInt(eff.value));
                        enemy.instantEffects.RemoveAt(i);
                    }
                }

                for (int i = enemy.tickEffects.Count - 1; i >= 0; i--)
                {
                    var eff = enemy.tickEffects[i];

                    if (Debuff.IsDebuff(eff.statType, eff.value))
                    {
                        LastRemovedSum += Mathf.Abs(Mathf.RoundToInt(eff.value));
                        enemy.tickEffects.RemoveAt(i);
                    }
                }
            }
        }
    }

    public override string GetDescription()
    {
        return "적의 해로운 효과를 제거합니다.";
    }
}