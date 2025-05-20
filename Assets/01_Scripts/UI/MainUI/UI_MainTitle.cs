using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainTitle : MonoBehaviour
{
    [Header("TitleSetting")]
    [SerializeField] Image title;
    [SerializeField] float titleSpeed;

    private void Start()
    {
        StartCoroutine(ShowTitle());   
    }

    private IEnumerator ShowTitle()
    {
        title.fillAmount = 0f;

        while(title.fillAmount < 1f)
        {
            title.fillAmount += Time.deltaTime * titleSpeed;
            yield return null;
        }

        title.fillAmount = 1f;
    }

    public void OnClickNewGame()
    {
        ProgressDataManager.Instance.GameStartType = GameStartType.New;

        // 플레이어 덱 초기화
        foreach (var player in PlayerManager.Instance.activePlayers.Values)
        {
            player.currentDeckIndexes.Clear();
        }

        // 보유 플레이어 초기화
        PlayerManager.Instance.activePlayers.Clear();

        // 데이터 초기화
        ProgressDataManager.Instance.ResetProgress();

        if (ProgressDataManager.Instance.IsSecondGame)
        {
            UIManager.Instance.PopupUnlockUI();          
            GameManager.Instance.tutorialController.ShowTutorial(8);

#if UNITY_EDITOR
            ProgressDataManager.Instance.IsSecondGame = false;
#endif
            return;
        }

        ProgressDataManager.Instance.IsSecondGame = true; // 인게임에서만 적용

        // 애널리틱스
        GameManager.Instance.analyticsLogger.LogReplayInfo();
        // 씬 전환
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
    }

    public void OnClickSaveGame()
    {
        var currentNode = ProgressDataManager.Instance.CurrentBattleNode;

        if (currentNode == null || ProgressDataManager.Instance.IsStageScene)
        {
            UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
            return;
        }

        switch (currentNode.type)
        {
            case NodeType.NormalBattle:
            case NodeType.EliteBattle:
            case NodeType.Boss:
                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.CombatScene);
                break;

            case NodeType.Camp:
                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.CampScene);
                break;
            case NodeType.RandomEvent:
                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.RandomEventScene);
                break;
            default:
                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
                break;
        }
    }

    private void OnClickUnlockCard()
    {
        //해금팝업 열기
    }

    public void OnClickExit()
    {
        // 저장하고 나가기
        ProgressDataManager.Instance.SaveProgress();
        Application.Quit();
    }

    public void OnClickButtonSound()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
    }
}
