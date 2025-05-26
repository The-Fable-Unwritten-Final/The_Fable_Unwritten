using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetArrowDisplay : MonoBehaviour
{
    private IStatusReceiver receiver;

    public void Init(IStatusReceiver receiver)
    {
        this.receiver = receiver;
        receiver.OnTargetableChanged += UpdateArrowVisibility;
        UpdateArrowVisibility(); // 초기 상태 반영
    }

    private void UpdateArrowVisibility()
    {
        receiver.tarArrow.gameObject.SetActive(receiver.IsTargetable);
    }

    private void OnDestroy()
    {
        if (receiver != null)
            receiver.OnTargetableChanged -= UpdateArrowVisibility;
    }
}
