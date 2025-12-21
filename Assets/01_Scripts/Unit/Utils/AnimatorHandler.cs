using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 애니메이션 재생을 담당하는 핸들러
/// </summary>
public class AnimationHandler
{
    private readonly Animator animator;
    private readonly SpriteRenderer spriteRenderer;
    private readonly MonoBehaviour coroutineRunner;

    private const float ATTACK_RESET_DELAY = 1.5f;
    private const float HIT_RESET_DELAY = 1.2f;
    private const int ATTACK_IDLE_VALUE = -1;

    public AnimationHandler(Transform visualTransform, MonoBehaviour coroutineRunner)
    {
        this.coroutineRunner = coroutineRunner;

        if (visualTransform != null)
        {
            animator = visualTransform.GetComponent<Animator>();
            spriteRenderer = visualTransform.GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// 애니메이션 컨트롤러 설정
    /// </summary>
    public void SetAnimatorController(RuntimeAnimatorController controller)
    {
        if (animator != null && controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogWarning("[AnimationHandler] Animator 또는 Controller가 null입니다.");
        }
    }

    /// <summary>
    /// 공격 애니메이션 재생
    /// </summary>
    public void PlayAttack(int attackType)
    {
        if (animator == null) return;

        animator.SetInteger("Attack", attackType);
        coroutineRunner.StartCoroutine(ResetIntegerParam("Attack", ATTACK_IDLE_VALUE, ATTACK_RESET_DELAY));
    }

    /// <summary>
    /// 피격 애니메이션 재생
    /// </summary>
    public void PlayHit()
    {
        if (animator == null) return;

        animator.SetBool("Hit", true);
        coroutineRunner.StartCoroutine(ResetBoolParam("Hit", HIT_RESET_DELAY));
    }

    /// <summary>
    /// 특정 트리거 발동
    /// </summary>
    public void SetTrigger(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    /// <summary>
    /// Bool 파라미터 설정
    /// </summary>
    public void SetBool(string paramName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(paramName, value);
        }
    }

    /// <summary>
    /// Integer 파라미터 설정
    /// </summary>
    public void SetInteger(string paramName, int value)
    {
        if (animator != null)
        {
            animator.SetInteger(paramName, value);
        }
    }

    /// <summary>
    /// Float 파라미터 설정
    /// </summary>
    public void SetFloat(string paramName, float value)
    {
        if (animator != null)
        {
            animator.SetFloat(paramName, value);
        }
    }

    /// <summary>
    /// 스프라이트 색상 설정
    /// </summary>
    public void SetSpriteColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// 스프라이트 플립 설정
    /// </summary>
    public void SetFlipX(bool flip)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flip;
        }
    }


    private IEnumerator ResetBoolParam(string param, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (animator != null)
        {
            animator.SetBool(param, false);
        }
    }

    private IEnumerator ResetIntegerParam(string param, int resetValue, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (animator != null)
        {
            animator.SetInteger(param, resetValue);
        }
    }
}