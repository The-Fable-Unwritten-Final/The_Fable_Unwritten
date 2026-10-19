using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// 대화 텍스트 하단의 "Space" 텍스트에 펄싱 효과를 적용
/// 1.0 ~ 1.05 크기로 반복하면서 커졌다 작아졌다 반복
/// </summary>
public class TextZoomingFX : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TargetText;           
    [SerializeField] private float minScale = 1f;      // 최소 크기
    [SerializeField] private float maxScale = 1.05f;   // 최대 크기
    [SerializeField] private float cycleDuration = 0.8f; // 한 사이클 시간
    
    private Sequence pulseSequence;

    private void Start()
    {
        StartPulsingAnimation();
    }

    private void StartPulsingAnimation()
    {
        if (TargetText == null) return;
        
        pulseSequence?.Kill();

        RectTransform rectTransform = TargetText.rectTransform;
        rectTransform.localScale = Vector3.one * minScale;

        // 펄싱 애니메이션 시퀀스 생성
        pulseSequence = DOTween.Sequence();
        
        // 1 → 1.05 (올라감)
        pulseSequence.Append(rectTransform.DOScale(maxScale, cycleDuration / 2f).SetEase(Ease.InOutQuad));
        
        // 1.05 → 1 (내려감)
        pulseSequence.Append(rectTransform.DOScale(minScale, cycleDuration / 2f).SetEase(Ease.InOutQuad));
        
        // 무한 반복
        pulseSequence.SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 시퀀스 정리
        pulseSequence?.Kill();
    }
}
