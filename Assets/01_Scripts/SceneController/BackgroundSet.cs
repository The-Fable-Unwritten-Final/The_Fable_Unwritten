using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundSet : MonoBehaviour
{
    [SerializeField] CombatLightingController combatLightingController; // 전투 조명
    [SerializeField] Material backgroundMaterial;
    [SerializeField] Image backgroundImage;
    [SerializeField] Material combatCamMaterial;

    private void Start()
    {
        int stageIndex = ProgressDataManager.Instance.StageIndex;
        //배경 설정
        //backgroundMaterial.SetTexture("_BaseMap", DataManager.Instance.GetBackground(stageIndex).texture);
        // >> 마테리얼을 사용한 world space 에서의 배경 변경 방식에서, Canvas를 사용한 UI 배경방식으로 변경
        // >> 기존에 world spcae를 사용한 이유는, 스킬 사용 시 뒷배경에도 동시에 반응하는 lighting 효과를 주려 했으나, 더 이상 필요 없게 됨.
        backgroundImage.sprite = DataManager.Instance.GetBackground(stageIndex);

        // 전투 카메라 배경 설정
        combatCamMaterial.SetTexture("_BaseMap", DataManager.Instance.GetBattleCamImage(stageIndex).texture);
        // 조명 설정
        combatLightingController.SetLighting((CombatLightingController.LightingState)(stageIndex - 1));
    }

}
