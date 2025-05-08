using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestCustomWindow : EditorWindow
{
    [MenuItem("Tools/Player Test Window")]
    public static void ShowWindow()
    {
        GetWindow<TestCustomWindow>("Tester");
    }

    private void OnGUI()
    {
        GUILayout.Label("테스트 유닛 추가", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("전체 추가"))
        {
            TryAddPlayer(CharacterClass.Leon, "LeonPlayer");
            TryAddPlayer(CharacterClass.Sophia, "SopiaPlayer");
            TryAddPlayer(CharacterClass.Kayla, "KaylaPlayer");
        }
        GUILayout.Space(8);
        if (GUILayout.Button("레온 추가"))
        {
            TryAddPlayer(CharacterClass.Leon, "LeonPlayer");
        }
        if (GUILayout.Button("소피아 추가"))
        {
            TryAddPlayer(CharacterClass.Sophia, "SopiaPlayer");
        }
        if (GUILayout.Button("카일라 추가"))
        {
            TryAddPlayer(CharacterClass.Kayla, "KaylaPlayer");
        }
        EditorGUILayout.EndVertical();


        GUILayout.Space(20);


        GUILayout.Label("스테이지 클리어 / 실패 즉시 실행", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        if(GUILayout.Button("스테이지 클리어"))
        {
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

            SceneManager.LoadScene("StageScene");
        }
        if (GUILayout.Button("스테이지 실패"))
        {
            var setting = ProgressDataManager.Instance;
            setting.RetryFromStart = true;
            setting.ClearStageState();
            GameManager.Instance.gameStartType = GameStartType.New;

            // 최소 시작 스테이지부터 재시작 (1 또는 2)
            setting.StageIndex = setting.MinStageIndex;

            SceneManager.LoadScene("TitleScene");
        }
        EditorGUILayout.EndVertical();


        GUILayout.Space(20);


        GUILayout.Label("전투 관련 기능", EditorStyles.boldLabel);
        var turnController = GameManager.Instance?.turnController;
        var battleFlow = turnController?.battleFlow;

        if (GUILayout.Button("전체 체력 회복"))
        {
            foreach (var player in GameManager.Instance.playerDatas)
            {
                player.currentHP = player.MaxHP;
            }
        }


        GUILayout.Space(20);


        EditorGUILayout.BeginHorizontal(); // 한 줄로 배치
        if(battleFlow == null) // 전투 씬 입장 전, 예외처리.
        {
            EditorGUILayout.HelpBox("현재 씬에 전투 컨트롤러가 없습니다.", MessageType.Info);
        }
        else
        {
            GUILayout.Label("    현재 마나:  ", GUILayout.Width(70));
            GUILayout.Label(GameManager.Instance.turnController.battleFlow.currentMana.ToString(), GUILayout.Width(20));

            if (GUILayout.Button("+ 5 마나", GUILayout.Width(70)))
            {
                battleFlow.currentMana += 5;
                battleFlow.Mana.text = battleFlow.currentMana.ToString();
                EditorUtility.SetDirty(battleFlow);
            }
        }
        EditorGUILayout.EndHorizontal();

    }

    private void TryAddPlayer(CharacterClass chClass, string name)
    {
        var playerDatas = GameManager.Instance.playerDatas;
        var targetData = playerDatas.Find(p => p.CharacterClass == chClass);
        if (targetData == null) return;

        PlayerManager.Instance.AddPlayerDuringGame(
            targetData,
            DataManager.Instance.AllCards
        );
    }

}
