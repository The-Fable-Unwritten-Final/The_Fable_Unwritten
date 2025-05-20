using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupUI_ReturnTitle : BasePopupUI
{
    public void OnClickToTitle()
    {
        var setting = ProgressDataManager.Instance;
        var lasVisitde = setting.VisitedNodes.Last();

        // 애널리틱스
        GameManager.Instance.analyticsLogger.LogStageFailInfo(setting.SavedStageData.stageIndex, lasVisitde.columnIndex); // 전투 패배 시 애널리틱스 기록

        CloseAll();
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.SubTitleScene);
    }

    void CloseAll()
    {
        foreach (BasePopupUI popup in UIManager.Instance.popupStack.ToList()) // List로 변환해 Close로 pop을 해서 예외처리
        {
            popup.Close();
        }
    }
}
