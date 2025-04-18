using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageMoveTest : MonoBehaviour
{
     public void OnClear()
    {
        GameManager.Instance.StageSetting.RetryFromStart = false;
        GameManager.Instance.StageSetting.StageCleared = true;

        // 1 스테이지 클리어 후, 실패 시 2스테이지부터 시작하게 설정
        if (GameManager.Instance.StageSetting.StageIndex == 2)
        {
            GameManager.Instance.StageSetting.MinStageIndex = 2;
        }

        SceneManager.LoadScene("StageScene");
    }

    public void OnFail()
    {

        GameManager.Instance.StageSetting.RetryFromStart = true;
        GameManager.Instance.StageSetting.ClearStageState();

        // 최소 시작 스테이지부터 재시작 (1 또는 2)
        GameManager.Instance.StageSetting.StageIndex = GameManager.Instance.StageSetting.MinStageIndex;

        SceneManager.LoadScene("StageScene");
    }
}
