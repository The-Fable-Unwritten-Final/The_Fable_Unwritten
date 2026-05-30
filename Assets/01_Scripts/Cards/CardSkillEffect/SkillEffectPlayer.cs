using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffectPlayer : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController baseController;

    private Action onHitFrame;
    private bool hitTriggered;
    private EffectAnimation currentAnim;
    private List<Sprite> frames;

    public void Play(
        EffectAnimation animInfo,
        float totalDuration,
        bool flipX = false,
        Action onHitFrame = null)
    {
        if (animInfo == null)
        {
            Destroy(gameObject);
            return;
        }

        if (spriteRenderer != null)
            spriteRenderer.flipX = flipX;

        currentAnim = animInfo;
        this.onHitFrame = onHitFrame;
        hitTriggered = false;

        if (animInfo.playMode == EffectPlayMode.AnimationClip && animInfo.animationClip != null)
        {
            PlayAnimatorClip(animInfo, onHitFrame);
            return;
        }

        if (animInfo.frames == null || animInfo.frames.Count == 0)
        {
            Debug.LogWarning("[SkillEffectPlayer] 재생할 스프라이트/클립이 없습니다.");
            Destroy(gameObject);
            return;
        }

        frames = animInfo.frames;
        StartCoroutine(PlaySpriteCoroutine(totalDuration));
    }

    private void PlayAnimatorClip(EffectAnimation animInfo, Action onHitFrame)
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("[SkillEffectPlayer] Animator가 없습니다.");
            Destroy(gameObject);
            return;
        }

        if (baseController == null)
        {
            Debug.LogError("[SkillEffectPlayer] Base Controller가 없습니다.");
            Destroy(gameObject);
            return;
        }
        Debug.Log($"[Effect] BaseController: {baseController}");
        Debug.Log($"[Effect] Override Clip: {animInfo.animationClip}");

        AnimatorOverrideController overrideController =
            new AnimatorOverrideController(baseController);

        overrideController["BaseEffect"] = animInfo.animationClip;

        animator.runtimeAnimatorController = overrideController;

        Debug.Log($"[Effect] Runtime Controller: {animator.runtimeAnimatorController}");

        animator.Play("Play", 0, 0f);

        if (animInfo.clipHitTime >= 0f && onHitFrame != null)
            StartCoroutine(CallHitAfter(animInfo.clipHitTime, onHitFrame));

        Destroy(gameObject, animInfo.animationClip.length);
    }

    private IEnumerator PlaySpriteCoroutine(float totalDuration)
    {
        int frameCount = frames.Count;
        float frameDelay = totalDuration / frameCount;

        for (int i = 0; i < frameCount; i++)
        {
            spriteRenderer.sprite = frames[i];

            if (!hitTriggered && currentAnim.hitFrame >= 0 && i >= currentAnim.hitFrame)
            {
                hitTriggered = true;
                onHitFrame?.Invoke();
            }

            yield return new WaitForSeconds(frameDelay);
        }

        Destroy(gameObject);
    }

    private IEnumerator CallHitAfter(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}