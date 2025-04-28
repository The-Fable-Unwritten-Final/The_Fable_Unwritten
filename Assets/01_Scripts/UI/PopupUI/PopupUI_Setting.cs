using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupUI_Setting : BasePopupUI
{
    [SerializeField] private Slider volumSlider;

    
    private void OnEnable()
    {        
        volumSlider = GetComponentInChildren<Slider>();
        volumSlider.value = SoundManager.Instance.bgmVolume;
        volumSlider.onValueChanged.AddListener(OnVolumChange);
    }
    public void OnVolumChange(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
        SoundManager.Instance.SetSFXVolume(value);
    }

}
