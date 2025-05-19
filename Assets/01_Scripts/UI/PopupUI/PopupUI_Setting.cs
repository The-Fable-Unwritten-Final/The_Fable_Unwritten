using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupUI_Setting : BasePopupUI
{
    [Header("Sound")]
    [SerializeField] Slider BGMvolumSlider;
    [SerializeField] Slider SFXvolumSlider;

    [Header("Resoolution")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
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

            // 현재 해상도와 일치하는 항목 찾기
            if (Screen.width == res.x && Screen.height == res.y)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex; 
        resolutionDropdown.RefreshShownValue(); 
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
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


    //.Find 를 활용하는 코드이고, 쓰고있는곳이 없어 보여서 일단은 주석 처리했어요.
    /*
    public void OnDropdownClicked()
    {
        StartCoroutine(MoveBlockerToResolution());
    }

    IEnumerator MoveBlockerToResolution()
    {
        
        GameObject blocker = GameObject.Find("Blocker");
        if (blocker != null)
        {
            blocker.transform.SetParent(resolutionTransform, false);
            blocker.transform.SetAsFirstSibling();
        }
        yield return null;
    }*/
}
