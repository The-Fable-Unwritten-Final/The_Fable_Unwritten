using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventInitTest : MonoSingleton<EventInitTest>
{
    [Header("테스트용 스테이지 인덱스")]
    [SerializeField] private int testStageIndex = 2;

    [Header("로드된 이벤트 데이터")]
    public List<RandomEventData> allEvents;

    [Header("선택된 이벤트 (현재 스테이지 기준)")]
    public RandomEventData selectedEvent;

    protected override void Awake()
    {
        base.Awake();
        allEvents = RandomEventJsonLoader.LoadAllEvents();

        var stageEvents = allEvents.Where(e => e.stage == testStageIndex).ToList();
        selectedEvent = stageEvents.Count > 0
            ? stageEvents[Random.Range(0, stageEvents.Count)]
            : null;
    }
}
