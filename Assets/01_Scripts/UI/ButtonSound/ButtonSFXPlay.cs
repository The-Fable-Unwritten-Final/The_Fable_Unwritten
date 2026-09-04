using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSFXPlay : MonoBehaviour
{
    public SoundCategory soundCat;
    public int key;
    
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayButtonSound);
        }
    }

    private void PlayButtonSound()
    {
        SoundManager.Instance.PlaySFX(soundCat, key);
    }
}
