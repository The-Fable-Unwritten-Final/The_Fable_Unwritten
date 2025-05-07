using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    [Header("CSV Data path")]
    // csv

    [Header("Data Loaded")]
    // 다이어리 데이터
    private List<DiaryData>[] diaryGroups = new List<DiaryData>[5]; // tag_num 0~4 (스테이지 1~5까지)
    public IReadOnlyList<DiaryData>[] DiaryGroups => diaryGroups; // 외부에서 읽기 전용으로 접근 가능

    // 카드북 카드 데이터
    private Dictionary<int, CardModel> cardForShopia = new();
    private Dictionary<int, CardModel> cardForKayla = new();
    private Dictionary<int, CardModel> cardForLeon = new();

    public IReadOnlyDictionary<int, CardModel> CardForShopia => cardForShopia; // 외부에서 읽기 전용으로 접근 가능
    public IReadOnlyDictionary<int, CardModel> CardForKayla => cardForKayla; // 외부에서 읽기 전용으로 접근 가능
    public IReadOnlyDictionary<int, CardModel> CardForLeon => cardForLeon; // 외부에서 읽기 전용으로 접근 가능

    // 스테이지 데이터
    private List<EnemyStageSpawnData> enemySpawnData;

    // 랜덤 이벤트 데이터
    public List<RandomEventData> allRandomEvents { get; set;}

    // 백그라운드 이미지 데이터
    private Dictionary<int, Sprite> stageBackgrounds;

    protected override void Awake()
    {
        base.Awake();
        InitDiaryDictionary();
        enemySpawnData = StageSpawnSetCSVParser.LoadEnemySpawnSet() ?? new();
        allRandomEvents = RandomEventJsonLoader.LoadAllEvents() ?? new();
        stageBackgrounds = BackgoundLoader.LoadBackgrounds() ?? new();
        //InitCardBookDictionary();
    }
    private void Start()
    {
        InitCardBookDictionary(); // 나중에 동환님쪽 카드데이터 로드함수 이후에 이거 넣어주세요.
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
        List<CardModel> allcards = CardSystemInitializer.Instance.loadedCards;
        foreach (var card in allcards)
        {
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
}
