using UnityEngine;
using System.Collections.Generic;

public class SpriteLoader : MonoBehaviour
{
    [Header("설정")]
    public string resourcePath = "CardSkillEffects";
    private Dictionary<string, List<Sprite>> loadedEffects = new();

    private void Awake()
    {
        LoadAllEffects();
    }

    private void LoadAllEffects()
    {
        TextAsset folderListAsset = Resources.Load<TextAsset>($"{resourcePath}/SkillEffectFolders");

        if (folderListAsset == null)
        {
            Debug.LogError("[SpriteLoader] SkillEffectFolders.txt 파일을 찾지 못했습니다!");
            return;
        }

        string[] folderNames = folderListAsset.text.Split('\n');

        foreach (var folderNameRaw in folderNames)
        {
            string folderName = folderNameRaw.Trim();

            if (string.IsNullOrEmpty(folderName)) continue;

            var sprites = Resources.LoadAll<Sprite>($"{resourcePath}/{folderName}");

            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[SpriteLoader] {folderName} 폴더에서 스프라이트를 찾지 못했습니다.");
                continue;
            }

            loadedEffects[folderName] = new List<Sprite>(sprites);
            Debug.Log($"[SpriteLoader] {folderName} 폴더에 {sprites.Length}개 스프라이트 로드 완료!");
        }

        Debug.Log($"[SpriteLoader] 총 {loadedEffects.Count}개 이펙트 그룹 로드 완료!");
    }

    public List<Sprite> GetEffectFrames(string effectName)
    {
        loadedEffects.TryGetValue(effectName, out var frames);
        return frames;
    }
}
