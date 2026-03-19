using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EffectManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private SkillEffectPlayer effectPrefab;
    [SerializeField] private Transform effectRoot;

    public void PlayEffect(
        string effectName,
        Transform caster,
        Transform target,
        bool flipX = false,
        float scaleFactor = 1f,
        System.Action onHitFrame = null)
    {
        if (!DataManager.Instance.CardEffects.TryGetValue(effectName, out var animInfo) || animInfo == null || animInfo.frames == null)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 {effectName}를 찾지 못했거나 스프라이트 없음.");
            return;
        }

        switch (animInfo.animationType)
        {
            case AnimationType.Projectile:
                StartCoroutine(PlayProjectileCoroutine(caster, target, animInfo, scaleFactor, onHitFrame));
                break;

            case AnimationType.OnBottomTarget:
                Vector3 bottomPos = GetBottomPosition(target);
                PlayOneShotEffect(animInfo, bottomPos, flipX, scaleFactor, onHitFrame, alignToBottom: true);
                break;

            case AnimationType.OnTarget:
            default:
                PlayOneShotEffect(animInfo, target.position, flipX, scaleFactor, onHitFrame);
                break;
        }
    }

    public void PlayOneShotEffect(
        EffectAnimation animInfo,
        Vector3 position,
        bool flipX,
        float scaleFactor,
        System.Action onHitFrame = null,
        bool alignToBottom = false)
    {
        var effectInstance = Instantiate(effectPrefab, position, Quaternion.identity, effectRoot);
        effectInstance.transform.localScale *= scaleFactor;

        if (alignToBottom)
        {
            float baseHeight = animInfo.frames[0].bounds.size.y;
            float spriteHeight = baseHeight * effectInstance.transform.localScale.y * scaleFactor / 2f;
            effectInstance.transform.position += new Vector3(0, spriteHeight, 0);
        }

        var sr = effectInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Effect";
            sr.sortingOrder = 100;
            sr.flipX = flipX;
        }

        effectInstance.Play(animInfo, 1.2f, flipX, onHitFrame);
    }

    public void PlayProjectileEffect(
        string effectName,
        Transform caster,
        Transform target,
        float scaleFactor,
        System.Action onHitFrame = null)
    {
        if (!DataManager.Instance.CardEffects.TryGetValue(effectName, out var animInfo) || animInfo == null)
        {
            return;
        }

        StartCoroutine(PlayProjectileCoroutine(caster, target, animInfo, scaleFactor, onHitFrame));
    }

    private IEnumerator PlayProjectileCoroutine(
        Transform caster,
        Transform target,
        EffectAnimation animInfo,
        float scaleFactor,
        System.Action onHitFrame = null)
    {
        var projectile = Instantiate(effectPrefab, caster.position, Quaternion.identity, effectRoot);
        projectile.transform.localScale *= scaleFactor;

        var sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Effect";
            sr.sortingOrder = 100;
        }

        Vector3 end = target.position;
        Vector3 direction = (end - caster.position).normalized;
        Vector3 start = end - direction * 1.0f;

        projectile.transform.position = start;

        float duration = 0.5f;
        float elapsed = 0f;

        bool flipX = target.position.x < caster.position.x;

        projectile.Play(animInfo, duration, flipX);

        while (elapsed < duration)
        {
            projectile.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        projectile.transform.position = end;

        onHitFrame?.Invoke();
    }

    public Vector3 GetBottomPosition(Transform target)
    {
        Transform ground = target.Find("GroundPoint");
        if (ground != null)
            return ground.position;

        if (target.TryGetComponent<SpriteRenderer>(out var sr))
        {
            return new Vector3(sr.bounds.center.x, sr.bounds.min.y, target.position.z);
        }

        return target.position;
    }
}