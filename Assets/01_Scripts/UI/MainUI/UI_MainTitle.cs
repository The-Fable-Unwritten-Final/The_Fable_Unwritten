using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainTitle : MonoBehaviour
{
    [Header("Diary & Setting Button")]
    [SerializeField] Button settingBtn;

    [Header("MenuButton")]
    [SerializeField] Button newBtn;
    [SerializeField] Button continueBtn;
    [SerializeField] Button unlockCardBtn;
    [SerializeField] Button exitBtn;

    private void Awake()
    {
        settingBtn.onClick.AddListener(OnClickSettingPopup);
    }

    private void OnClickNewGame()
    {
        // 게임 데이터 초기화 로직 추가 (DataManager 나 GameManager에 로직추가?)
        SceneManager.LoadScene(SceneNameData.StageScene);
    }
    private void OnClickSaveGame()
    {
        // 플레이어정보(덱, 재료) 및 Stage 진행 정도 정보 가지고 오기
        SceneManager.LoadScene(SceneNameData.StageScene);
    }

    private void OnClickUnlockCard()
    {
        //해금팝업 열기
    }

    private void OnClickExit()
    {
        //게임 데이터 저장
        //게임 종료 로직 추가
    }

    // 세팅 팝업 열어주는 매서드
    private void OnClickSettingPopup()
    {
        UIManager.Instance.ShowPopup<PopupUI_Setting>();
    }
}
