using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "CardEffect/ConditionalEffect")]
public class ConditionalEffect : CardEffectBase
{
    public CardEffectBase conditionalEffect;
    public string conditionTag;

    public override void Apply(IStatusReceiver caster, IStatusReceiver target)
    {
        if (EvaluateCondition(conditionTag, caster, target))
        {
            conditionalEffect.Apply(caster, target);
        }
    }

    /// <summary>
    /// 주어진 조건 문자열을 평가하는 함수입니다. (예: 첫 카드, 체력 50% 이하 등)
    /// </summary>
    private bool EvaluateCondition(string tag, IStatusReceiver caster, IStatusReceiver target)
    {
        return true; // 추후 GameContext 등에 연결해 세부 로직 구현
    }

    public override string GetDescription() => $"조건({conditionTag}) 만족 시: {conditionalEffect.GetDescription()}";

}
