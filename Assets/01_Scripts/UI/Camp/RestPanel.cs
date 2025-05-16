using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestPanel : BaseCampPanel
{
    public void OnClickConfirm()
    {
        // 보유중인 캐릭터 현채체력 10 증가 매서드 추가

        EventEffectManager.Instance.AddEventEffect(0); // 야영 휴식 현재 체력 10 증가
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
    }
}
