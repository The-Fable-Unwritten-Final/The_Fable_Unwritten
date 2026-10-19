using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnUseSkillBlack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image blackOverlay; // 반투명 검정 Image
    [SerializeField] private float fadeDuration = 0.2f; // 페이드 인/아웃 시간
    [SerializeField] private float targetAlpha = 0.7f; // 반투명 정도 (0~1)

    private BattleFlowController battleFlow;
    private Dictionary<IStatusReceiver, Color> originalSpriteColors = new();

    private void Awake()
    {
        // Image가 없으면 가져오기
        if (blackOverlay == null)
        {
            blackOverlay = GetComponent<Image>();
            if (blackOverlay == null)
            {
                Debug.LogError("[OnUseSkillBlack] Image 컴포넌트를 찾을 수 없습니다.");
                return;
            }
        }

        // 초기 상태: 반투명 오버레이 숨김
        Color color = blackOverlay.color;
        color.a = 0f;
        blackOverlay.color = color;
    }

    private void Start()
    {
        battleFlow = GameManager.Instance?.turnController?.battleFlow;
        if (battleFlow == null)
        {
            Debug.LogError("[OnUseSkillBlack] BattleFlowController를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 스킬 사용 시 호출 - 해당 캐릭터와 타겟만 보이고 나머지는 반투명 처리
    /// </summary>
    public void ShowSkillFocus(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        if (battleFlow == null || blackOverlay == null) return;

        StopAllCoroutines();

        // 타겟 목록에 시전자 추가 (시전자도 포커스 대상)
        var focusTargets = new List<IStatusReceiver> { caster };
        focusTargets.AddRange(targets);

        // 모든 캐릭터 및 몬스터 반투명 처리
        DimAllCharactersExcept(focusTargets);

        // 반투명 오버레이 표시
        StartCoroutine(FadeImage(blackOverlay, 0f, targetAlpha, fadeDuration));

        // 포커스 대상의 HP바만 활성화
        ShowHPBarsForTargets(focusTargets);
    }

    /// <summary>
    /// 스킬 종료 후 호출 - 모든 처리 복구
    /// </summary>
    public void HideSkillFocus()
    {
        StopAllCoroutines();

        // 모든 캐릭터 원래 색으로 복구
        RestoreAllCharacterColors();

        // 모든 HP바 복구
        RestoreAllHPBars();

        // 반투명 오버레이 숨김
        if (blackOverlay != null)
        {
            StartCoroutine(FadeImage(blackOverlay, targetAlpha, 0f, fadeDuration));
        }
    }

    /// <summary>
    /// 포커스 대상을 제외한 모든 캐릭터를 반투명 처리
    /// </summary>
    private void DimAllCharactersExcept(List<IStatusReceiver> focusTargets)
    {
        originalSpriteColors.Clear();

        // 플레이어 파티 반투명 처리
        foreach (var player in battleFlow.playerParty)
        {
            if (player != null && !focusTargets.Contains(player))
            {
                if (player is PlayerController pc && pc.spriteRenderer != null)
                {
                    // 원래 색 저장
                    originalSpriteColors[player] = pc.spriteRenderer.color;

                    // 반투명 처리
                    Color dimmedColor = pc.spriteRenderer.color;
                    dimmedColor.a *= 0.3f; // 30% 투명도
                    pc.spriteRenderer.color = dimmedColor;
                }
            }
        }

        // 적 파티 반투명 처리
        foreach (var enemy in battleFlow.enemyParty)
        {
            if (enemy != null && !focusTargets.Contains(enemy))
            {
                if (enemy is Enemy e && e.spriteRenderer != null)
                {
                    // 원래 색 저장
                    originalSpriteColors[enemy] = e.spriteRenderer.color;

                    // 반투명 처리
                    Color dimmedColor = e.spriteRenderer.color;
                    dimmedColor.a *= 0.3f; // 30% 투명도
                    e.spriteRenderer.color = dimmedColor;
                }
            }
        }
    }

    /// <summary>
    /// 모든 캐릭터 색을 원래대로 복구
    /// </summary>
    private void RestoreAllCharacterColors()
    {
        foreach (var kvp in originalSpriteColors)
        {
            IStatusReceiver character = kvp.Key;
            Color originalColor = kvp.Value;

            if (character is PlayerController pc && pc.spriteRenderer != null)
            {
                pc.spriteRenderer.color = originalColor;
            }
            else if (character is Enemy e && e.spriteRenderer != null)
            {
                e.spriteRenderer.color = originalColor;
            }
        }

        originalSpriteColors.Clear();
    }

    /// <summary>
    /// 포커스 대상의 HP바만 표시 (다른 HP바는 숨김)
    /// </summary>
    private void ShowHPBarsForTargets(List<IStatusReceiver> focusTargets)
    {
        // 모든 HP바를 찾아서 처리
        HpBarDisplay[] allHPBars = FindObjectsOfType<HpBarDisplay>();

        foreach (var hpBar in allHPBars)
        {
            // 해당 HP바가 포커스 대상인지 확인
            bool isForFocusTarget = false;

            foreach (var target in focusTargets)
            {
                if (target is PlayerController pc)
                {
                    if (IsHPBarConnectedTo(hpBar, pc.gameObject))
                    {
                        isForFocusTarget = true;
                        break;
                    }
                }
                else if (target is Enemy e)
                {
                    if (IsHPBarConnectedTo(hpBar, e.gameObject))
                    {
                        isForFocusTarget = true;
                        break;
                    }
                }
            }

            // 포커스 대상이 아니면 HP바와 텍스트 숨김
            hpBar.SetHPBarVisible(isForFocusTarget);
        }
    }

    /// <summary>
    /// 모든 HP바 복구
    /// </summary>
    private void RestoreAllHPBars()
    {
        HpBarDisplay[] allHPBars = FindObjectsOfType<HpBarDisplay>();

        foreach (var hpBar in allHPBars)
        {
            hpBar.SetHPBarVisible(true);
        }
    }

    /// <summary>
    /// HP바가 특정 캐릭터에 연결되어 있는지 확인
    /// </summary>
    private bool IsHPBarConnectedTo(HpBarDisplay hpBar, GameObject character)
    {
        // 방법 1: Transform 구조를 통한 확인 (HP바가 캐릭터의 자식이거나 특정 구조)
        Transform hpBarTransform = hpBar.transform;
        Transform current = hpBarTransform.parent;

        // 캔버스까지 올라가며 확인
        while (current != null)
        {
            if (current.gameObject == character)
                return true;
            current = current.parent;
        }

        // 방법 2: 같은 Canvas 내에서 position 근처 확인 (월드/캔버스 좌표로 매칭)
        // 이는 HpBarDisplay.FollowTarget() 메서드가 사용되는 경우
        // 대체 방법으로 사용 가능

        return false;
    }

    /// <summary>
    /// Image의 알파값을 부드럽게 변환
    /// </summary>
    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        if (image == null) yield break;

        float elapsed = 0f;
        Color color = image.color;
        color.a = startAlpha;
        image.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color = image.color;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            image.color = color;
            yield return null;
        }

        color = image.color;
        color.a = endAlpha;
        image.color = color;
    }
}
