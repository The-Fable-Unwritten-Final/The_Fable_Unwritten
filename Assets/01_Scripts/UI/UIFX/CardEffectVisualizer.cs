using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CardEffectVisualizer : MonoBehaviour
{
    [SerializeField] CardInHand cardInHand;

    [SerializeField] float duration = 0.8f; // 알파값 변화 시간
    [SerializeField] float targetAlpha = 0.6f; // 목표 알파값

    [SerializeField] private Image innerEdge;
    [SerializeField] UIParticle useCardFXParticle; // 사용 효과 파티클
    [SerializeField] UIParticle useEnhancedCardFXParticle; // 강화된 카드 사용 효과 파티클
    public bool enhanceTriggered = false; // 강화 효과가 트리거 되었는지 확인용(강화 상태에서, Use 상태로 전환시 이곳에서 트리거 실행.)

    [SerializeField] GameObject cardReadyFX; // 카드 준비 효과 (노란 테두리)
    [SerializeField] GameObject cardReadyOnChainFX; // 카드 연계 준비 효과 (초록(노+파) 테두리)
    [SerializeField] GameObject cardCanChainFX; // 카드 연계 가능 효과 (얇은 노란 테두리)
    [SerializeField] GameObject cardChainFX; // 카드 연계 효과 (파란 테두리)

    Coroutine innerAlphaCoroutine; // innerEdge 알파값 코루틴.

    public CardVisualState currentState = CardVisualState.None;

    public enum CardVisualState
    {
        None,
        Ready,   // 노란 테두리
        ReadyOnChain, // 연계 상태일때 마우스를 올릴경우,
        CanChain, // 얇은 노란 테두리 >> 연계 가능 상태 (현재 마우스를 올린 카드를 사용시 체인 적용될 카드 표시)
        Chain, // 파란 테두리
        Use     // 사용 효과
    }

    // 지정 선택, 해제시 사용되는 None 메서드
    // 지정시 체인 적용,
    public void SetStateToNone()
    {
        // 카드의 이펙트 시각 효과
        if (currentState == CardVisualState.Chain) return; // 카드의 상태가 Chain인 경우 이펙트 변경 취소 (빨간색 유지)
        if (currentState == CardVisualState.ReadyOnChain)
        {
            ApplyVisualState(CardVisualState.Chain); // 카드의 상태를 Chain상태로 돌리기.
            return;
        }

        this.ApplyVisualState(CardVisualState.None);
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
                cardReadyFX.SetActive(true); // 노란색 테두리
                break;

            case CardVisualState.ReadyOnChain:
                cardReadyOnChainFX.SetActive(true); // 초록색(노란+파란) 테두리
                break;

            case CardVisualState.CanChain:
                cardCanChainFX.SetActive(true); // 얇은 노란색 테두리
                break;

            case CardVisualState.Chain:
                SetEdgeGlow("#FF2523"); // 빨간색 테두리
                cardChainFX.SetActive(true); // 파란색 테두리
                break;

            case CardVisualState.Use:
                UseFXPlay();
                cardInHand.cardDisplay.CheckChainCard(); // 카드 상태 변경시, 연계 카드 표시 갱신
                break;

            case CardVisualState.None:
                break;
            default:
                break;
        }

        currentState = newState;
    }
    void UseFXPlay()
    {
        if (enhanceTriggered)
            useEnhancedCardFXParticle.Play();
        else
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
        cardReadyFX.SetActive(false);
        cardReadyOnChainFX.SetActive(false);
        cardCanChainFX.SetActive(false);
        cardChainFX.SetActive(false);

        if (innerAlphaCoroutine != null)
        {
            StopCoroutine(innerAlphaCoroutine);
            innerAlphaCoroutine = null;
        }
    }
}
