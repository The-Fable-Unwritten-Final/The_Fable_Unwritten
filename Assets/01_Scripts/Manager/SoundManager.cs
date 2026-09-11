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
    SubBGM,
    RandomEventBGM,
    EventBGM,
    BossBGM,
    Button,
    Player,
    UI,
    Card,
    Enemy,
    SFX,
}

public class SoundManager : MonoSingleton<SoundManager>
{
    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource subBGMSource;
    [Range(0, 1f)] public float bgmVolume = 1f;

    [Header("SFX Settings")]
    [SerializeField] private SoundSource soundSourcePrefab;
    [Range(0, 1f)] public float sfxVolume = 1f;
    [SerializeField] private float sfxPitchVariance = 0.1f;

    [Header("Mute")]
    private bool isMuted = false;

    private Coroutine fadeCoroutine;
    private Queue<SoundSource> soundSourcePool = new();
    
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 25;
    [SerializeField] private int maxPoolSize = 60;

    private readonly Dictionary<SoundCategory, Dictionary<int, AudioClip>> bgmClips = new();
    private readonly Dictionary<SoundCategory, Dictionary<int, AudioClip>> sfxClips = new();

    private Dictionary<string, int> sceneToBGMKey = new()
    {

        { SceneNameData.TitleScene, 0 },
        { SceneNameData.SubTitleScene, 0 },
        { SceneNameData.StageScene, 1 },
        { SceneNameData.CampScene, 2 },
        { SceneNameData.CombatScene, 3 },
        { SceneNameData.RandomEventScene, 4},
    };


    protected override void Awake()
    {
        base.Awake();
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;

        // 서브 BGM용 AudioSource 생성
        GameObject subBGMObject = new GameObject("SubBGMSource");
        subBGMObject.transform.SetParent(transform);
        subBGMSource = subBGMObject.AddComponent<AudioSource>();
        subBGMSource.loop = true;

        LoadAudioFromJson();

        // 오브젝트 풀 사전 초기화
        InitializePool();

        SceneManager.sceneLoaded += OnSceneLoaded;

        SetBGMVolume(0.25f);
        SetSFXVolume(0.25f);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var node = ProgressDataManager.Instance.CurrentNode;
        var theme = ProgressDataManager.Instance.CurrentTheme;

        if(scene.name != SceneNameData.TitleScene)
            PlaySFX(SoundCategory.SFX, 201); // 최초 시점 제외 씬 이동 마다 책 넘기는 사운드

        /*if (scene.name == SceneNameData.CombatScene &&
        node != null && node.type == NodeType.Boss)
        {
            PlayBossBGMByTheme(theme);
        } else*/
        if (sceneToBGMKey.TryGetValue(scene.name, out var bgmKey))
        {
            if (bgmKey == 3)
            {
                switch (ProgressDataManager.Instance.CurrentNode?.type)
                {
                    case NodeType.NormalBattle:
                        PlayBGM(SoundCategory.BGM, 31); // 보스 전투 BGM
                        return;
                    case NodeType.EliteBattle:
                        PlayBGM(SoundCategory.BGM, 32); // 엘리트 전투 BGM
                        return;
                    case NodeType.Boss: // 추후 5스테이지 작업 끝나면 여기서 한번 더 분기 생성
                        PlayBGM(SoundCategory.BGM, 33); // 보스 전투 BGM
                        return;
                }
            }
            else
            {
                PlayBGM(SoundCategory.BGM, bgmKey);
            }
        }

        if (scene.name == SceneNameData.CombatScene)
        {
            PlaySubBGM(ProgressDataManager.Instance.StageIndex);
        }
        else if(scene.name == SceneNameData.CampScene)
        {
            PlaySubBGM(0); // 캠프씬 서브 bgm key 0
        }
        else
        {
            StopSubBGM();
        }
    }

    // ===== BGM =====

