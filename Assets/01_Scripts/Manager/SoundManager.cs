using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoSingleton<SoundManager>
{
    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;
    [Range(0, 1f)] public float bgmVolume = 1f;

    [Header("SFX Settings")]
    [SerializeField] private SoundSource soundSourcePrefab;
    [Range(0, 1f)] public float sfxVolume = 1f;
    [SerializeField] private float sfxPitchVariance = 0.1f;

    [Header("Mute")]
    private bool isMuted = false;

    private Coroutine fadeCoroutine;
    private Queue<SoundSource> soundSourcePool = new();

    private readonly Dictionary<string, AudioClip> bgmClips = new();
    private readonly Dictionary<string, AudioClip> sfxClips = new();
    private Dictionary<string, string> sceneToBGM = new()
    {
        { SceneNameData.TitleScene, "Title" },
        { SceneNameData.StageScene, "Stage" },
        { SceneNameData.CombatScene_Test, "CombatScene_Test"},
        { SceneNameData.CampScene, "Camp" },
        { SceneNameData.RandomEventScene, "Event" },
    };


    protected override void Awake()
    {
        base.Awake();
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;

        LoadAllAudioClips();

        SceneManager.sceneLoaded += OnSceneLoaded;

        // 우선 볼륨 0으로 해뒀어요
        SetBGMVolume(0);
        SetSFXVolume(0);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneToBGM.TryGetValue(scene.name, out var bgmName))
        {
            PlayBGM(bgmName);
        }

        AttachAllButtonClickSounds();
    }

    // ===== BGM =====

    /// <summary>
    /// BGM 변경 매서드
    /// </summary>
    public void PlayBGM(string clipName)
    {
        if (isMuted) return;
        if (!bgmClips.TryGetValue(clipName, out var clip)) return;
        if (bgmSource.clip == clip) return;

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
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
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
        if (!bgmClips.TryGetValue(clipName, out var clip)) yield break;

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
        if (!Instance.sfxClips.TryGetValue(clipName, out var clip)) return;

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

        var newSource = Instantiate(soundSourcePrefab, transform);
        return newSource;
    }

    public void ReturnSoundSource(SoundSource source)
    {
        source.gameObject.SetActive(false);
        soundSourcePool.Enqueue(source);
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

    private void LoadAllAudioClips()
    {
        var bgm = Resources.LoadAll<AudioClip>("Sounds/BGM");
        var sfx = Resources.LoadAll<AudioClip>("Sounds/SFX");

        foreach (var clip in bgm)
        {
            if (!bgmClips.ContainsKey(clip.name))
                bgmClips.Add(clip.name, clip);
        }

        foreach (var clip in sfx)
        {
            if (!sfxClips.ContainsKey(clip.name))
                sfxClips.Add(clip.name, clip);
        }
    }

    // 크릭 버튼 소리 추가 매서드
    private void AttachAllButtonClickSounds()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (var button in buttons)
        {
            button.onClick.RemoveListener(PlayClickSFX);  
            button.onClick.AddListener(PlayClickSFX);
        }
    }
    private void PlayClickSFX()
    {
        PlaySFX("Click");
    }
}
