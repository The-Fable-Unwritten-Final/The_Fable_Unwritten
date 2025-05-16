using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 드로우 관련 효과가 주어질 경우
/// </summary>
[CreateAssetMenu(menuName = "Cards/Effects/DrawCardEffect")]
public class DrawCardEffect : CardEffectBase
{
    public int amount;       //드로우 횟수
    public int target;

    /// <summary>
    /// 사용 시 해당자 카드 드로우
    /// </summary>
    /// <param name="caster">사용자</param>
    /// <param name="target">적일 수도?</param>
    public override void Apply(IStatusReceiver caster, List<IStatusReceiver> targets, bool? isEnhanced = null)
    {
        if(target == 0)
        {
            caster.Deck.Draw(amount);
        }
        else if(target == 1)
        {
            foreach (var target in targets)
            {
                target.Deck?.Draw(amount);
            }
        }
    }

    public override string GetDescription() => $"카드를 {amount} 뽑습니다.";
}