    /// <summary>
    /// BGM 변경 매서드
    /// </summary>
    public void PlayBGM(SoundCategory category, int key)
    {
        if (isMuted) return;

        // 요청한 category/key가 있으면 그것 사용
        if (bgmClips.TryGetValue(category, out var categoryDict) && categoryDict.TryGetValue(key, out var clip))
        {
            if (bgmSource.clip == clip) return;

            // 현재 재생 중인 BGM과 다르면 페이드 적용, 같으면 즉시 재생
            if (bgmSource.clip != null)
            {
                ChangeBGMWithFade(category, key, 1f);  // 1초 페이드
            }
            else
            {
                bgmSource.Stop();
                bgmSource.clip = clip;
                bgmSource.volume = bgmVolume;
                bgmSource.Play();
            }
            return;
        }

        // 없으면 디폴트 재생 (BGM, 0)
        Debug.LogWarning($"[SoundManager] BGM not found: category={category}, key={key}. Falling back to default (BGM, 0)");

        if (bgmClips.TryGetValue(SoundCategory.BGM, out var defaultCategoryDict) && defaultCategoryDict.TryGetValue(0, out var defaultClip))
        {
            if (bgmSource.clip == defaultClip) return;

            // 디폴트도 페이드 적용
            if (bgmSource.clip != null)
            {
                ChangeBGMWithFade(SoundCategory.BGM, 0, 1f);
            }
            else
            {
                bgmSource.Stop();
                bgmSource.clip = defaultClip;
                bgmSource.volume = bgmVolume;
                bgmSource.Play();
            }
        }
        else
        {
            Debug.LogError("[SoundManager] Default BGM (BGM, 0) also not found!");
        }
    }
    /// <summary>
    /// 효과음 사용 매서드
    /// </summary>
    public void PlaySFX(SoundCategory category, int key)
    {
        if (Instance.isMuted) return;
        if (!Instance.sfxClips.TryGetValue(category, out var dict) || !dict.TryGetValue(key, out var clip)) return;

        var source = Instance.GetSoundSource();
        source.Play(clip, Instance.sfxVolume, Instance.sfxPitchVariance);
    }

    /// <summary>
    /// 딜레이가 적용된 효과음 사용 매서드
    /// </summary>
    public void PlaySFX(SoundCategory category, int key, float delay)
    {
        if (Instance.isMuted) return;
        if (!Instance.sfxClips.TryGetValue(category, out var dict) || !dict.TryGetValue(key, out var clip)) return;

        if (delay > 0)
        {
            Instance.StartCoroutine(Instance.PlaySFXDelayed(category, key, clip, delay));
        }
        else
        {
            var source = Instance.GetSoundSource();
            source.Play(clip, Instance.sfxVolume, Instance.sfxPitchVariance);
        }
    }

    private IEnumerator PlaySFXDelayed(SoundCategory category, int key, AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isMuted) yield break;

