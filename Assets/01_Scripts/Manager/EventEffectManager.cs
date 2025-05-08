using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventEffectManager : MonoSingleton<EventEffectManager>
{
    [Header("CSV Data path")]
    [SerializeField] string csvPath = "ExternalFiles/EventEffects.csv"; // CSV 파일 경로

    //[Header("Event effect Lists")]
    List<EventEffects> eventEffectList;// 이벤트 효과 리스트.
    public Dictionary<int, EventEffects> eventEffectDict;// 데이터 외부 접근용 딕셔너리.

    // 액션 처럼 사용할 효과들 List
    List<EventEffects> untillNextCombat = new List<EventEffects>(); // 다음 전투까지 지속되는 효과 리스트
    List<EventEffects> untillNextStage = new List<EventEffects>(); // 다음 스테이지까지 지속되는 효과 리스트
    List<EventEffects> untillEndAdventure = new List<EventEffects>(); // 모험이 끝날 때까지 지속되는 효과 리스트

    protected override void Awake()
    {
        base.Awake();
        // CSV 파일에서 데이터 로드
        eventEffectList = LoadDatas(csvPath);
        eventEffectDict = eventEffectList.ToDictionary(effect => effect.index);
    }

    private List<EventEffects> LoadDatas(string csvPath)
    {
        string fullPath = $"{Application.dataPath}/Resources/{csvPath}";

        var eventEffectList = new List<EventEffects>();
        var eventEffectDatas = EventEffectCSVParser.Parse(fullPath);

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

                case 1:
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
                        cost = data.cost
                    };

                    eventEffectList.Add(cardEventEffect);
                    break;

                case 2:
                    var enemyEventEffect = new EncounterEventEffects
                    {
                        index = data.index,
                        text = data.text,
                        eventType = data.eventType,
                        duration = data.duration,
                    };

                    eventEffectList.Add(enemyEventEffect);
                    break;


                // 추후 다른 이벤트 타입에 대한 처리 추가
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
        Debug.Log("Play Next Combat");
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

        ProgressDataManager.Instance.UpdateEventEffectsData(untillNextCombat,untillNextStage,untillEndAdventure); // 현재 적용중인 효과 리스트들 저장 및 관리.
    }

    public void LoadEventEffectsData(List<EventEffects> com, List<EventEffects> stage, List<EventEffects> adv)
    {
        untillNextCombat = com;
        untillNextStage = stage;
        untillEndAdventure = adv;
    }
    public string GetEventEffectText(int index)
    {
        return eventEffectList[index].text;
    }
}
