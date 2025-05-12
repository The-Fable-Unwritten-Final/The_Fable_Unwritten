using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SoundCategory
{
    BGM,
    EvenetBGM,
    BossBGM,
    Button,
    Player,
    UI,
    Card,
    Enemy,
}

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

    private readonly Dictionary<SoundCategory, Dictionary<int, AudioClip>> bgmClips = new();
    private readonly Dictionary<SoundCategory, Dictionary<int, AudioClip>> sfxClips = new();

    private Dictionary<string, int> sceneToBGMKey = new()
    {

        { SceneNameData.TitleScene, 0 },
        { SceneNameData.RandomEventScene, 1 },
        { SceneNameData.StageScene, 1 },
        { SceneNameData.CampScene, 2 },
        { SceneNameData.CombatScene, 3 },
    };


    protected override void Awake()
    {
        base.Awake();
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;

        LoadAudioFromJson();

        SceneManager.sceneLoaded += OnSceneLoaded;

        SetBGMVolume(0);
        SetSFXVolume(0);
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var node = ProgressDataManager.Instance.CurrentBattleNode;
        var theme = ProgressDataManager.Instance.CurrentTheme;


        if (scene.name == SceneNameData.CombatScene &&
        node != null && node.type == NodeType.Boss)
        {
            PlayBossBGMByTheme(theme);
        }
        else if (sceneToBGMKey.TryGetValue(scene.name, out var bgmKey))
        {
            PlayBGM(SoundCategory.BGM, bgmKey);
        }
    }

    // ===== BGM =====

    /// <summary>
    /// BGM 변경 매서드
    /// </summary>
    public void PlayBGM(SoundCategory category, int key)
    {
        if (isMuted ||
            !bgmClips.TryGetValue(category, out var categoryDict) ||
            !categoryDict.TryGetValue(key, out var clip)) return;

        if (bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }


    /// <summary>
    /// BGM 전환 시 페이드 효과 매서드
    /// </summary>
    public void ChangeBGMWithFade(SoundCategory category, int key, float duration)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(ChangeBGMCoroutine(category, key, duration));
    }

    private IEnumerator ChangeBGMCoroutine(SoundCategory category, int key, float duration)
    {
        yield return FadeOut(duration * 0.5f);
        yield return FadeIn(category, key, duration * 0.5f);
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

    private IEnumerator FadeIn(SoundCategory category, int key, float duration)
    {
        if (!bgmClips.TryGetValue(category, out var categoryDict) || !categoryDict.TryGetValue(key, out var clip)) yield break;

        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        while (bgmSource.volume < bgmVolume)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, bgmVolume, (bgmVolume / duration) * Time.deltaTime);
            yield return null;
        }
    }

    public void PlayBossBGMByTheme(StageTheme theme)
    {
        PlayBGM(SoundCategory.BossBGM, (int)theme);
    }


    // ===== SFX =====

    /// <summary>
    /// 효과음 사용 매서드
    /// </summary>
    public void PlaySFX(SoundCategory category, int key)
    {
        if (Instance.isMuted) return;
        if (!Instance.sfxClips.TryGetValue(category, out var dict) || !dict.TryGetValue(key, out var clip)) return;

        Debug.Log("실행됨");
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
    /// BGM 효과음 조절
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = isMuted ? 0f : bgmVolume;
    }

    /// <summary>
    /// SFX 효과음 조절
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxVolume = isMuted ? 0f : sfxVolume;
    }

    private void LoadAudioFromJson()
    {
        TextAsset json = Resources.Load<TextAsset>("ExternalFiles/Sounds");
        if (json == null)
        {
            Debug.LogError("SoundManager: JSON file not found at Resources/ExternalFiles/Sounds.json");
            return;
        }

        string wrappedJson = $"{{\"sounds\":{json.text}}}";
        SoundDatabase database = JsonUtility.FromJson<SoundDatabase>(wrappedJson);

        foreach (var entry in database.sounds)
        {
            string rawCategory = entry.category?.Trim();

            if (!Enum.TryParse(rawCategory, true, out SoundCategory parsedCategory))
            {
                Debug.LogWarning($"[SoundManager] Invalid category: '{entry.category}' at index {entry.index}");
                continue;
            }

            string rawSound = entry.sound?.Trim();
            if (string.IsNullOrEmpty(rawSound))
            {
                Debug.LogWarning($"[SoundManager] Skipped (empty sound): category={rawCategory}, key={entry.key}");
                continue;
            }

            string path = $"Sounds/{rawSound}";
            AudioClip clip = Resources.Load<AudioClip>(path);

            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] Missing AudioClip at path: {path}");
                continue;
            }

            bool isBGMType = parsedCategory == SoundCategory.BGM || parsedCategory == SoundCategory.BossBGM || parsedCategory == SoundCategory.EvenetBGM;
            var targetDict = isBGMType ? bgmClips : sfxClips;

            if (!targetDict.ContainsKey(parsedCategory))
                targetDict[parsedCategory] = new Dictionary<int, AudioClip>();

            if (targetDict[parsedCategory].ContainsKey(entry.key))
                Debug.LogWarning($"[SoundManager] Duplicate {parsedCategory} key: {entry.key} — Overwriting.");

            targetDict[parsedCategory][entry.key] = clip;
        }
    }

    // 크릭 버튼 소리 추가 매서드
    public void AttachAllButtonClickSounds(int sfxIndex)
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => PlayButtonSFX(sfxIndex));
        }
    }
    private void PlayButtonSFX(int sfxIndex)
    {
        PlaySFX(SoundCategory.Button, sfxIndex);
    }
}

[System.Serializable]
public class SoundEntry
{
    public int index;
    public string category;
    public int key;
    public string sound;
}

[System.Serializable]
public class SoundDatabase
{
    public List<SoundEntry> sounds;
}