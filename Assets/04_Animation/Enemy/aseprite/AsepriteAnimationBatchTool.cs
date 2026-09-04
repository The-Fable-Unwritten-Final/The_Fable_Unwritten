#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public static class AsepriteAnimationBatchTool
{
    // ================================
    // 설정
    // ================================

    // .aseprite들이 들어있는 최상위 폴더
    private const string SourceFolder =
        "Assets/04_Animation/Enemy/aseprite";

    // 생성된 .anim 저장 폴더
    private const string OutputFolder =
        "Assets/04_Animation/Enemy/GeneratedClips";

    // 1프레임 애니메이션 길이
    private const float SingleFrameDuration = 0.1f;


    // ================================
    // 실행
    // ================================

    [MenuItem("Tools/Enemy/Generate Aseprite Clips")]
    public static void GenerateAll()
    {
        EnsureFolder(OutputFolder);

        string[] guids =
            AssetDatabase.FindAssets("", new[] { SourceFolder });

        int copiedCount = 0;
        int singleFrameCount = 0;
        int failedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath =
                AssetDatabase.GUIDToAssetPath(guid);

            if (!assetPath.EndsWith(".aseprite"))
                continue;

            string fileName =
                Path.GetFileNameWithoutExtension(assetPath);

            Object[] subAssets =
                AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // --------------------------------
            // 1. AnimationClip 찾기
            // --------------------------------

            AnimationClip sourceClip = subAssets
                .OfType<AnimationClip>()
                .FirstOrDefault(IsUsableClip);

            AnimationClip resultClip;

            if (sourceClip != null)
            {
                // 기존 Aseprite 내부 Clip 복사
                resultClip = Object.Instantiate(sourceClip);

                Debug.Log(
                    $"[AsepriteBatch] Clip 복사 : {fileName}"
                );

                copiedCount++;
            }
            else
            {
                // --------------------------------
                // 2. Clip이 없으면 Sprite 찾기
                // --------------------------------

                Sprite sprite = subAssets
                    .OfType<Sprite>()
                    .FirstOrDefault();

                if (sprite == null)
                {
                    Debug.LogWarning(
                        $"[AsepriteBatch] " +
                        $"{assetPath}에서 Clip과 Sprite를 모두 찾지 못했습니다."
                    );

                    failedCount++;
                    continue;
                }

                // 1프레임 AnimationClip 생성
                resultClip =
                    CreateSingleFrameClip(sprite);

                Debug.Log(
                    $"[AsepriteBatch] 1 Frame Clip 생성 : {fileName}"
                );

                singleFrameCount++;
            }

            // --------------------------------
            // 3. 이름
            // --------------------------------

            resultClip.name = fileName;

            // --------------------------------
            // 4. Loop 설정
            // --------------------------------

            bool shouldLoop = ShouldLoop(fileName);

            SetLoopTime(resultClip, shouldLoop);

            // --------------------------------
            // 5. 저장
            // --------------------------------

            string outputPath =
                $"{OutputFolder}/{fileName}.anim";

            SaveOrReplaceClip(
                resultClip,
                outputPath
            );

            Debug.Log(
                $"[AsepriteBatch] 생성 완료 : " +
                $"{fileName} / Loop = {shouldLoop}"
            );
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[AsepriteBatch] 전체 완료\n" +
            $"기존 Clip 복사 : {copiedCount}\n" +
            $"1 Frame 생성 : {singleFrameCount}\n" +
            $"실패 : {failedCount}"
        );
    }


    // ================================
    // 사용할 Clip인지 확인
    // ================================

    private static bool IsUsableClip(AnimationClip clip)
    {
        if (clip == null)
            return false;

        // Preview 등 Unity 내부용 Clip 제외
        if (clip.name.StartsWith("__preview__"))
            return false;

        return true;
    }


    // ================================
    // Loop 규칙
    // ================================

    private static bool ShouldLoop(string fileName)
    {
        string lower =
            fileName.ToLowerInvariant();

        // Idle만 Loop
        return lower.Contains("idle");
    }


    // ================================
    // 1프레임 Clip 생성
    // ================================

    private static AnimationClip CreateSingleFrameClip(
        Sprite sprite)
    {
        AnimationClip clip =
            new AnimationClip();

        // 10 FPS
        clip.frameRate =
            1f / SingleFrameDuration;

        EditorCurveBinding binding =
            new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

        ObjectReferenceKeyframe[] keyFrames =
        {
            new ObjectReferenceKeyframe
            {
                time = 0f,
                value = sprite
            },

            // 애니메이션 길이를 확보하기 위한 두 번째 Key
            new ObjectReferenceKeyframe
            {
                time = SingleFrameDuration,
                value = sprite
            }
        };

        AnimationUtility.SetObjectReferenceCurve(
            clip,
            binding,
            keyFrames
        );

        return clip;
    }


    // ================================
    // Loop 설정
    // ================================

    private static void SetLoopTime(
        AnimationClip clip,
        bool loop)
    {
        SerializedObject serializedClip =
            new SerializedObject(clip);

        SerializedProperty settings =
            serializedClip.FindProperty(
                "m_AnimationClipSettings"
            );

        if (settings == null)
        {
            Debug.LogWarning(
                $"[AsepriteBatch] " +
                $"{clip.name}: AnimationClipSettings를 찾지 못했습니다."
            );

            return;
        }

        SerializedProperty loopTime =
            settings.FindPropertyRelative(
                "m_LoopTime"
            );

        if (loopTime != null)
            loopTime.boolValue = loop;

        SerializedProperty loopBlend =
            settings.FindPropertyRelative(
                "m_LoopBlend"
            );

        if (loopBlend != null)
            loopBlend.boolValue = false;

        serializedClip.ApplyModifiedProperties();
    }


    // ================================
    // 저장
    // ================================

    private static void SaveOrReplaceClip(
        AnimationClip newClip,
        string path)
    {
        AnimationClip existing =
            AssetDatabase.LoadAssetAtPath<AnimationClip>(
                path
            );

        if (existing != null)
        {
            // GUID 유지
            EditorUtility.CopySerialized(
                newClip,
                existing
            );

            EditorUtility.SetDirty(existing);

            // Instantiate로 만들어진 임시 객체 제거
            Object.DestroyImmediate(newClip);
        }
        else
        {
            AssetDatabase.CreateAsset(
                newClip,
                path
            );
        }
    }


    // ================================
    // 폴더 자동 생성
    // ================================

    private static void EnsureFolder(
        string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent =
            Path.GetDirectoryName(folderPath)
                ?.Replace("\\", "/");

        string folderName =
            Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(
            parent,
            folderName
        );
    }
}

#endif