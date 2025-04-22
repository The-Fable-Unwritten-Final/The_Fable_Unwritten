using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventEffectManager : MonoSingleton<EventEffectManager>
{
    [Header("CSV Data path")]
    [SerializeField] string csvPath = "ExternalFiles/EventEffects.csv"; // CSV 파일 경로
    [Header("Event effect Lists")]
    [SerializeField] List<EventEffects> eventEffectList;// 이벤트 효과 SO 리스트.

    // 액션 처럼 사용할 효과들 List
    [SerializeField] List<EventEffects> untillNextCombat; // 다음 전투까지 지속되는 효과 리스트
    [SerializeField] List<EventEffects> untillNextStage; // 다음 스테이지까지 지속되는 효과 리스트
    [SerializeField] List<EventEffects> untillEndAdventure; // 모험이 끝날 때까지 지속되는 효과 리스트

    protected override void Awake()
    {
        base.Awake();
        // CSV 파일에서 데이터 로드
        eventEffectList = LoadDatas(csvPath);
    }
    private void Update()
    {
        // 테스트용
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddEventEffect(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddEventEffect(2);
        }
    }

    private List<EventEffects> LoadDatas(string csvPath) // CSV의 이벤트 효과 데이터를 기반으로 이벤트 효과 SO의 리스트를 생성.
    {
        string fullPath = $"{Application.dataPath}/Resources/{csvPath}";

        var eventEffectList = new List<EventEffects>();
        var eventEffectDatas = EventEffectCSVParser.Parse(fullPath);

        foreach (var data in eventEffectDatas)
        {
            // 이벤트 타입에 따른 다른 종류의 SO 생성
            switch (data.eventType)
            {
                case 0:
                    var statEffect = ScriptableObject.CreateInstance<StatEventEffects>();

                    statEffect.index = data.index;
                    statEffect.text = data.text;
                    statEffect.eventType = data.eventType;
                    statEffect.duration = data.duration;
                    statEffect.sophia = data.sophia;
                    statEffect.kyla = data.kyla;
                    statEffect.leon = data.leon;
                    statEffect.enemy = data.enemy;
                    statEffect.hp = data.hp;
                    statEffect.hpPercent = data.hpPercent;
                    statEffect.atk = data.atk;
                    statEffect.def = data.def;

                    eventEffectList.Add(statEffect);
                    break;

                case 1:
                    var cardEventEffect = ScriptableObject.CreateInstance<CardEventEffects>();
                    cardEventEffect.index = data.index;
                    cardEventEffect.text = data.text;
                    cardEventEffect.eventType = data.eventType;
                    cardEventEffect.duration = data.duration;
                    cardEventEffect.sophia = data.sophia;
                    cardEventEffect.kyla = data.kyla;
                    cardEventEffect.leon = data.leon;
                    cardEventEffect.cardType = data.cardType;
                    cardEventEffect.cost = data.cost;
                    cardEventEffect.newCardIndex = data.newCardIndex;
                    cardEventEffect.unusable = data.unusable;

                    eventEffectList.Add(cardEventEffect);
                    break;

                case 2:
                    var enemyEventEffect = ScriptableObject.CreateInstance<EncounterEventEffects>();

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
    }
    /// <summary>
    /// EventEffects 중 즉시 효과를 적용하는 경우 호출.
    /// </summary>
    public void InstantEffect(int index)
    {
        eventEffectList[index].Apply();
    }

    // List에 효과를 추가하는 메서드
    public void AddEventEffect(int index)
    {
        EventEffects effect = eventEffectList[index].Clone();
        switch (effect.duration)
        {
            case 0: // 즉시 사용 효과 적용, 왠만하면 InstantEffect(int index) 로 호출 할 것.
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
    }
}
