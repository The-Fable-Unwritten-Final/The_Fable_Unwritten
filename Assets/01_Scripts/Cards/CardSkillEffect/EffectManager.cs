using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EffectManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private SkillEffectPlayer effectPrefab;
    [SerializeField] private Transform effectRoot;


    /// <summary>
    /// 특정 이펙트를 위치에 재생
    /// </summary>
    public void PlayEffect(string effectName, Transform caster, Transform target, bool flipX = false, float scaleFactor = 1f)
    {
        // 1. 애니메이션 정보 로드
        if (!DataManager.Instance.CardEffects.TryGetValue(effectName, out var animInfo) || animInfo == null || animInfo.frames == null)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 {effectName}를 찾지 못했거나 스프라이트 없음.");
            return;
        }

        switch(animInfo.animationType)
        {
            case AnimationType.Projectile:
                StartCoroutine(PlayProjectileCoroutine(caster, target, animInfo,scaleFactor, () => { }));
                break;
            case AnimationType.OnBottomTarget:
                Vector3 bottomPos = GetBottomPosition(target);
                PlayOneShotEffect(animInfo, bottomPos, flipX, scaleFactor);
                break;
            case AnimationType.OnTarget:
            default:
                PlayOneShotEffect(animInfo, target.position, flipX, scaleFactor);
                break;
        }
    }

    public void PlayOneShotEffect(EffectAnimation animInfo, Vector3 position, bool flipX, float scaleFactor)
    {
        var effectInstance = Instantiate(effectPrefab, position, Quaternion.identity, effectRoot);
        effectInstance.transform.localScale *= scaleFactor;

        var sr = effectInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Effect";
            sr.sortingOrder = 100;
            sr.flipX = flipX;
        }

        effectInstance.Play(animInfo, 1.2f, flipX);
    }

    public void PlayProjectileEffect(string effectName, Transform caster, Transform target, float scaleFactor, System.Action onArrive)
    {
        if (!DataManager.Instance.CardEffects.TryGetValue(effectName, out var animInfo) || animInfo == null)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 {effectName} 없음");
            return;
        }

        StartCoroutine(PlayProjectileCoroutine(caster, target, animInfo, scaleFactor, onArrive));
    }

    private IEnumerator PlayProjectileCoroutine(Transform caster, Transform target, EffectAnimation animInfo, float scaleFactor, System.Action onArrive)
    {
        var projectile = Instantiate(effectPrefab, caster.position, Quaternion.identity, effectRoot);
        projectile.transform.localScale *= scaleFactor;

        var sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Effect";
            sr.sortingOrder = 100;
        }

        // ▶ 위치 계산
        Vector3 end = target.position;
        Vector3 direction = (end - caster.position).normalized;
        Vector3 start = end - direction * 1.0f; // 타겟 기준 살짝 앞에서 출발

        projectile.transform.position = start;

        float duration = 0.5f;
        float elapsed = 0f;

        projectile.Play(animInfo, duration, flipX: false);

        while (elapsed < duration)
        {
            projectile.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        projectile.transform.position = end;

        // 도착 후 Hit 처리
        onArrive?.Invoke();
    }
    public Vector3 GetBottomPosition(Transform target)
    {
        if (target.TryGetComponent<SpriteRenderer>(out var sr))
        {
            var bounds = sr.bounds;
            return new Vector3(bounds.center.x, bounds.min.y, target.position.z);
        }
        return target.position;
    }
}
