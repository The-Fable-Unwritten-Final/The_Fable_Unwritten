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

    private static readonly Dictionary<int, string> lootNames = new()
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
            rewardText.text = GenerateText(GameManager.Instance.turnController.battleFlow);
            // 리워드 로드 + 텍스트 표시
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
                var setting = ProgressDataManager.Instance;
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

                if (setting.CurrentBattleNode.type == NodeType.EliteBattle)
                {
                    setting.EliteClear(setting.CurrentTheme);
                }
                ProgressDataManager.Instance.SavedEnemySetIndex = -1; // 랜덤 에너미 셋 초기화

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
                ProgressDataManager.Instance.RetryFromStart = true;
                ProgressDataManager.Instance.ClearStageState();

                // 최소 시작 스테이지부터 재시작 (1 또는 2)
                ProgressDataManager.Instance.StageIndex = ProgressDataManager.Instance.MinStageIndex;
                ProgressDataManager.Instance.GameStartType = GameStartType.New;
                ProgressDataManager.Instance.ResetProgress();

                SceneManager.LoadScene("SubTitleScene");
                gameObject.SetActive(false);

                // StageMoveTest.cs 를 임시로 가져만 왔음. 추후 전투 승리/패배시 기능 재 구현
            });
        }
    }
    

    private string GenerateText(BattleFlowController battleFlow)
    {
        if (battleFlow.recentLoots == null || battleFlow.recentLoots.Count == 0)
            return "획득한 전리품이 없습니다.";


        var countMap = battleFlow.recentLoots.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        List<string> lines = new();
        lines.Add($"Exp {battleFlow.totalExp}");
        foreach (var pair in countMap)
        {
            string name = lootNames.TryGetValue(pair.Key, out var result) ? result : $"알 수 없는 전리품({pair.Key})";
            lines.Add($"{name} x{pair.Value}");
        }

        return string.Join("\n", lines);
    }
}
