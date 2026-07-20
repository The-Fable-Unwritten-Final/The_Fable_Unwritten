using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class EventEffectManager : MonoSingleton<EventEffectManager>
{
    [Header("CSV Data path")]
    [SerializeField] string csvPath = "ExternalFiles/EventEffects.csv"; // CSV 파일 경로

    List<EventEffects> eventEffectList;// 이벤트 효과 리스트.
    public Dictionary<int, EventEffects> eventEffectDict;// 데이터 외부 접근용 딕셔너리.

    // 액션 처럼 사용할 효과들 List
    List<EventEffects> untillNextCombat = new List<EventEffects>(); // 다음 전투까지 지속되는 효과 리스트
    List<EventEffects> untillNextStage = new List<EventEffects>(); // 다음 스테이지까지 지속되는 효과 리스트
    List<EventEffects> untillEndAdventure = new List<EventEffects>(); // 모험이 끝날 때까지 지속되는 효과 리스트


    // 이벤트 효과 변수들 저장 (각 효과 클래스에서 사용)
    public CardModel cardData; // 카드 획득시 팝업 UI에서 사용
    public int healModi = 0; // 힐량 변화 수치.

    protected override void Awake()
    {
        base.Awake();
        // CSV 파일에서 데이터 로드
        eventEffectList = LoadDatas(csvPath);
        eventEffectDict = eventEffectList.ToDictionary(effect => effect.index);
    }

    private List<EventEffects> LoadDatas(string csvPath)
    {
        string resourcePath = Path.ChangeExtension(csvPath, null);

        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null)
        {
            Debug.LogError($"[EventEffectManager] Resources/{resourcePath} 에서 CSV 파일을 찾을 수 없습니다.");
            return new List<EventEffects>();
        }

        var eventEffectList = new List<EventEffects>();
        var eventEffectDatas = EventEffectCSVParser.Parse(csvFile.text); // 경로 대신 텍스트 전달

        foreach (var data in eventEffectDatas)
        {
            switch (data.eventType)
            {
                case 0:
                    var statEffect = new StatEventEffects
                    {
                        index = data.index,
                        text = data.text,
                        eventType = data.eventType,
                        duration = data.duration,
                        sophia = data.sophia,
                        kyla = data.kyla,
                        leon = data.leon,
                        enemy = data.enemy,

                        hp = data.hp,
                        hpPercent = data.hpPercent,
                        atk = data.atk,
                        def = data.def
                    };
                    eventEffectList.Add(statEffect);
                    break;

                case 1: // 카드 관련 효과 전반
                    var cardEventEffect = new CardEventEffects
                    {
                        index = data.index,
                        text = data.text,
                        eventType = data.eventType,
                        duration = data.duration,
                        sophia = data.sophia,
                        kyla = data.kyla,
                        leon = data.leon,
                        unusable = data.unusable,

                        newCardIndex = data.newCardIndex,
                        cardType = data.cardType,
                        cost = data.cost,
                        healModi = data.healModi
                    };
                    eventEffectList.Add(cardEventEffect);
                    break;

                case 2: // 전투 인카운터 효과
                    var enemyEventEffect = new EncounterEventEffects
                    {
                        index = data.index,
                        text = data.text,
                        eventType = data.eventType,
                        duration = data.duration,
                        battle = data.battle,
                    };
                    eventEffectList.Add(enemyEventEffect);
                    break;

                case 3: // 상태이상류 효과 (기절, 출혈, 수호 등..)
                    var buffStatEffect = new BuffStatEventEffects
                    {
                        index = data.index,
                        text = data.text,
                        eventType = data.eventType,
                        duration = data.duration,
                        sophia = data.sophia,
                        kyla = data.kyla,
                        leon = data.leon,   
                        buffStatType = data.buffStatType,
                        buffStatValue = data.buffStatValue,
                        buffStatDuration = data.buffStatDuration
                    };
                    eventEffectList.Add(buffStatEffect);
                    break;

                case 4: // 노드 텔레포트 효과
                    var nodeTeleportEffect = new NodeTeleportEventEffect
                    {
                        index = data.index,
                        text = data.text,
                        eventType = data.eventType,
                        duration = data.duration
                    };
                    eventEffectList.Add(nodeTeleportEffect);
                    break;

                case 5: // 잉크 +- 효과
                    var inkEffect = new InkEventEffect
                    {
                        index = data.index,
                        text = data.text,
                        eventType = data.eventType,
                        duration = data.duration,
                        inkAmount = data.inkAmount
                    };
                    eventEffectList.Add(inkEffect);
                    break;
            }
        }

        return eventEffectList;
    }


    // List의 효과 사용 메서드
    /// <summary>
    /// "다음 전투까지 지속되는 효과"를 실행합니다.
    /// </summary>
    public void PlayNextCombat()
    {
        for (int i = 0; i < untillNextCombat.Count; i++)
        {
            untillNextCombat[i].Apply();
        }
    }
    /// <summary>
    /// "다음 전투까지 지속되는 효과" 리스트를 제거 합니다.
    /// </summary>
    public void EndNextCombat()
    {
        for (int i = 0; i < untillNextCombat.Count; i++)
        {
            untillNextCombat[i].UnApply(); // 효과 해제 메서드 호출
        }

        untillNextCombat.Clear();
        ProgressDataManager.Instance.UpdateEventEffectsData(untillNextCombat, untillNextStage, untillEndAdventure);
    }
    /// <summary>
    /// "다음 스테이지까지 지속되는 효과"를 실행합니다.
    /// </summary>
    public void PlayNextStage()
    {
        for (int i = 0; i < untillNextStage.Count; i++)
        {
            untillNextStage[i].Apply();
        }
    }
    /// <summary>
    /// "다음 스테이지까지 지속되는 효과" 리스트를 제거 합니다.
    /// </summary>
    public void EndNextStage()
    {
        for (int i = 0; i < untillNextStage.Count; i++)
        {
            untillNextStage[i].UnApply(); // 효과 해제 메서드 호출
        }
        untillNextStage.Clear();
        ProgressDataManager.Instance.UpdateEventEffectsData(untillNextCombat, untillNextStage, untillEndAdventure);
    }
    /// <summary>
    /// "모험이 끝날 때까지 지속되는 효과"를 실행합니다.
    /// </summary>
    public void PlayEndAdventure()
    {
        for (int i = 0; i < untillEndAdventure.Count; i++)
        {
            untillEndAdventure[i].Apply();
        }
    }
    /// <summary>
    /// "모험이 끝날 때까지 지속되는 효과" 리스트를 제거 합니다.
    /// <summary>
    public void EndAdventure()
    {
        for (int i = 0; i < untillEndAdventure.Count; i++)
        {
            untillEndAdventure[i].UnApply(); // 효과 해제 메서드 호출
        }
        untillEndAdventure.Clear();
        ProgressDataManager.Instance.UpdateEventEffectsData(untillNextCombat, untillNextStage, untillEndAdventure);
    }


    // List에 효과를 추가하는 메서드 + 즉시 효과 사용의 경우 실행.
    public void AddEventEffect(int index)
    {
        EventEffects effect = eventEffectList[index].Clone();
        switch (effect.duration)
        {
            case 0:
                effect.Apply();
                break;
            case 1:
                untillNextCombat.Add(effect);
                break;
            case 2:
                untillNextStage.Add(effect);
                break;
            case 3:
                untillEndAdventure.Add(effect);
                break;
        }

        // 현재 적용중인 효과 리스트들 저장 및 관리.
        ProgressDataManager.Instance.UpdateEventEffectsData(untillNextCombat,untillNextStage,untillEndAdventure);
    }

    public void LoadEventEffectsData(List<EventEffects> com, List<EventEffects> stage, List<EventEffects> adv)
    {
        untillNextCombat = com;
        untillNextStage = stage;
        untillEndAdventure = adv;
    }
}
