#if UNITY_EDITOR


using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
            if (pngFiles.Length == 0) continue;

            List<Sprite> sprites = new();

            foreach (var path in pngFiles)
            {
                string assetPath = path.Replace(Application.dataPath, "Assets");

                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.filterMode = FilterMode.Point;
                    importer.spritePixelsPerUnit = 100;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                    sprites.Add(sprite);
            }

            if (sprites.Count == 0) continue;

            sprites = sprites.OrderBy(s => s.name).ToList();

            // EffectAnimation 생성
            var animAsset = ScriptableObject.CreateInstance<EffectAnimation>();
            animAsset.animationName = animName;
            animAsset.frames = sprites;
            animAsset.animationType = AnimationType.OnTarget; // 기본값, 나중에 변경 가능
            string savePath = $"{animationSavePath}{animName}.asset";
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
}
#endif