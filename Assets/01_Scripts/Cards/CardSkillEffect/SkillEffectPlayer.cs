using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill 이펙트 재생기 (Coroutine 기반, 부드러운 재생 + 자동 삭제)
/// </summary>
public class SkillEffectPlayer : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private List<Sprite> frames;    // 재생할 스프라이트 리스트

    /// <summary>
    /// 이펙트를 설정하고 재생 시작
    /// </summary>
    /// <param name="animationFrames">재생할 스프라이트 리스트</param>
    /// <param name="fps">초당 프레임 수</param>
    public void Play(List<Sprite> animationFrames, float fps, bool flipX = false)
    {
        if (animationFrames == null || animationFrames.Count == 0)
        {
            Debug.LogWarning("[SkillEffectPlayer] 재생할 스프라이트가 없습니다.");
            Destroy(gameObject);
            return;
        }

        frames = animationFrames;
        spriteRenderer.flipX = flipX; // ← 방향 반영
        StartCoroutine(PlayCoroutine(fps));
    }

    /// <summary>
    /// 코루틴으로 프레임마다 자연스럽게 스프라이트 교체
    /// </summary>
    private IEnumerator PlayCoroutine(float fps)
    {
        float frameDelay = 1f / fps; // 프레임당 대기시간

        for (int i = 0; i < frames.Count; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameDelay);
        }

        Destroy(gameObject); // 마지막 프레임 이후 삭제
    }
}
