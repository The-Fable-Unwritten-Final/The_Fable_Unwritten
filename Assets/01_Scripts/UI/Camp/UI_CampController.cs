using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CampController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] Sprite[] campBackgrounds;

    [Header("PanelConnect")]
    [SerializeField] GameObject restPanel;
    [SerializeField] GameObject trainingPanel;
    [SerializeField] GameObject readingPanel;
    [SerializeField] GameObject conversationPanel;

    private void Start()
    {
        SetBackGround();
    }

    // 스테이지에 따른 백그라운드 설정
    private void SetBackGround()
    {
        int stageIndex = GameManager.Instance.StageSetting.StageIndex;

        if (stageIndex >= 2 && stageIndex <= 4)
        {
            int index = stageIndex - 2; // 2스테이지 = 0번 인덱스 ...
            if (index >= 0 && index < campBackgrounds.Length)
            {
                background.sprite = campBackgrounds[index];
            }      
        }

    }

    // 각패널 버튼 등록
    public void ShowRestPanel() => ShowPanel(restPanel);
    public void ShowTrainingPanel() => ShowPanel(trainingPanel);
    public void ShowReadingPanel() => ShowPanel(readingPanel);
    public void ShowConversationPanel() => ShowPanel(conversationPanel);

    private void ShowPanel(GameObject target)
    {
        restPanel.SetActive(target == restPanel);
        trainingPanel.SetActive(target == trainingPanel);
        readingPanel.SetActive(target == readingPanel);
        conversationPanel.SetActive(target == conversationPanel);
    }

    
}
