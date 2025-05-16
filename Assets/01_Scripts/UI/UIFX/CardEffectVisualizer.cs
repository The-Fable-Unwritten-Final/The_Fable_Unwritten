using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CardInHand;

public class CardEffectVisualizer : MonoBehaviour
{
    [SerializeField] float duration = 0.8f; // 알파값 변화 시간
    [SerializeField] float targetAlpha = 0.6f; // 목표 알파값

    [SerializeField] private Image innerEdge;
    [SerializeField] UIParticle useCardFXParticle; // 사용 효과 파티클

    Coroutine innerAlphaCoroutine; // innerEdge 알파값 코루틴.

    public CardVisualState currentState = CardVisualState.None;

    public enum CardVisualState
    {
        None,
        Ready,   // 노란 테두리
        Chain, // 빨간 테두리
        Use     // 사용 효과
    }

    public void ApplyVisualState(CardVisualState newState)
    {
        if (newState == currentState)
            return;

        ClearAllEffects();

        switch (newState)
        {
            case CardVisualState.Ready:
                SetEdgeGlow("#FFED34"); // 노란색 테두리
                break;

            case CardVisualState.Chain:
                SetEdgeGlow("#FF2523"); // 빨간색 테두리
                break;

            case CardVisualState.Use:
                UseFXPlay();
                break;

            case CardVisualState.None:
            default:
                break;
        }

        currentState = newState;
    }
    void UseFXPlay()
    {
        useCardFXParticle.Play();
    }
    void SetEdgeGlow(string hex)
    {
        innerEdge.color = ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.white;
        innerEdge.gameObject.SetActive(true);
        innerAlphaCoroutine = StartCoroutine(PulseAlpha());
    }
    void OffEdgeGlow()
    {
        innerEdge.gameObject.SetActive(false);
    }
    private IEnumerator PulseAlpha()
    {
        float halfDuration = duration / 2f;

        while (true)
        {
            // 0 >> targetAlpha
            yield return StartCoroutine(FadeAlpha(0f, targetAlpha, halfDuration));

            // targetAlpha >> 0
            yield return StartCoroutine(FadeAlpha(targetAlpha, 0f, halfDuration));
        }
    }
    IEnumerator FadeAlpha(float from, float to, float time)
    {
        float elapsed = 0f;
        Color color = innerEdge.color;

        while (elapsed < time)
        {
            float t = elapsed / time;
            float alpha = Mathf.Lerp(from, to, t);
            innerEdge.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 보정
        innerEdge.color = new Color(color.r, color.g, color.b, to);
    }
    private void ClearAllEffects()
    {
        // 모든 효과 정지
        OffEdgeGlow();

        if(innerAlphaCoroutine != null)
        {
            StopCoroutine(innerAlphaCoroutine);
            innerAlphaCoroutine = null;
        }
    }
}
