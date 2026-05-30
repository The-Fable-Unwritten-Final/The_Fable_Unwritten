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
       IStatusReceiver caster,
       IStatusReceiver target,
       bool flipX = false,
       float scaleFactor = 1f,
       System.Action onHitFrame = null)
    {
        if (!TryGetEffect(effectName, out EffectAnimation animInfo))
            return;

        switch (animInfo.animationType)
        {
            case AnimationType.Projectile:
                StartCoroutine(PlayProjectileCoroutine(
                    caster,
                    target,
                    animInfo,
                    scaleFactor,
                    onHitFrame));
                break;

            case AnimationType.OnBottomTarget:
            case AnimationType.OnTarget:
            case AnimationType.OnHeadPoint:
            case AnimationType.OnOverheadPoint:
            case AnimationType.Looping:
            case AnimationType.AOE:
            default:
                Vector3 position = GetEffectPosition(target, animInfo.animationType);
                PlayOneShotEffect(
                    animInfo,
                    position,
                    flipX,
                    scaleFactor,
                    onHitFrame);
                break;
        }
    }

    private void PlayOneShotEffect(
           EffectAnimation animInfo,
           Vector3 position,
           bool flipX,
           float scaleFactor,
           System.Action onHitFrame = null)
    {
        SkillEffectPlayer effectInstance =
            Instantiate(effectPrefab, position, Quaternion.identity, effectRoot);

        effectInstance.transform.localScale *= scaleFactor;

        SpriteRenderer sr = effectInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Effect";
            sr.sortingOrder = 4;
            sr.flipX = flipX;
        }

        effectInstance.Play(animInfo, 1.2f, flipX, onHitFrame);
    }

    public void PlayProjectileEffect(
        string effectName,
        IStatusReceiver caster,
        IStatusReceiver target,
        float scaleFactor,
        System.Action onHitFrame = null)
    {
        if (!TryGetEffect(effectName, out EffectAnimation animInfo))
            return;

        StartCoroutine(PlayProjectileCoroutine(
                    caster,
                    target,
                    animInfo,
                    scaleFactor,
                    onHitFrame));
    }

    private IEnumerator PlayProjectileCoroutine(
        IStatusReceiver caster,
        IStatusReceiver target,
        EffectAnimation animInfo,
        float scaleFactor,
        System.Action onHitFrame = null)
    {
        Vector3 casterBody = GetEffectPosition(caster, AnimationType.OnTarget);
        Vector3 targetBody = GetEffectPosition(target, AnimationType.OnTarget);

        Vector3 direction = (targetBody - casterBody).normalized;

        Vector3 start = casterBody;
        Vector3 end = targetBody;

        SkillEffectPlayer projectile =
            Instantiate(effectPrefab, start, Quaternion.identity, effectRoot);

        projectile.transform.localScale *= scaleFactor;

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Effect";
            sr.sortingOrder = 100;
        }

        bool flipX = end.x < start.x;

        float duration = 0.5f;
        float elapsed = 0f;

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

    private bool TryGetEffect(string effectName, out EffectAnimation animInfo)
    {
        animInfo = null;

        if (!DataManager.Instance.CardEffects.TryGetValue(effectName, out animInfo) || animInfo == null)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 {effectName}를 찾지 못했습니다.");
            return false;
        }

        bool hasSpriteFrames = animInfo.frames != null && animInfo.frames.Count > 0;
        bool hasAnimationClip = animInfo.animationClip != null;

        if (!hasSpriteFrames && !hasAnimationClip)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 {effectName}에 frames/animationClip이 없습니다.");
            return false;
        }

        return true;
    }

    private Vector3 GetEffectPosition(IStatusReceiver target, AnimationType type)
    {
        if (target == null)
            return Vector3.zero;

        return type switch
        {
            AnimationType.OnBottomTarget => target.FootPoint.position,
            AnimationType.OnTarget => target.BodyPoint.position,
            AnimationType.OnHeadPoint => target.HeadPoint.position,
            AnimationType.OnOverheadPoint => target.OverheadPoint.position,
            AnimationType.OnAheadPoint => target.AheadPoint.position,

            AnimationType.Projectile => target.BodyPoint.position,
            AnimationType.Looping => target.BodyPoint.position,
            AnimationType.AOE => target.BodyPoint.position,

            _ => target.CachedTransform.position
        };
    }
}