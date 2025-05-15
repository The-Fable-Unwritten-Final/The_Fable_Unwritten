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
    [SerializeField] BaseCampPanel restPanel;
    [SerializeField] BaseCampPanel trainingPanel;
    [SerializeField] BaseCampPanel readingPanel;
    [SerializeField] BaseCampPanel conversationPanel;

    private BaseCampPanel currentPanel;
    private void Start()
    {
        SetBackGround();
    }

    // 스테이지에 따른 백그라운드 설정
    private void SetBackGround()
    {
        int stageIndex = ProgressDataManager.Instance.StageIndex;

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

    private void ShowPanel(BaseCampPanel panel)
    {
        ProgressDataManager.Instance.IsNewCamp = false;
        currentPanel?.Close();
        currentPanel = panel;
        currentPanel.Open();
    }
}
