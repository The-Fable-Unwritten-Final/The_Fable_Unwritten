using HisaGames.Cutscene;
using HisaGames.CutsceneManager;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoSingleton<DialogueManager>
{
    [Header("JSON 데이터")]
    [SerializeField] private TextAsset jsonFile;

    [Header("컷씬 이름 (Hierarchy에 존재하는 오브젝트 이름)")]
    [SerializeField] private EcCutscene targetCutscene;


    private Dictionary<string, List<JsonCutsceneData>> dialogueDatabase;
    private bool isPlaying = false;


    protected override void Awake()
    {
        base.Awake();
        LoadDialogueFromJSON(jsonFile);

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
        dialogueDatabase = new Dictionary<string, List<JsonCutsceneData>>();

        foreach (var line in parsedList)
        {
            if (!dialogueDatabase.ContainsKey(line.id))
                dialogueDatabase[line.id] = new List<JsonCutsceneData>();

            dialogueDatabase[line.id].Add(line);
        }
    }
    /// <summary>
    /// 특정 ID에 맞는 대화를 실행
    /// </summary>
    public void PlayDialogue(string dialogueID, UnityAction onComplete = null)
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

        isPlaying = true;
        EcCutsceneManager.instance.guiPanel.SetActive(true);
        targetCutscene.gameObject.SetActive(true);
        targetCutscene.SetCutSceneData(data);
        targetCutscene.StartCutscene();

        StartCoroutine(WaitUntilCutsceneEnds(onComplete));
    }

    /// <summary>
    /// 컷씬 종료 시점을 기다리고 콜백 실행
    /// </summary>
    private IEnumerator WaitUntilCutsceneEnds(UnityAction callback = null)
    {
        yield return new WaitUntil(() => !targetCutscene.gameObject.activeSelf);
        isPlaying = false;
        callback?.Invoke();
    }

    public bool IsPlaying => isPlaying;

    public void PlayDialogueScene0()
    {
        PlayDialogue("scene_0");
    }

}
