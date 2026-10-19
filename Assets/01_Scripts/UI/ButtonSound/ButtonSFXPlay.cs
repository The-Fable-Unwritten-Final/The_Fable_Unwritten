using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSFXPlay : MonoBehaviour
{
    public SoundCategory soundCat;
    public int key;
    public float clickCooldown = 0.3f; // 클릭 간 최소 시간 간격 (초)
    public bool playOnce = false; // true: 최초 1회만 재생, false: 매번 재생
    
    private Button button;
    private float lastClickTime = -999f; // 마지막 클릭 시간
    private bool hasPlayed = false; // playOnce 모드에서 1회 재생 여부

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
        // playOnce 모드: 이미 재생했으면 무시
        if (playOnce && hasPlayed) return;
       
        // 쿨타임 체크: 마지막 클릭 이후 충분한 시간이 지났는지 확인
        if (Time.realtimeSinceStartup - lastClickTime < clickCooldown) return;
       
        lastClickTime = Time.realtimeSinceStartup;
        
        // playOnce 모드면 재생 완료 표시
        if (playOnce) hasPlayed = true;
        
        SoundManager.Instance.PlaySFX(soundCat, key);
    }
}
