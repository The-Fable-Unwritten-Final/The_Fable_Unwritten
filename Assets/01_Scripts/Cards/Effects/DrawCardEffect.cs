using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 드로우 관련 효과가 주어질 경우
/// </summary>
[CreateAssetMenu(menuName = "Card/Effect/Draw")]
public class DrawCardEffect : CardEffectBase
{
    public int count;       //드로우 횟수

    /// <summary>
    /// 사용 시 사용자가 카드 드로우
    /// </summary>
    /// <param name="caster">사용자</param>
    /// <param name="target">적일 수도?</param>
    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        caster.Deck?.Draw(count);
    }

    public override string GetDescription() => $"카드를 {count} 뽑습니다.";
}
