using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampBackgroundSet : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Animator fireFlareAnimator;
    
    void Start()
    {
        int stageIndex = ProgressDataManager.Instance.StageIndex;
        
        // 6, 7, 8, 9 스테이지를 2~5로 매핑
        if (stageIndex >= 6)
        {
            stageIndex = stageIndex - 4;  // 6>2, 7>3, 8>4, 9>5
        }
        
        // 캠프 배경 설정
        Sprite sprite = DataManager.Instance.GetCampBackground(stageIndex);
        backgroundImage.sprite = sprite;
        Debug.Log($"Sprite: {sprite}");
        Debug.Log($"Texture: {sprite.texture}");
        Debug.Log($"Bounds: {sprite.bounds}");
        
        // 모닥불 반사광 색 변경 (모든 스테이지에서 휴식 씬은 파란 배경으로 설정)
        fireFlareAnimator.SetBool("IsBlueFlare", true);
    }
}
