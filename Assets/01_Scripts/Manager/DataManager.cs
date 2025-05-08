using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    [Header("CSV Data path")]
    // csv

    [Header("Data Loaded")]
    // 다이어리 데이터
    private List<DiaryData>[] diaryGroups = new List<DiaryData>[5]; // tag_num 0~4 (스테이지 1~5까지)
    private List<CardModel> allCards = new();
    //카드 통합
    private Dictionary<int, CardModel> cardLookup = new();
    // 카드북 카드 데이터
    private Dictionary<int, CardModel> cardForShopia = new();
    private Dictionary<int, CardModel> cardForKayla = new();
    private Dictionary<int, CardModel> cardForLeon = new();
    //효과 스프라이트
    private Dictionary<string, List<Sprite>> cardEffects = new();
    //적 행동
    private Dictionary<int, EnemyAct> enemyActDict = new();
    //대화 트리거
    private Dictionary<string, string> dialogueTriggers = new();
    //대화 내용
    private Dictionary<string, List<JsonCutsceneData>> dialogueDatabase = new();
    public List<CardModel> AllCards => allCards;
    // 적 데이터
    [SerializeField]
    public EnemyDataContainer enemyDataContainer;
    // 스테이지 데이터
    private List<EnemyStageSpawnData> enemySpawnData;
    // 랜덤 이벤트 데이터
    public List<RandomEventData> allRandomEvents { get; set;}
    // 백그라운드 이미지 데이터
    private Dictionary<int, Sprite> stageBackgrounds;

    public IReadOnlyList<DiaryData>[] DiaryGroups => diaryGroups; // 외부에서 읽기 전용으로 접근 가능
    public IReadOnlyDictionary<int, CardModel> CardLookup => cardLookup;
    public IReadOnlyDictionary<int, CardModel> CardForShopia => cardForShopia; // 외부에서 읽기 전용으로 접근 가능
    public IReadOnlyDictionary<int, CardModel> CardForKayla => cardForKayla; // 외부에서 읽기 전용으로 접근 가능
    public IReadOnlyDictionary<int, CardModel> CardForLeon => cardForLeon; // 외부에서 읽기 전용으로 접근 가능
    public IReadOnlyDictionary<string, List<Sprite>> CardEffects => cardEffects;        //카드 애니메이션 스프라이트
    public IReadOnlyDictionary<int, EnemyAct> EnemyActDict => enemyActDict;        //적 스킬 유형
    public IReadOnlyDictionary<string, string> DialogueTriggers => dialogueTriggers;    //대화 트리거
    public IReadOnlyDictionary<string, List<JsonCutsceneData>> DialogueDatabase => dialogueDatabase;


    protected override void Awake()
    {
        base.Awake();
        InitDiaryDictionary();
        enemySpawnData = StageSpawnSetCSVParser.LoadEnemySpawnSet() ?? new();
        allRandomEvents = RandomEventJsonLoader.LoadAllEvents() ?? new();
        stageBackgrounds = BackgoundLoader.LoadBackgrounds() ?? new();
        InitEnemySkillDictionary();
        InitDialogueTriggers();
        InitDialogueDatabase();
        InitCardEffectSprites();
        InitCardBookDictionary();

    }

    /// <summary>
    /// 다이어리의 데이터를 JSON 파일에서 로드합니다.
    /// </summary>
    private void InitDiaryDictionary()
    {
        // 추후 스테이지의 진행도 데이터를 받아와서, diaryData의 isOpen를 설정.
        // EX) int storyComplete == 11 이면, list의 0~10까지의 isOpen을 true로 설정.

        // Resources 폴더에 있는 DiaryData.json 로드
        TextAsset jsonText = Resources.Load<TextAsset>("ExternalFiles/DiaryData"); // 확장자 제거
        if (jsonText == null)
        {
            Debug.LogError("[InitDictionary] DiaryData.json not found.");
            return;
        }

        for (int i = 0; i < 5; i++)
            diaryGroups[i] = new List<DiaryData>();

        DiaryListWrapper wrapper = JsonUtility.FromJson<DiaryListWrapper>(jsonText.text);
        foreach (var data in wrapper.diaries)
        {
            if (data.tag_num >= 0 && data.tag_num < 5)
                diaryGroups[data.tag_num].Add(data);
        }

        // 정렬까지 해놓고 저장
        for (int i = 0; i < 5; i++)
            diaryGroups[i] = diaryGroups[i].OrderBy(d => d.index).ToList();
    }

    /// <summary>
    /// 카드북 카드 데이터를 초기화 + 분류작업.
    /// </summary>
    private void InitCardBookDictionary()
    {
        allCards = CardDatabaseLoader.LoadAll("ExternalFiles/Cards");
        cardLookup.Clear();
        foreach (var card in allCards)
        {
            cardLookup[card.index] = card;

            ///이부분은 나중에 최적화 위해 이야기 필요할 것 같습니다. 분류가 필수라면 이쪽을 남기는게 좋을 수도 있겠네요.
            if (card.characterClass == CharacterClass.Sophia)
            {
                cardForShopia.Add(card.index, card);
            }
            else if (card.characterClass == CharacterClass.Kayla)
            {
                cardForKayla.Add(card.index, card);
            }
            else if (card.characterClass == CharacterClass.Leon)
            {
                cardForLeon.Add(card.index, card);
            }
        }
    }

    /// <summary>
    /// 특정 스테이지 및 노드타입에 해당하는 적 배치 데이터 리스트 반환
    /// </summary>

    public List<EnemyStageSpawnData> GetEnemySpawnData(StageTheme theme, NodeType type)
    {
        var result = enemySpawnData
        .Where(x => x.theme == theme && x.type == type)
        .ToList();

        Debug.Log($"[StageSettingController] 스폰 데이터 검색: Theme={theme}, Type={type}, 결과={result.Count}개");

        return result;
    }

    /// <summary>
    /// 특정 스테이지에 해당하는 배경 이미지 반환
    /// </summary>
    public Sprite GetBackground(int stageIndex)
    {
        return stageBackgrounds.TryGetValue(stageIndex, out var sprite) ? sprite : null;
    }

    /// <summary>
    /// 적 스킬 로딩
    /// </summary>
    private void InitEnemySkillDictionary()
    {
        TextAsset csvText = Resources.Load<TextAsset>("ExternalFiles/EnemyAct");
        if (csvText == null)
        {
            Debug.LogError("[DataManager] EnemyAct.csv를 Resources/ExternalFiles/ 에서 찾을 수 없습니다.");
            return;
        }

        List<EnemyAct> parsedActs = EnemyActCSVParser.ParseEnemyAct(csvText.text);
        enemyActDict.Clear();

        foreach (var act in parsedActs)
        {
            if (!enemyActDict.ContainsKey(act.index))
                enemyActDict.Add(act.index, act);
            else
                Debug.LogWarning($"[DataManager] 중복 스킬 인덱스: {act.index}");
        }

        Debug.Log($"[DataManager] EnemySkill {enemyActDict.Count}개 등록 완료.");
    }

    /// <summary>
    /// 대화 로딩
    /// </summary>
    private void InitDialogueTriggers()
    {
        TextAsset triggerJson = Resources.Load<TextAsset>("ExternalFiles/DialogueTrigger");
        if (triggerJson == null)
        {
            Debug.LogError("[DataManager] DialogueTrigger.json이 없습니다.");
            return;
        }

        dialogueTriggers.Clear();
        List<DialogueTriggerData> triggerList = JsonUtilityWrapper.FromJsonList<DialogueTriggerData>(triggerJson.text);

        foreach (var data in triggerList)
        {
            string triggerKey = DialogueTriggerParser.ParseNoteToKey(data.note);
            if (!string.IsNullOrEmpty(triggerKey))
            {
                dialogueTriggers[triggerKey] = data.index;
            }
        }

        Debug.Log($"[DataManager] Dialogue Trigger {dialogueTriggers.Count}개 등록 완료");
    }

    private void InitDialogueDatabase()
    {
        TextAsset json = Resources.Load<TextAsset>("ExternalFiles/scene_0_cutscene");
        if (json == null)
        {
            Debug.LogError("[DataManager] DialogueData.json 이 없습니다.");
            return;
        }

        dialogueDatabase.Clear();
        List<JsonCutsceneData> parsedList = JsonCutsceneLoader.ParseFromJSON(json);

        foreach (var line in parsedList)
        {
            if (!dialogueDatabase.ContainsKey(line.id))
                dialogueDatabase[line.id] = new List<JsonCutsceneData>();

            dialogueDatabase[line.id].Add(line);
        }

        Debug.Log($"[DataManager] DialogueDatabase에 총 {dialogueDatabase.Count}개 씬 등록 완료");
    }

    private void InitCardEffectSprites()
    {
        string basePath = "CardSkillEffects";

        TextAsset folderListAsset = Resources.Load<TextAsset>($"{basePath}/SkillEffectFolders");
        if (folderListAsset == null)
        {
            Debug.LogError("[DataManager] SkillEffectFolders.txt 파일을 찾을 수 없습니다.");
            return;
        }

        string[] folderNames = folderListAsset.text.Split('\n');

        foreach (var folderNameRaw in folderNames)
        {
            string folderName = folderNameRaw.Trim();
            if (string.IsNullOrEmpty(folderName)) continue;

            Sprite[] sprites = Resources.LoadAll<Sprite>($"{basePath}/{folderName}");
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[DataManager] {folderName} 폴더에 스프라이트 없음.");
                continue;
            }

            cardEffects[folderName] = new List<Sprite>(sprites);
        }

        Debug.Log($"[DataManager] 총 {cardEffects.Count}개의 카드 이펙트 그룹 로드 완료.");
    }

}
