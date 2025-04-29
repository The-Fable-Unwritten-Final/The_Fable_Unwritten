using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Skill 이펙트 재생기 (프레임 단위 애니메이션 출력 후 자동 삭제)
/// </summary>
public class SkillEffectPlayer : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private SpriteRenderer spriteRenderer; // 이펙트를 출력할 스프라이트 렌더러

    private List<Sprite> frames;    // 재생할 스프라이트 프레임 리스트
    private float frameTime;        // 한 프레임당 표시 시간 (초)
    private float timer;            // 현재 프레임까지 누적된 시간
    private int currentFrame;       // 현재 재생 중인 프레임 인덱스

    /// <summary>
    /// 이펙트를 설정하고 재생 시작
    /// </summary>
    /// <param name="animationFrames">재생할 스프라이트 리스트</param>
    /// <param name="fps">초당 프레임 수</param>
    public void Play(List<Sprite> animationFrames, float fps)
    {
        frames = animationFrames;
        frameTime = 1f / fps;        // ex) 15fps -> frameTime = 1/15초
        timer = 0f;
        currentFrame = 0;

        if (frames != null && frames.Count > 0)
            spriteRenderer.sprite = frames[0]; // 첫 번째 프레임 표시
    }

    /// <summary>
    /// 프레임 단위로 스프라이트를 교체하면서 재생
    /// </summary>
    private void Update()
    {
        if (frames == null || frames.Count == 0) return; // 재생할 게 없으면 무시

        timer += Time.deltaTime;
        if (timer >= frameTime)
        {
            timer -= frameTime;   // 다음 프레임으로 넘어감
            currentFrame++;

            if (currentFrame >= frames.Count)
            {
                Destroy(gameObject); // 마지막 프레임까지 재생하면 오브젝트 삭제
                return;
            }

            spriteRenderer.sprite = frames[currentFrame]; // 다음 프레임 적용
        }
    }
}