using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupUI_Setting : BasePopupUI
{
    [Header("Sound")]
    [SerializeField] Slider volumSlider;

    [Header("Resoolution")]
    [SerializeField] TMP_Dropdown resolutionDropdown;

    private readonly Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1280, 720),
    };

    
    private void OnEnable()
    {   
        // 사운드 셋팅
        volumSlider = GetComponentInChildren<Slider>();
        volumSlider.value = SoundManager.Instance.bgmVolume;
        volumSlider.onValueChanged.AddListener(OnVolumChange);

        // 해상도 셋팅
        resolutionDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var res in resolutions)
        {
            options.Add(new TMP_Dropdown.OptionData($"{res.x}X{res.y}"));
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }
    private void OnResolutionChanged(int index)
    {
        var selectedResolution = resolutions[index];
        Screen.SetResolution(selectedResolution.x, selectedResolution.y, false);

        Debug.Log($"Requested resolution: {selectedResolution.x}x{selectedResolution.y}");
        Debug.Log($"Actual resolution: {Screen.width}x{Screen.height}");
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
        if(SceneManager.GetActiveScene().name == SceneNameData.CombatScene) //전투씬 에서의 타이틀로 돌아가기.
        {
            UIManager.Instance.ShowPopupByName("PopupUI_ReturnTitle");
        }
        else // 다른 씬에서의 타이틀로 돌아가기.
        {
            Close();
            SceneManager.LoadScene(SceneNameData.SubTitleScene);
        }
    }

 
}
