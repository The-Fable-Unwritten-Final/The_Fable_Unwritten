using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class PopupUI_Setting : BasePopupUI
{
    private static readonly Dictionary<string, string> _localeDisplayNames = new Dictionary<string, string>()
    {
    { "en", "English" },
    { "ko", "한국어" },
    { "ja", "日本語" },
    };

    [Header("Sound")]
    [SerializeField] Slider BGMvolumSlider;
    [SerializeField] Slider SFXvolumSlider;

    [Header("Resoolution")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown localeDropdown;
    bool isChangingLocale;
    [SerializeField] Transform resolutionTransform;

    private readonly Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1280, 720),
    };

    
    private void OnEnable()
    {
        OnClickButtonSound();
        // 사운드 셋팅
        BGMvolumSlider.value = SoundManager.Instance.bgmVolume;
        BGMvolumSlider.onValueChanged.AddListener(OnBGMVolumChange);

        SFXvolumSlider.value = SoundManager.Instance.sfxVolume;
        SFXvolumSlider.onValueChanged.AddListener(OnSFXVolumChange);

        // 해상도 셋팅
        resolutionDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            var res = resolutions[i];
            options.Add(new TMP_Dropdown.OptionData($"{res.x}X{res.y}"));

            // 현재 해상도와 일치하는 항목 찾기 (주의: 전체화면 모드일 경우 Screen.width/height가 바뀔 수 있음)
            if (Mathf.Abs(Screen.width - res.x) <= 10 && Mathf.Abs(Screen.height - res.y) <= 10) // 근사 비교
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        InitLocaleDropdown();
    }

    public void OnBGMVolumChange(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);

    }

    public void OnSFXVolumChange(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }

    private void OnResolutionChanged(int index)
    {
        var selectedResolution = resolutions[index];
        Screen.SetResolution(selectedResolution.x, selectedResolution.y, false);   
        ProgressDataManager.Instance.resolutions = new Vector2Int[] { new Vector2Int(selectedResolution.x, selectedResolution.y) };
        ProgressDataManager.Instance.SaveProgress(true); // 해상도 변경 시 저장
    }


    public void OnVolumChange(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
        SoundManager.Instance.SetSFXVolume(value);
    }
    public void CreditPanel()
    {
        UIManager.Instance.ShowPopupByName("PopupUI_CreditsPanel");
    }
    public void GoToTitle()
    {
        if(DialogueManager.Instance.IsPlaying)
        {
            DialogueManager.Instance.ForceStopDialogue();
        }
        if (SceneManager.GetActiveScene().name == SceneNameData.CombatScene) //전투씬 에서의 타이틀로 돌아가기.
        {
            
            UIManager.Instance.ShowPopupByName("PopupUI_ReturnTitle");
        }
        else // 다른 씬에서의 타이틀로 돌아가기.
        {
            Close();
            UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.SubTitleScene);
        }
    }

    public void OnClickButtonSound()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
    }




    // 언어 변경 드롭다운 설정
    private void InitLocaleDropdown()
    {
        localeDropdown.ClearOptions();

        var locales = LocalizationSettings.AvailableLocales.Locales;
        var options = new List<TMP_Dropdown.OptionData>();
        int currentLocaleIndex = 0;

        for (int i = 0; i < locales.Count; i++)
        {
            var locale = locales[i];
            string code = locale.Identifier.Code; // "en", "ko", "ja"

            if (!_localeDisplayNames.TryGetValue(code, out string displayName))
                displayName = locale.LocaleName; // fallback

            options.Add(new TMP_Dropdown.OptionData(displayName));

            if (LocalizationSettings.SelectedLocale == locale)
                currentLocaleIndex = i;
        }

        localeDropdown.AddOptions(options);
        localeDropdown.value = currentLocaleIndex;
        localeDropdown.RefreshShownValue();
        localeDropdown.onValueChanged.AddListener(ChangeLocale);
    }
    private void ChangeLocale(int index)
    {
        //  사용자가 선택한 Locale
        var targetLocale = LocalizationSettings.AvailableLocales.Locales[index];

        // 현재 적용중인 Locale와 비교, 동일한 경우 return
        if (LocalizationSettings.SelectedLocale == targetLocale)
            return;

        if (isChangingLocale) return;
        StartCoroutine(ChangeLocaleCoroutine(index));

    }

    IEnumerator ChangeLocaleCoroutine(int index)
    {
        isChangingLocale = true;

        // 로케일 변경
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

        isChangingLocale = false;

        // 로케일 변경후 텍스트 데이터 업데이트
        DataManager.Instance.InitLocaleText();
    }
}
