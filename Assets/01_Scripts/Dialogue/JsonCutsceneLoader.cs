using HisaGames.Cutscene;
using System.Collections.Generic;
using UnityEngine;

public class JsonCutsceneLoader
{
    public static List<JsonCutsceneData> ParseFromJSON(TextAsset json)
    {
        return JsonUtilityWrapper.FromJsonList<JsonCutsceneData>(json.text);
    }

    public static EcCutscene.CutsceneData[] Convert(List<JsonCutsceneData> list)
    {
        var result = new List<EcCutscene.CutsceneData>();

        foreach (var item in list)
        {
            var converted = new EcCutscene.CutsceneData
            {
                name = item.id,
                nameString = item.nameString,
                chatString = item.chatString,
                charactersData = item.charactersData.ConvertAll(c => new EcCutscene.CharacterData
                {
                    name = c.name,
                    initialTransformID = c.initialTransformID,
                    finalTransformID = c.finalTransformID,
                    spriteString = c.spriteString
                }).ToArray(),
                propsData = item.propsData.ConvertAll(p => new EcCutscene.PropsData
                {
                    name = p.name,
                    position = p.position,
                    fadeSpeed = p.fadeSpeed
                }).ToArray(),
                cutscenePreEvent = new EcCutscene.CSUnityEvent(),
                cutscenePostEvent = new EcCutscene.CSUnityEvent()
            };

            // 연출 타입별 분기 이벤트 등록
            switch (item.type)
            {
                case "animation":
                    converted.cutscenePreEvent.AddListener(() =>
                    {
                        Debug.Log($"[Cutscene] 애니메이션 재생: {item.animationName}");
                        // 여기에 애니메이션 호출 코드 삽입
                        // e.g., AnimationManager.Instance.Play(item.animationName);
                    });
                    break;

                case "blackout":
                    converted.cutscenePreEvent.AddListener(() =>
                    {
                        Debug.Log("[Cutscene] 화면 암전");
                        // 예: UIManager.Instance.FadeToBlack();
                    });
                    break;

                case "center":
                    converted.cutscenePreEvent.AddListener(() =>
                    {
                        Debug.Log($"[Cutscene] 중앙 텍스트 연출: {item.chatString}");
                        // 예: CenterTextEffect.Show(item.chatString);
                    });
                    break;
            }

            // 사운드 이펙트 처리
            if (!string.IsNullOrEmpty(item.sfx))
            {
                converted.cutscenePreEvent.AddListener(() =>
                {
                    Debug.Log($"[Cutscene] 효과음 재생: {item.sfx}");
                    // 예: SoundManager.Instance.PlaySFX(item.sfx);
                });
            }

            // 배경 전환 처리 (선택)
            if (!string.IsNullOrEmpty(item.bgName))
            {
                converted.cutscenePreEvent.AddListener(() =>
                {
                    Debug.Log($"[Cutscene] 배경 전환: {item.bgName}");
                    // 예: BackgroundManager.Instance.ChangeBackground(item.bgName);
                });
            }

            result.Add(converted);
        }

        return result.ToArray();
    }
}
