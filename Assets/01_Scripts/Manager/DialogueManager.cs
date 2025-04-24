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
    [Header("JSON 데이터")]
    [SerializeField] private TextAsset jsonFile;

    [Header("트리거 JSON (Trigger -> sceneID 매핑)")]
    [SerializeField] private TextAsset triggerJson;

    [Header("컷씬 이름 (Hierarchy에 존재하는 오브젝트)")]
    [SerializeField] private EcCutscene targetCutscene;

    [Header("컷씬 배경 이미지 (StageScene 전용)")]
    [SerializeField] private Image backgroundImage;


    private Dictionary<string, List<JsonCutsceneData>> dialogueDatabase;    //대화 데이터 저장용
    private Dictionary<string, string> noteToSceneIDMap;    //대화 트리거 확인용 
    private HashSet<string> playedCampScenes = new(); // 야영 이벤트 중복 방지용
    private bool isPlaying = false;


    protected override void Awake()
    {
        base.Awake();
        LoadDialogueFromJSON(jsonFile);
        LoadTriggerFromJSON(triggerJson);

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
        }
    }

    /// <summary>
    /// JSON 파일에서 대화 데이터를 로드하여 Dictionary로 저장
    /// </summary>
    private void LoadDialogueFromJSON(TextAsset json)
    {
        var parsedList = JsonCutsceneLoader.ParseFromJSON(json);
        dialogueDatabase = new();

        foreach (var line in parsedList)
        {
            if (!dialogueDatabase.ContainsKey(line.id))
                dialogueDatabase[line.id] = new List<JsonCutsceneData>();

            dialogueDatabase[line.id].Add(line);
        }
    }

    /// <summary>
    /// JSON 파일에서 대화 트리거를 로드하여 Dictionary에 저장
    /// </summary>
    /// <param name="triggerJson">대화 트리거 파일</param>
    private void LoadTriggerFromJSON(TextAsset triggerJson)
    {
        noteToSceneIDMap = new();

        List<DialogueTriggerData> triggerList = JsonUtilityWrapper.FromJsonList<DialogueTriggerData>(triggerJson.text);
        foreach (var data in triggerList)
        {
            string triggerKey = ParseNoteToTriggerKey(data.note);
            if (!string.IsNullOrEmpty(triggerKey))
            {
                noteToSceneIDMap[triggerKey] = data.index;
                Debug.Log($"[Trigger] 등록됨: {triggerKey} → {data.index}");
            }
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

        if (!dialogueDatabase.ContainsKey(dialogueID))
        {
            Debug.LogError($"[DialogueManager] ID '{dialogueID}' 대화 없음");
            return;
        }

        if (targetCutscene == null)
        {
            Debug.LogError("[DialogueManager] targetCutscene 연결 실패");
            return;
        }

        var data = JsonCutsceneLoader.Convert(dialogueDatabase[dialogueID]);
        int stageIndex = GameManager.Instance.StageSetting.StageIndex;
        SetCutsceneBackground(stageIndex);

        isPlaying = true;
        EcCutsceneManager.instance.guiPanel.SetActive(true);
        targetCutscene.gameObject.SetActive(true);
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
            backgroundImage.enabled = false;

        callback?.Invoke();
    }

    public bool IsPlaying => isPlaying;

    public void PlayDialogueScene0()
    {
        PlayDialogue("scene_0");
    }

    public void TryPlaySceneByTrigger(string key, UnityAction onComplete = null)
    {
        if (noteToSceneIDMap.TryGetValue(key, out var sceneID))
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

            if (noteToSceneIDMap.TryGetValue(key, out var sceneID))
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

    /// <summary>
    /// 파싱된 데이터의 note를 정제
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    private string ParseNoteToTriggerKey(string note)
    {
        if (note.Contains("시작 전"))
        {
            var stage = System.Text.RegularExpressions.Regex.Match(note, "\\d+").Value;
            return $"StageStart:{stage}";
        }
        if (note.Contains("전투 노드 클릭"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(note, "\\d+번째");
            var node = match.Value.Replace("번째", "");
            var stage = System.Text.RegularExpressions.Regex.Match(note, "스테이지 (\\d+)").Groups[1].Value;
            return $"Battle:{stage}_{node}";
        }
        if (note.Contains("엘리트 전투 후"))
        {
            var stage = System.Text.RegularExpressions.Regex.Match(note, "\\d+").Value;
            return $"EliteEnd:{stage}";
        }
        if (note.Contains("보스 전투 승리 후"))
        {
            var stage = System.Text.RegularExpressions.Regex.Match(note, "\\d+").Value;
            return $"Boss:{stage}";
        }
        if (note.StartsWith("야영"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(note, @"야영\s*(\S+)\s*(\d+)");
            if (match.Success)
            {
                string charName = match.Groups[1].Value.Trim();  // 예: 소피아
                string index = match.Groups[2].Value.Trim();      // 예: 1, 2
                return $"Camp:{charName}:{index}";
            }
        }
        return null;
    }

    private void SetCutsceneBackground(int stageIndex)
    {
        var bg = GameManager.Instance.StageSetting.GetBackground(stageIndex);
        if (backgroundImage != null)
        {
            backgroundImage.sprite = bg;
            backgroundImage.enabled = (bg != null);
            backgroundImage.gameObject.SetActive(bg != null);
        }
    }

}

[System.Serializable]
public class DialogueTriggerData
{
    public string index;
    public string bg;
    public string note;
}

