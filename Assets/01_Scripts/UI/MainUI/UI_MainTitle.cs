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
    [SerializeField] GameObject saveGame;

    private void Start()
    {
        StartCoroutine(ShowTitle());
        SetSaveGameButton();
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
        ProgressDataManager pg = ProgressDataManager.Instance;

        pg.GameStartType = GameStartType.New;

        // 플레이어 덱 초기화
        foreach (var player in PlayerManager.Instance.activePlayers.Values)
        {
            player.currentDeckIndexes.Clear();
        }

        // 보유 플레이어 초기화
        PlayerManager.Instance.activePlayers.Clear();

        // 데이터 초기화
        if(pg.IsSecondGame) pg.ResetProgress(); // 튜토리얼을 클리어 한 이후 New Game 시 호출
        else pg.FullResetProgress(); // 처음 플레이 시 호출

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

        // 저장된 게임 데이터 로드
        ProgressDataManager.Instance.LoadProgress();
        
        // 전투 입장 시점의 상태 복원 (진행 중 저장된 상태 무시)
        ProgressDataManager.Instance.RestoreBattleEntryState();

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

    private void SetSaveGameButton()
    {
        if (ProgressDataManager.Instance.StageIndex == 0 ||
            !ProgressDataManager.Instance.RetryFromStart) return;

        // 버튼 비활성화 및 글자 선명도 조정
        saveGame.GetComponent<Button>().interactable = false;
        TextMeshProUGUI tmp = saveGame.GetComponentInChildren<TextMeshProUGUI>();
        Color c = tmp.color;
        c.a = 140f / 255f;
        tmp.color = c;
    }
    private void OnClickUnlockCard()
    {
        //해금팝업 열기
    }

    public void OnClickExit()
    {
        // 저장하고 나가기
        ProgressDataManager.Instance.SaveProgress(true);
        Application.Quit();
    }

    public void OnClickButtonSound()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
    }
}
