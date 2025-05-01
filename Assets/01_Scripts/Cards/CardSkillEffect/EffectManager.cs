using UnityEngine;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private SkillEffectPlayer effectPrefab;
    [SerializeField] private Transform effectRoot;

    [Header("이펙트 로드")]
    public SpriteLoader spriteLoader; // Sprite 리스트를 관리하는 로더 (따로 만들자!)

    /// <summary>
    /// 특정 이펙트를 위치에 재생
    /// </summary>
    public void PlayEffect(string effectName, Vector3 position, bool flipX = false)
    {
        if (spriteLoader == null)
        {
            Debug.LogError("[EffectManager] SpriteLoader가 설정되어 있지 않습니다.");
            return;
        }

        var frames = spriteLoader.GetEffectFrames(effectName);
        if (frames == null || frames.Count == 0)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 {effectName}를 찾지 못했습니다.");
            return;
        }

        var effectInstance = Instantiate(effectPrefab, effectRoot);
        effectInstance.transform.position = position;
        effectInstance.Play(frames, 15f, flipX); // 15fps 고정
    }
}
