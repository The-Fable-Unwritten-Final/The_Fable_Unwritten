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
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform rewardContentParent;
    [SerializeField] private Sprite[] lootIcons; // 0 ~ 3 아이템 아이콘 순서대로 지정
    [SerializeField] Button confirmButton;

    private readonly Dictionary<int, string> lootNames = new()
    {
        { 0, "백색의 결정" },
        { 1, "지혜의 잎사귀" },
        { 2, "사랑의 물약" },
        { 3, "용기의 돌덩이" }
    };


    private void OnEnable()
    {
        short iswin = GameManager.Instance.turnController.battleFlow.isWin;
        if(iswin == 1)
        {
            SoundManager.Instance.PlaySFX(SoundCategory.UI, 5); // 승리 시 효과음 적용
            resultText.text = "전투 승리";
            GenerateRewardUI(GameManager.Instance.turnController.battleFlow);
            var setting = ProgressDataManager.Instance;


            // 리워드 로드 + 텍스트 표시
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
                
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

                if (setting.CurrentBattleNode.type == NodeType.Boss || 
                    (setting.StageIndex == 1 && setting.CurrentBattleNode.columnIndex == 3))
                {
                    setting.IsNewStage = true;
                }
                else
                {
                    setting.IsNewStage = false;
                }

                if (setting.CurrentBattleNode.type == NodeType.EliteBattle)
                {
                    setting.EliteClear(setting.CurrentTheme);
                }
                ProgressDataManager.Instance.SavedEnemySetIndex = -1; // 랜덤 에너미 셋 초기화

                gameObject.SetActive(false);

                // 앤딩일시 표시(*유저 테스트용*)
                if (setting.CurrentBattleNode.type == NodeType.Boss
                    && setting.StageIndex == 3)
                {
                    GameManager.Instance.tutorialController.ShowTutorial(9);
                    ProgressDataManager.Instance.ResetProgress();
                    return;
                }

                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);

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
                ProgressDataManager.Instance.RetryFromStart = true;
                ProgressDataManager.Instance.ClearStageState();

                // 최소 시작 스테이지부터 재시작 (1 또는 2)
                ProgressDataManager.Instance.StageIndex = ProgressDataManager.Instance.MinStageIndex;
                ProgressDataManager.Instance.GameStartType = GameStartType.New;
                ProgressDataManager.Instance.ResetProgress();

                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.SubTitleScene);
                gameObject.SetActive(false);

                // StageMoveTest.cs 를 임시로 가져만 왔음. 추후 전투 승리/패배시 기능 재 구현
            });
        }
    }
    

    private void GenerateRewardUI(BattleFlowController battleFlow)
    {
        foreach(Transform child in rewardContentParent)
        {
            Destroy(child.gameObject);
        }

        if (battleFlow.recentLoots == null || battleFlow.recentLoots.Count == 0) return;

        var countMap = battleFlow.recentLoots.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        foreach (var pair in countMap)
        {
            int lootIndex = pair.Key;
            int count = pair.Value;
            string name = lootNames.TryGetValue(lootIndex, out var result) ? result : $"알 수 없는 전리품 : {lootIndex}";
            Sprite icon = lootIndex < lootIcons.Length ? lootIcons[lootIndex] : null;

            var itemGo = Instantiate(itemPrefab, rewardContentParent);
            var rewardItem = itemGo.GetComponent<RewardItemPrefab>();
            rewardItem.SetItem(icon, name, count);
        }
    }
}
