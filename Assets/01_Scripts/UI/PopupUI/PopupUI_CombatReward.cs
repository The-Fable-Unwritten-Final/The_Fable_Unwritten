using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupUI_CombatReward : BasePopupUI
{
    [SerializeField] TextMeshProUGUI resultText; // 결과 텍스트
    [SerializeField] TextMeshProUGUI rewardText; // 리워드 텍스트
    [SerializeField] Button confirmButton;

    private void OnEnable()
    {
        short iswin = GameManager.Instance.turnController.battleFlow.isWin;
        if(iswin == 1)
        {
            resultText.text = "전투 승리";
            // 리워드 로드 + 텍스트 표시
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
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

                SceneManager.LoadScene("StageScene");
                gameObject.SetActive(false);

                // StageMoveTest.cs 를 임시로 가져만 왔음. 추후 전투 승리/패배시 기능 재 구현
            });
        }
        else
        {
            resultText.text = "전투 패배";
            // 리워드 로드 + 텍스트 표시
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                GameManager.Instance.StageSetting.RetryFromStart = true;
                GameManager.Instance.StageSetting.ClearStageState();

                // 최소 시작 스테이지부터 재시작 (1 또는 2)
                GameManager.Instance.StageSetting.StageIndex = GameManager.Instance.StageSetting.MinStageIndex;
                GameManager.Instance.gameStartType = GameStartType.New;

                SceneManager.LoadScene("TitleScene");
                gameObject.SetActive(false);

                // StageMoveTest.cs 를 임시로 가져만 왔음. 추후 전투 승리/패배시 기능 재 구현
            });
        }
    }
}
