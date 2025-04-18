using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageMoveTest : MonoBehaviour
{
     public void OnClear()
    {
        GameManager.Instance.retryFromStart = false;
        GameManager.Instance.stageCleared = true;

        // 1 스테이지 클리어 후, 실패 시 2스테이지부터 시작하게 설정
        if (GameManager.Instance.stageIndex == 2)
        {
            GameManager.Instance.minimumStageIndex = 2;
        }

        SceneManager.LoadScene("StageScene");
    }

    public void OnFail()
    {
        GameManager.Instance.retryFromStart = true;
        GameManager.Instance.ClearStageState();

        // 최소 시작 스테이지부터 재시작 (1 또는 2)
        GameManager.Instance.stageIndex = GameManager.Instance.minimumStageIndex;

        SceneManager.LoadScene("StageScene");
    }
}
