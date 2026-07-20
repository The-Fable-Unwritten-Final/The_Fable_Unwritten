using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkEventEffect : EventEffects
{
    public int inkAmount; // 잉크량 변화

    public override void Apply()
    {
        if (ProgressDataManager.Instance != null)
        {
            int newInkAmount = ProgressDataManager.Instance.inkAmount + inkAmount;
            ProgressDataManager.Instance.inkAmount = Mathf.Clamp(newInkAmount, 0, ProgressDataManager.Instance.maxInkAmount); // 최소 0, 최대 maxInkAmount로 제한
        }
    }

    public override void UnApply()
    {
        // 현재로서는 일회성 효과이므로 UnApply는 호출 x
    }

    public override EventEffects Clone()
    {
        return new InkEventEffect
        {
            index = this.index,
            text = this.text,
            eventType = this.eventType,
            duration = this.duration,
            inkAmount = this.inkAmount
        };
    }
}
