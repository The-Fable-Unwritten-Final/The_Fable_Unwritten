using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageMoveTest : MonoBehaviour
{
    // Start is called before the first frame update
     public void OnClear()
    {
        GameManager.Instance.retryFromStart = false;
        GameManager.Instance.stageCleared = true;

        if (GameManager.Instance.stageIndex == 1)
        {
            GameManager.Instance.minimumStageIndex = 2;
        }

        SceneManager.LoadScene("StageScene");
    }

    // Update is called once per frame
    public void OnFail()
    {
        GameManager.Instance.retryFromStart = true;
        GameManager.Instance.ClearStageState(); // 상태 초기화

        GameManager.Instance.stageIndex = GameManager.Instance.minimumStageIndex;

        SceneManager.LoadScene("StageScene");
    }
}
