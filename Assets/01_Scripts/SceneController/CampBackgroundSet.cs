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
        
        // 6, 7, 8, 9, 10 스테이지를 1~5로 매핑
        if (stageIndex >= 6)
        {
            stageIndex = stageIndex - 5;  // 6→1, 7→2, 8→3, 9→4, 10→5
        }
        
        // 캠프 배경 설정
        Sprite sprite = DataManager.Instance.GetCampBackground(stageIndex);
        backgroundImage.sprite = sprite;
        Debug.Log($"Sprite: {sprite}");
        Debug.Log($"Texture: {sprite.texture}");
        Debug.Log($"Bounds: {sprite.bounds}");
        
        // 모닥불 반사광 색 변경 (2,4,5 쪽 스테이지는 파란 배경이라 파란 반사광 재생)
        if(stageIndex == 2 || stageIndex == 4 || stageIndex == 5 || stageIndex == 7 || stageIndex == 9 || stageIndex == 10)
        {
            fireFlareAnimator.SetBool("IsBlueFlare", true);
        }
    }
}