        var source = GetSoundSource();
        source.Play(clip, sfxVolume, sfxPitchVariance);
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
        // 요청한 category/key 시도
        AudioClip clip = null;
        if (bgmClips.TryGetValue(category, out var categoryDict) && categoryDict.TryGetValue(key, out var foundClip))
        {
            clip = foundClip;
        }
        else
        {
            // 해당하는 카테고리 및 key가 없을 경우, 디폴트 재생 (BGM, 0)
            Debug.LogWarning($"[SoundManager] BGM not found for FadeIn: category={category}, key={key}. Falling back to default (BGM, 0)");
            if (bgmClips.TryGetValue(SoundCategory.BGM, out var defaultCategoryDict) && defaultCategoryDict.TryGetValue(0, out var defaultClip))
            {
                clip = defaultClip;
            }
            else
            {
                Debug.LogError("[SoundManager] Default BGM (BGM, 0) also not found for FadeIn!");
                yield break;
            }
        }

        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        while (bgmSource.volume < bgmVolume)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, bgmVolume, (bgmVolume / duration) * Time.deltaTime);
            yield return null;
        }
    }
    public void PlayBGMForCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        var node = ProgressDataManager.Instance.CurrentNode;
        var theme = ProgressDataManager.Instance.CurrentTheme;

        if (scene.name == SceneNameData.CombatScene &&
            node != null && node.type == NodeType.Boss)
        {
            //PlayBossBGMByTheme(theme);
        }
        else if (sceneToBGMKey.TryGetValue(scene.name, out var bgmKey))
        {
            PlayBGM(SoundCategory.BGM, bgmKey);
        }
    }

    /// <summary>
    /// 서브 BGM 재생 매서드 (Fade In)
    /// </summary>
    public void PlaySubBGM(int key, bool loop = true)
    {
        if (isMuted) return;
        if (!bgmClips.TryGetValue(SoundCategory.SubBGM, out var categoryDict) || 
            !categoryDict.TryGetValue(key, out var clip)) return;

        // 같은 클립이면 유지
        if (subBGMSource.clip == clip && subBGMSource.isPlaying) return;

        subBGMSource.loop = loop;
        subBGMSource.clip = clip;
        StartCoroutine(SubBGMFadeIn(duration: 1f));
    }

    private IEnumerator SubBGMFadeIn(float duration)
    {
        subBGMSource.volume = 0f;
        subBGMSource.Play();

        while (subBGMSource.volume < bgmVolume)
        {
            subBGMSource.volume = Mathf.MoveTowards(subBGMSource.volume, bgmVolume, (bgmVolume / duration) * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 서브 BGM 중지 매서드 (Fade Out)
    /// </summary>
    public void StopSubBGM(float duration = 1f)
    {
        StartCoroutine(SubBGMFadeOut(duration));
    }

    private IEnumerator SubBGMFadeOut(float duration)
    {
        float start = subBGMSource.volume;
        while (subBGMSource.volume > 0)
        {
            subBGMSource.volume = Mathf.MoveTowards(subBGMSource.volume, 0f, (start / duration) * Time.deltaTime);
            yield return null;
        }
        subBGMSource.Stop();
    }
    private SoundSource GetSoundSource()
    {
        if (soundSourcePool.Count > 0)
        {
            var src = soundSourcePool.Dequeue();
            src.gameObject.SetActive(true);
            return src;
        }

        // 최대 풀 크기를 초과하지 않으면 새로 생성
        if (soundSourcePool.Count < maxPoolSize)
        {
            var newSource = Instantiate(soundSourcePrefab, transform);
            return newSource;
        }

        // 최대 크기 초과 시 경고 로그 및 재사용 강제
        Debug.LogWarning($"[SoundManager] SFX pool exceeded max size ({maxPoolSize}). Reusing oldest source.");
        var reusedSource = soundSourcePool.Dequeue();
        reusedSource.gameObject.SetActive(true);
        return reusedSource;
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var source = Instantiate(soundSourcePrefab, transform);
            source.gameObject.SetActive(false);
            soundSourcePool.Enqueue(source);
        }
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

    // Mute 토글 시 서브 BGM도 함께 처리
    public void SetMute(bool mute)
    {
        isMuted = mute;
        bgmSource.volume = isMuted ? 0f : bgmVolume;
        subBGMSource.volume = isMuted ? 0f : bgmVolume;
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
                Debug.LogWarning($"[SoundManager] Invalid category: '{entry.category}'");
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

            bool isBGMType = parsedCategory == SoundCategory.BGM || parsedCategory == SoundCategory.BossBGM || parsedCategory == SoundCategory.EventBGM || parsedCategory == SoundCategory.RandomEventBGM || parsedCategory == SoundCategory.SubBGM;
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
    public string category;
    public int key;
    public string sound;
}

[System.Serializable]
public class SoundDatabase
{
    public List<SoundEntry> sounds;
}