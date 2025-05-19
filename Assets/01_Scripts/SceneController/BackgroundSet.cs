using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSet : MonoBehaviour
{
    [SerializeField] CombatLightingController combatLightingController; // 전투 조명
    [SerializeField] Material backgroundMaterial;
    [SerializeField] Material combatCamMaterial;

    private void Start()
    {
        int stageIndex = ProgressDataManager.Instance.StageIndex;
        //배경 설정
        backgroundMaterial.SetTexture("_BaseMap", DataManager.Instance.GetBackground(stageIndex).texture);
        // 전투 카메라 배경 설정
        combatCamMaterial.SetTexture("_BaseMap", DataManager.Instance.GetBattleCamImage(stageIndex).texture);
        // 조명 설정
        combatLightingController.SetLighting((CombatLightingController.LightingState)(stageIndex - 1));
    }

}
