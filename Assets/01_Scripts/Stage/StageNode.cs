using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StageNode : MonoBehaviour
{
    public Image iconImage;
    private Tween pulseTween;
    public Image clearCircleImage;

    /// <summary>
    /// 타입 확인 후 노드 이미지 설정
    /// </summary>
    public void Setup(NodeType type, Sprite icon)
    {
        iconImage.sprite = icon;
    }

    /// <summary>
    /// 노드 강조 효과
    /// </summary>
    public void PlayPulse()
    {
        if (pulseTween != null && pulseTween.IsPlaying()) return;

        pulseTween = transform
            .DOScale(1.00f, 0.5f) // 펄스 효과 임시로 미적용
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// 노드 강조 효과 취소
    /// </summary>
    public void StopPulse()
    {
        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
            pulseTween = null;
        }

        transform.localScale = Vector3.one;
    }

    // DoTween 끝난 후 에러 표시 방지를 위해 파괴
    private void OnDestroy()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
    }

    public void HideClearCircle()
    {
        // clearCircleImage의 알파값 0로 설정
        Color color = clearCircleImage.color;
        color.a = 0f;
        clearCircleImage.color = color;
    }
    public void ShowClearCircle()
    {
        // clearCircleImage의 알파값 1로 설정
        Color color = clearCircleImage.color;
        color.a = 1f;
        clearCircleImage.color = color;
    }
}
