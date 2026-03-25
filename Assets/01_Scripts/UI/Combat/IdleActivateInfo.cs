using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class IdleActivateInfo : MonoBehaviour
{
    Sequence seq;
    [SerializeField] private Vector2 targetPosition = new Vector2(-300, -200);
    [SerializeField] private Vector2 originalPosition = new Vector2(300, -200);
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float stayDuration = 1.0f;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descriptionText;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowIdle();
        }
    }
    
    /// <summary>
    /// 이상 실현 활성화시 관련 정보 호출 + UI 등장
    /// </summary>
    /// <param name="idleKey"></param>
    public void CallIdleInfo(string idleKey)
    {
        string idleName = LocaleDataManager.GetLocalizedStringTable("Idle Table", idleKey + "Name");
        string idleDesc = LocaleDataManager.GetLocalizedStringTable("Idle Table", idleKey + "Effect");

        nameText.text = idleName;
        descriptionText.text = idleDesc;

        ShowIdle();
    }

    // UI의 좌우 등장 모션
    void ShowIdle()
    {
        RectTransform rt = this.GetComponent<RectTransform>();

        if(seq != null)
        {
            seq.Kill();
            seq = null;
        }

        rt.anchoredPosition  = originalPosition;

        seq = DOTween.Sequence();

        seq.Append(
        rt.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.OutCubic)
        );

        seq.AppendInterval(stayDuration);

        seq.Append(
            rt.DOAnchorPos(originalPosition, moveDuration).SetEase(Ease.InCubic)
        );
        
    }
}
