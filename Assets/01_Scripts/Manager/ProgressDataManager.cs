using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressDataManager : MonoSingleton<ProgressDataManager>
{
    List<EventEffects> untillNextCombat = new List<EventEffects>(); // 다음 전투까지 지속되는 효과 리스트
    List<EventEffects> untillNextStage = new List<EventEffects>(); // 다음 스테이지까지 지속되는 효과 리스트
    List<EventEffects> untillEndAdventure = new List<EventEffects>(); // 모험이 끝날 때까지 지속되는 효과 리스트

    protected override void Awake()
    {
        base.Awake();
    }
    
    public void UpdateEventEffectsData(List<EventEffects> com, List<EventEffects> stage, List<EventEffects> adv)
    {
        untillNextCombat = com;
        untillNextStage = stage;
        untillEndAdventure = adv;
    }

    // 추후 세이브 로드 추가.
}
