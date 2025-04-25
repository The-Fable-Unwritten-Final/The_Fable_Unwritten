using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageMoveTest : MonoBehaviour
{
     public void OnClear()
    {
        var setting = GameManager.Instance.StageSetting;
        setting.RetryFromStart = false;
        setting.StageCleared = true;

        // 1 스테이지 클리어 후, 실패 시 2스테이지부터 시작하게 설정
        if (setting.StageIndex == 1)
        {
            var lasVisitde = setting.VisitedNodes.Last();
            var lasColum = setting.SavedStageData.columns[^1];

            if (lasColum.Contains(lasVisitde))
            {
                setting.MinStageIndex = 2;
            }            
        }

        if (setting.CurrentBattleNode.type == NodeType.EliteBattle)
        {
            setting.EliteClear(setting.CurrentTheme);
        }

        SceneManager.LoadScene("StageScene");
    }

    public void OnFail()
    {

        GameManager.Instance.StageSetting.RetryFromStart = true;
        GameManager.Instance.StageSetting.ClearStageState();

        // 최소 시작 스테이지부터 재시작 (1 또는 2)
        GameManager.Instance.StageSetting.StageIndex = GameManager.Instance.StageSetting.MinStageIndex;

        SceneManager.LoadScene("TitleScene");
    }
}
