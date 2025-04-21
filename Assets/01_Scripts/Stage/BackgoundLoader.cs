using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BackgoundLoader
{
    private const string BackgroundPath = "BackGround";


    public static Dictionary<int, Sprite> LoadBackgrounds()
    {
        var sprites = Resources.LoadAll<Sprite>(BackgroundPath);
        var dic = new Dictionary<int, Sprite>();

        foreach (var sprite in sprites)
        {
            string[] parts = sprite.name.Split('_');
            if (parts.Length >  1 && int.TryParse(parts[1], out int stage))
            {
                dic[stage] = sprite;
            }
        }

        //Debug.Log($"총 {dic.Count}개의 배경 이미지 로드 완료");

        return dic;
    }
}
