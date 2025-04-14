using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
{
    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;  // 리소스에서 가져오고 있음(어드레스블 변경 할수도)
    [Range(0, 1f)] public float bgmVolume = 1f;

    [Header("SFX Settings")]
    [SerializeField] private SoundSource soundSourcePrefab; // 리소스에서 가져오고 있음(어드레스블 변경 할수도)
    [Range(0, 1f)] public float sfxVolume = 1f;
    [SerializeField] private float sfxPitchVariance = 0.1f;

    [Header("Mute")]
    private bool isMuted = false;

    private Coroutine fadeCoroutine;
    private Queue<SoundSource> soundSourcePool = new();

    private readonly Dictionary<string, AudioClip> loadedClips = new();

    protected override void Awake()
    {
        base.Awake();
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
    }

    // ===== BGM =====

    /// <summary>
    /// BGM 변경 매서드
    /// </summary>
    public void PlayBGM(string clipName)
    {
        if (isMuted) return;

        var clip = LoadClip($"Sounds/BGM/{clipName}");
        if (clip == null || bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    /// <summary>
    /// BGM 전환 시 페이드 효과 매서드
    /// </summary>
    public void ChangeBGMWithFade(string clipName, float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(ChangeBGMCoroutine(clipName, duration));
    }

    private IEnumerator ChangeBGMCoroutine(string clipName, float duration)
    {
        yield return FadeOut(duration * 0.5f);
        yield return FadeIn(clipName, duration * 0.5f);
    }

    private IEnumerator FadeOut(float duration)
    {
        float start = bgmSource.volume;
        while (bgmSource.volume > 0)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, 0f, (start / duration) * Time.deltaTime);
            yield return null;
        }
        bgmSource.Stop();
    }

    private IEnumerator FadeIn(string clipName, float duration)
    {
        var clip = LoadClip($"Sounds/BGM/{clipName}");
        if (clip == null) yield break;

        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        while (bgmSource.volume < bgmVolume)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, bgmVolume, (bgmVolume / duration) * Time.deltaTime);
            yield return null;
        }
    }

    // ===== SFX =====

    /// <summary>
    /// 효과음 사용 매서드
    /// </summary>
    public static void PlaySFX(string clipName)
    {
        if (Instance.isMuted) return;

        var clip = Instance.LoadClip($"Sounds/SFX/{clipName}");
        if (clip == null) return;

        var source = Instance.GetSoundSource();
        source.Play(clip, Instance.sfxVolume, Instance.sfxPitchVariance);
    }

    private SoundSource GetSoundSource()
    {
        if (soundSourcePool.Count > 0)
        {
            var src = soundSourcePool.Dequeue();
            src.gameObject.SetActive(true);
            return src;
        }
        return Instantiate(soundSourcePrefab, transform);
    }

    public void ReturnSoundSource(SoundSource source)
    {
        source.gameObject.SetActive(false);
        soundSourcePool.Enqueue(source);
    }

    // 사운드 리소스 캐싱 하기 위한 매서드
    private AudioClip LoadClip(string path)
    {
        if (loadedClips.TryGetValue(path, out var cachedClip))
            return cachedClip;

        var clip = Resources.Load<AudioClip>(path);
       
        if (clip == null) return null;

        loadedClips[path] = clip;

        return clip;
    }

    /// <summary>
    /// 음소거 매서드
    /// </summary>
    public void SetMute(bool mute)
    {
        isMuted = mute;
        ApplyVolume();
    }

    /// <summary>
    /// BGM 효과음 조절
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    /// <summary>
    /// SFX 효과음 조절
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    // 소리 크기 셋팅 
    private void ApplyVolume()
    {
        bgmSource.volume = isMuted ? 0f : bgmVolume;
    }
}
