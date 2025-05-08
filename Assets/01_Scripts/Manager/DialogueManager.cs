using HisaGames.Cutscene;
using HisaGames.CutsceneManager;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueManager : MonoSingleton<DialogueManager>
{

    [Header("컷씬 이름 (Hierarchy에 존재하는 오브젝트)")]
    [SerializeField] private EcCutscene targetCutscene;

    [Header("컷씬 배경 이미지 (StageScene 전용)")]
    [SerializeField] private Image backgroundImage;

    private HashSet<string> playedCampScenes = new(); // 야영 이벤트 중복 방지용
    private bool isPlaying = false;


    protected override void Awake()
    {
        base.Awake();

        if (targetCutscene == null)
        {
            var allCutscenes = Resources.FindObjectsOfTypeAll<EcCutscene>();
            foreach (var cutscene in allCutscenes)
            {
                if (cutscene.name == "EcCutscene1") // 이름 확인
                {
                    targetCutscene = cutscene;
                    break;
                }
            }

            if (targetCutscene == null)
                Debug.LogError("[DialogueManager] targetCutscene 연결 실패 (비활성화 포함 탐색 실패)");
            else
                targetCutscene.gameObject.SetActive(false);
        }
        if (backgroundImage != null)
        {
            backgroundImage.enabled = false;
            backgroundImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 특정 ID에 맞는 대화를 실행
    /// </summary>
    public void PlayDialogue(string dialogueID)
    {
        if (isPlaying)
        {
            Debug.LogWarning("[DialogueManager] 대화 재생 중!");
            return;
        }

        if (!DataManager.Instance.DialogueDatabase.ContainsKey(dialogueID))
        {
            Debug.LogError($"[DialogueManager] ID '{dialogueID}' 대화 없음");
            return;
        }

        if (targetCutscene == null)
        {
            Debug.LogError("[DialogueManager] targetCutscene 연결 실패");
            return;
        }

        var data = JsonCutsceneLoader.Convert(DataManager.Instance.DialogueDatabase[dialogueID]);
        int stageIndex = ProgressDataManager.Instance.StageIndex;
        SetCutsceneBackground(stageIndex); // sprite 설정만

        // 컷씬 및 배경 활성화
        if (backgroundImage != null)
        {
            backgroundImage.enabled = true;
            backgroundImage.gameObject.SetActive(true);
        }

        isPlaying = true;
        targetCutscene.gameObject.SetActive(true);
        EcCutsceneManager.instance.guiPanel.SetActive(true);
        targetCutscene.SetCutSceneData(data);
        targetCutscene.StartCutscene();

        StartCoroutine(WaitUntilCutsceneEnds());
    }

    /// <summary>
    /// 컷씬 종료 시점을 기다리고 콜백 실행
    /// </summary>
    private IEnumerator WaitUntilCutsceneEnds(UnityAction callback = null)
    {
        yield return new WaitUntil(() => !targetCutscene.gameObject.activeSelf);
        isPlaying = false;

        if (targetCutscene != null)
        {
            targetCutscene.gameObject.SetActive(false);

            // 추가: 위치 초기화
            targetCutscene.transform.localPosition = Vector3.zero; // 또는 초기 위치 저장해두기
        }

        // 추가: 배경도 함께 꺼줌
        if (backgroundImage != null)
        {   
            backgroundImage.enabled = false;
            backgroundImage.gameObject.SetActive(false);
        }

        CutsceneEffectPlayer.Instance?.ClearAll();

        callback?.Invoke();
    }

    public bool IsPlaying => isPlaying;

    public void PlayDialogueScene0()
    {
        PlayDialogue("scene_0");
    }

    public void TryPlaySceneByTrigger(string key, UnityAction onComplete = null)
    {
        if (DataManager.Instance.DialogueTriggers.TryGetValue(key, out var sceneID))
        {
            PlayDialogue(sceneID);
        }
    }

    /// <summary>
    /// 스테이지 시작 시 스테이지 단계에 따라 대화 씬 출력
    /// </summary>
    /// <param name="stage">현재 스테이지</param>
    public void OnStageStart(int stage) => TryPlaySceneByTrigger($"StageStart:{stage}");

    /// <summary>
    /// 스테이지의 배틀 노드를 클릭했을 때에 맞는 대화 씬 출력
    /// </summary>
    /// <param name="stage">현재 스테이지</param>
    public void OnBattleNodeClicked(int stage, int nodeIndex) => TryPlaySceneByTrigger($"Battle:{stage}_{nodeIndex}");

    /// <summary>
    /// 스테이지의 엘리트 배틀 노드를 클리어 했을 때 대화 씬 출력
    /// </summary>
    /// <param name="stage">현재 스테이지</param>
    public void OnEliteBattleEnd(int stage) => TryPlaySceneByTrigger($"EliteEnd:{stage}");

    /// <summary>
    /// 스테이지의 보스 배틀 노드를 클리어 했을 때 대화 씬 출력 (5층)
    /// </summary>
    /// <param name="stage">현재 스테이지</param>
    public void OnBossClear(int stage) => TryPlaySceneByTrigger($"Boss:{stage}");

    /// <summary>
    /// 야영 중 야영 대화 이벤트 중 하나를 골라서 출력함
    /// </summary>
    /// <param name="onComplete"></param>
    public void OnCamp(CharacterClass character)
    {
        string charName = character.ToString(); // 예: "Sophia"
        List<string> candidateScenes = new();

        for (int i = 1; i <= 5; i++) // 최대 5개까지 있다고 가정
        {
            string key = $"Camp:{charName}:{i}";

            if (DataManager.Instance.DialogueTriggers.TryGetValue(key, out var sceneID))
            {
                if (!playedCampScenes.Contains(sceneID))
                {
                    playedCampScenes.Add(sceneID);
                    PlayDialogue(sceneID);
                    return;
                }
            }
        }

        Debug.Log($"[DialogueManager] {character}의 야영 컷씬은 모두 재생됨");
    }

    private void SetCutsceneBackground(int stageIndex)
    {
        if (backgroundImage == null) return;

        var bg = DataManager.Instance.GetBackground(stageIndex);

        if (bg != null)
            backgroundImage.sprite = bg;
        else
            Debug.LogWarning($"[DialogueManager] 스테이지 {stageIndex}에 대한 배경 스프라이트가 없습니다.");

        //  스프라이트는 유지하되, 평소엔 꺼둠
        backgroundImage.enabled = false;
        backgroundImage.gameObject.SetActive(false);
    }

}

[System.Serializable]
public class DialogueTriggerData
{
    public string index;
    public string bg;
    public string note;
}

