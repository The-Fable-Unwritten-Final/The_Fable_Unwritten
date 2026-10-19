#if UNITY_EDITOR


using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EffectAnimationGenerator
{
    private const string animationRoot = "Assets/03_Datas/AnimationSprite";
    private const string animationSavePath = "Assets/05._ScriptableObjects/AnimationDatabase/";
    private const string databasePath = "Assets/Resources/EffectAnimationDatabase.asset";

    [MenuItem("Tools/Generate EffectAnimations + Database")]
    public static void Generate()
    {
        if (!Directory.Exists(animationSavePath))        //경로에 폴더 없으면 생성
            Directory.CreateDirectory(animationSavePath);

        var db = ScriptableObject.CreateInstance<EffectAnimationDatabase>();
        db.allAnimations = new List<EffectAnimation>();


        // animationRoot 안의 모든 하위 폴더가 애니메이션 폴더
        var animationFolders = Directory.GetDirectories(animationRoot, "*", SearchOption.TopDirectoryOnly);

        foreach (var animFolder in animationFolders)
        {
            string animName = Path.GetFileName(animFolder);
            string[] pngFiles = Directory.GetFiles(animFolder, "*.png", SearchOption.TopDirectoryOnly);

            if (pngFiles.Length == 0)
                continue;

            List<Sprite> sprites = new();

            foreach (var filePath in pngFiles)
            {
                string assetPath = filePath.Replace("\\", "/");

                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;

                // 공통 설정
                bool changed = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }

                if (importer.spritePixelsPerUnit != 100)
                {
                    importer.spritePixelsPerUnit = 100;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    changed = true;
                }

                if (changed)
                    importer.SaveAndReimport();

                // 여기서 Single / Multiple 구분 처리
                if (importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                    foreach (var asset in assets)
                    {
                        if (asset is Sprite sprite)
                            sprites.Add(sprite);
                    }
                }
                else
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                        sprites.Add(sprite);
                }
            }

            if (sprites.Count == 0)
                continue;

            // 이름 기준 정렬
            sprites = sprites
                .OrderBy(s => GetTrailingNumber(s.name))
                .ThenBy(s => s.name, System.StringComparer.Ordinal)
                .ToList();

            string savePath = $"{animationSavePath}{animName}.asset".Replace("\\", "/");

            // 기존 애니메이션 에셋이 있으면 삭제
            var oldAnim = AssetDatabase.LoadAssetAtPath<EffectAnimation>(savePath);
            if (oldAnim != null)
                AssetDatabase.DeleteAsset(savePath);

            var animAsset = ScriptableObject.CreateInstance<EffectAnimation>();
            animAsset.animationName = animName;
            animAsset.frames = sprites;
            animAsset.animationType = AnimationType.OnTarget;

            AssetDatabase.CreateAsset(animAsset, savePath);
            db.allAnimations.Add(animAsset);

            Debug.Log($"[EffectAnimation] 생성 완료: {animName} ({sprites.Count} frames)");
        }

        if (!Directory.Exists("Assets/Resources"))
            Directory.CreateDirectory("Assets/Resources");

        AssetDatabase.CreateAsset(db, databasePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[완료] EffectAnimationDatabase 생성: 총 {db.allAnimations.Count}개 등록됨");
    }
    private static int GetTrailingNumber(string name)
    {
        var match = Regex.Match(name, @"(\d+)$");
        if (match.Success && int.TryParse(match.Value, out int number))
            return number;

        return int.MaxValue;
    }
}
#endif