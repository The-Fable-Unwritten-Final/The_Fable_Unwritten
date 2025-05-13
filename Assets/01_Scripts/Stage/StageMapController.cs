using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 스테이지 흐름을 제어하는 메인 매니저 클래스
/// </summary>
public class StageMapController : MonoBehaviour
{
    [Header("Stage Settings")]
    [SerializeField] Image backGround; // stage별 백그라운드 설정
    [SerializeField] Vector2 spacing = new(300, 200);      // 노드 간격
    [SerializeField] int mapTargetWidth;                   // 지도 가로 크기
    public StageMapRenderer mapRenderer;                   // StageMapRederer 연결

    private int stageIndex = 1;

    private StageData stageData;                           // 현재 스테이지 데이터
    private readonly List<GraphNode> visitedNodes = new(); // 방문한 노드 목록


    private void Start()
    {
        var pd = ProgressDataManager.Instance;

        pd.IsStageScene = true;
        pd.InitializePlayerHPByGameType();

        if (!TryRestoreStage())
        {
            var theme = pd.GetThemeForStage(pd.StageIndex);
            pd.SetTheme(theme);

            LoadStage(pd.StageIndex);

            // 기획자 요청으로 대화씬 스킵
            DialogueManager.Instance.OnStageStart(pd.StageIndex); // 대화 호출
        }
        
        int stageIndex = pd.StageIndex;
        backGround.sprite = DataManager.Instance.GetBackground(stageIndex);

        pd.SaveProgress();
    }

    // 저장된 상태가 있다면 복원 시도
    private bool TryRestoreStage()
    {
        var stageSetting = ProgressDataManager.Instance;
        
        if (stageSetting.SavedStageData != null && !stageSetting.RetryFromStart)
        {
            if (stageSetting.StageCleared)
            {
                var lastVisited = stageSetting.VisitedNodes.LastOrDefault();
                bool wasLastColumnNode = stageSetting.SavedStageData.columns[^1].Contains(lastVisited); // 보스 노드 방문 여부

                // 보스 노드 클리어면 다음 스테이지
                if (wasLastColumnNode)
                {
                    stageSetting.StageIndex++;

                    var newTheme = stageSetting.GetThemeForStage(stageSetting.StageIndex);
                    stageSetting.SetTheme(newTheme);

                    stageSetting.ClearStageState();
                    stageIndex = stageSetting.StageIndex;
                    stageSetting.StageCleared= false;

                    LoadStage(stageIndex);
                    DialogueManager.Instance.OnStageStart(stageSetting.StageIndex); // 대화 호출
                    return true;
                }
            }
            // 스테이지 데이터 복구 (야영지, 이벤트, 인게임에서 돌아 왔을 경우)
            stageData = stageSetting.SavedStageData;  // 게임 매니저에서 데이터 불러오기
            visitedNodes.Clear();           
            visitedNodes.AddRange(stageSetting.VisitedNodes);
            stageIndex = stageSetting.StageIndex;
            stageSetting.StageCleared = false;

            mapRenderer.Render(stageData, OnNodeClicked);
            mapRenderer.CenterMap();

            var lastNode = visitedNodes.LastOrDefault() ?? stageData.columns[0].First();
            mapRenderer.UpdateInteractables(lastNode, visitedNodes);
            return true;
        }

        return false;
    }

    // 노드 클릭 시 스테이지 호출 및 저장
    private void OnNodeClicked(GraphNode clicked)
    {
        visitedNodes.Add(clicked);

        var pdm = ProgressDataManager.Instance;
        pdm.SaveStageState(stageData, visitedNodes);
        pdm.SetCurrentBattleNode(clicked);

        // 노드 클릭 시 저장
        pdm.IsStageScene = false;
        pdm.SaveProgress();
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 1
            );

        int stageIndex = pdm.StageIndex;
        int columnIndex = clicked.columnIndex;

        if (stageIndex == 1 && (clicked.type == NodeType.NormalBattle))
        {
            CharacterClass charToAdd = CharacterClass.Sophia;

            switch (columnIndex)
            {
                case 1:
                    charToAdd = CharacterClass.Sophia;
                    break;
                case 2:
                    charToAdd = CharacterClass.Kayla;
                    break;
                case 3:
                    charToAdd = CharacterClass.Leon;
                    break;
            }

            var playerData = pdm.PlayerDatas.FirstOrDefault(p => p.CharacterClass == charToAdd);
            if (playerData != null)
            {
                PlayerManager.Instance.AddPlayerDuringGame(playerData, DataManager.Instance.AllCards);
            }
        }

        // 노드 타입별 씬 전환
        switch (clicked.type)
        {
            case NodeType.NormalBattle:
            case NodeType.EliteBattle:
            case NodeType.Boss:
                SceneManager.LoadScene(SceneNameData.CombatScene);
                return;

            case NodeType.Camp:
                SceneManager.LoadScene(SceneNameData.CampScene);
                return;

            case NodeType.RandomEvent:
                SceneManager.LoadScene(SceneNameData.RandomEventScene);
                return;
        }
   
        if (IsLastColumnNode(clicked))
        {
            // 마지막 열(보스노드)의 경우 스테이지 증가
            pdm.StageIndex++;
        }
        else
        {
            // 중간 노드의 경우 다음 스테이지 연결
            mapRenderer.UpdateInteractables(clicked, visitedNodes);
        }     
    }

    // 클릭된 노드가 마지막 열에 있는지 확인
    private bool IsLastColumnNode(GraphNode clicked)
        => stageData.columns[^1].Contains(clicked);


    // 스테이지를 불러오고 맵 초기화
    private void LoadStage(int index)
    {
        // 맵 정보 초기화
        mapRenderer.ClearMap();
        visitedNodes.Clear();

        stageData = RebuildStage(index);
        mapRenderer.Render(stageData, OnNodeClicked);
        mapRenderer.CenterMap();

        DisableAllNodeButtons();
        ActivateStartNode();
    }

    // 스테이지 생성 및 spacing 보정
    private StageData RebuildStage(int index)
    {
        var stage = StageGraphGenerator.Generate(index, spacing);
        AdjustSpacing(stage.columnCount);
        return StageGraphGenerator.Generate(index, spacing);
    }


    // 열 개수에 따라 spacing 조절
    private void AdjustSpacing(int columnCount)
    {
        spacing.x = mapTargetWidth / Mathf.Max(1, columnCount - 1);
    }

    // 모든 노드 비활성화
    private void DisableAllNodeButtons()
    {
        foreach (var column in stageData.columns)
        {
            foreach (var node in column)
            {
                var ui = mapRenderer.nodeUIMap[node];
                var btn = ui.GetComponent<Button>();

                ui.gameObject.SetActive(true);
                btn.enabled = false;
                btn.interactable = false;                
            }
        }
    }

    // 시작 지점 노드 활성화
    private void ActivateStartNode()
    {
        var startNode = stageData.columns[0].First();
        visitedNodes.Add(startNode);

        var startUI = mapRenderer.nodeUIMap[startNode];
        var startBtn = startUI.GetComponent<Button>();
        startBtn.enabled = true;
        startBtn.interactable = true;
        startBtn.image.color = Color.white;

        mapRenderer.UpdateInteractables(startNode, visitedNodes);
    }
}
