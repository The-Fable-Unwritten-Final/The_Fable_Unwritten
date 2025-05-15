using UnityEngine;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private SkillEffectPlayer effectPrefab;
    [SerializeField] private Transform effectRoot;


    /// <summary>
    /// 특정 이펙트를 위치에 재생
    /// </summary>
    public void PlayEffect(string effectName, Vector3 position, bool flipX = false)
    {
        // DataManager에서 가져오기
        if (!DataManager.Instance.CardEffects.TryGetValue(effectName, out var frames) || frames == null)
        {
            Debug.LogWarning($"[EffectManager] 이펙트 {effectName}를 찾지 못했거나 스프라이트 없음.");
            return;
        }

        var effectInstance = Instantiate(effectPrefab, effectRoot);
        effectInstance.transform.position = position;
        effectInstance.Play(frames, 15f, flipX);
    }
}
