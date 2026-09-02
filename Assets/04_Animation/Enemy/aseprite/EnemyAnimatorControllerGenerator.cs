#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class EnemyAnimatorControllerGenerator
{
    private const string ClipFolder =
        "Assets/04_Animation/Enemy/GeneratedClips";

    private const string ControllerFolder =
        "Assets/04_Animation/Enemy/GeneratedControllers";

    private const string PARAM_HIT = "Hit";
    private const string PARAM_ATTACK = "Attack";
    private const string PARAM_DEATH = "Death";


    [MenuItem("Tools/Enemy/Generate Animator Controllers")]
    public static void GenerateControllers()
    {
        EnsureFolder(ControllerFolder);

        string[] guids = AssetDatabase.FindAssets(
            "t:AnimationClip",
            new[] { ClipFolder }
        );

        List<AnimationClip> clips = guids
            .Select(guid =>
                AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    AssetDatabase.GUIDToAssetPath(guid)))
            .Where(clip => clip != null)
            .ToList();

        // =========================================
        // *_idle.anim을 기준으로 적 이름 추출
        // =========================================

        var idleClips = clips
            .Where(c =>
                c.name.EndsWith("_idle",
                    System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        int generatedCount = 0;

        foreach (AnimationClip idleClip in idleClips)
        {
            string enemyName =
                idleClip.name.Substring(
                    0,
                    idleClip.name.Length - "_idle".Length
                );

            GenerateController(
                enemyName,
                idleClip,
                clips
            );

            generatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[EnemyAnimatorGenerator] 완료 : " +
            $"{generatedCount}개 Controller 생성"
        );
    }


    private static void GenerateController(
        string enemyName,
        AnimationClip idleClip,
        List<AnimationClip> allClips)
    {
        // =========================================
        // 필요한 Clip 검색
        // =========================================

        AnimationClip hitClip =
            FindClip(allClips, $"{enemyName}_hit");

        AnimationClip deathClip =
            FindClip(allClips, $"{enemyName}_death");

        List<AnimationClip> attackClips =
            new List<AnimationClip>();

        // 최대 Attack5까지
        for (int i = 1; i <= 5; i++)
        {
            AnimationClip attack =
                FindClip(
                    allClips,
                    $"{enemyName}_attack{i}"
                );

            if (attack != null)
                attackClips.Add(attack);
        }


        // =========================================
        // Controller 생성
        // =========================================

        string controllerPath =
            $"{ControllerFolder}/{enemyName}.controller";

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(
                controllerPath
            );

        if (controller == null)
        {
            controller =
                AnimatorController.CreateAnimatorControllerAtPath(
                    controllerPath
                );
        }

        SetupController(
            controller,
            enemyName,
            idleClip,
            hitClip,
            deathClip,
            attackClips
        );

        Debug.Log(
            $"[EnemyAnimatorGenerator] {enemyName} 생성 완료 " +
            $"Attack Count = {attackClips.Count}"
        );
    }


    private static void SetupController(
        AnimatorController controller,
        string enemyName,
        AnimationClip idleClip,
        AnimationClip hitClip,
        AnimationClip deathClip,
        List<AnimationClip> attackClips)
    {
        // =========================================
        // Parameter 초기화
        // =========================================

        controller.parameters =
            System.Array.Empty<AnimatorControllerParameter>();

        controller.AddParameter(
            PARAM_HIT,
            AnimatorControllerParameterType.Bool
        );

        controller.AddParameter(
            PARAM_ATTACK,
            AnimatorControllerParameterType.Int
        );

        controller.AddParameter(
            PARAM_DEATH,
            AnimatorControllerParameterType.Bool
        );

        // Attack 기본값 -1
        AnimatorControllerParameter attackParam =
            controller.parameters
                .First(p => p.name == PARAM_ATTACK);

        attackParam.defaultInt = -1;

        // parameters 배열은 복사본 성격이 있으므로 다시 설정
        AnimatorControllerParameter[] parameters =
            controller.parameters;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == PARAM_ATTACK)
                parameters[i].defaultInt = -1;
        }

        controller.parameters = parameters;


        // =========================================
        // StateMachine 가져오기
        // =========================================

        AnimatorStateMachine stateMachine =
            controller.layers[0].stateMachine;

        // 기존 State 제거
        AnimatorState[] oldStates =
            stateMachine.states
                .Select(s => s.state)
                .ToArray();

        foreach (AnimatorState state in oldStates)
        {
            stateMachine.RemoveState(state);
        }

        // 기존 AnyState Transition 제거
        AnimatorStateTransition[] oldAnyTransitions =
            stateMachine.anyStateTransitions.ToArray();

        foreach (AnimatorStateTransition transition
                 in oldAnyTransitions)
        {
            stateMachine.RemoveAnyStateTransition(
                transition
            );
        }


        // =========================================
        // Idle
        // =========================================

        AnimatorState idleState =
            stateMachine.AddState("Idle");

        idleState.motion = idleClip;

        stateMachine.defaultState = idleState;


        // =========================================
        // Hit
        // =========================================

        AnimatorState hitState = null;

        if (hitClip != null)
        {
            hitState =
                stateMachine.AddState("Hit");

            hitState.motion = hitClip;

            // Any State → Hit
            AnimatorStateTransition toHit =
                stateMachine.AddAnyStateTransition(
                    hitState
                );

            SetupTransition(toHit);

            toHit.canTransitionToSelf = false;

            toHit.AddCondition(
                AnimatorConditionMode.If,
                0,
                PARAM_HIT
            );

            // 죽었을 때 Hit로 가지 않게
            toHit.AddCondition(
                AnimatorConditionMode.IfNot,
                0,
                PARAM_DEATH
            );


            // Hit → Idle
            AnimatorStateTransition hitToIdle =
                hitState.AddTransition(
                    idleState
                );

            SetupTransition(hitToIdle);

            hitToIdle.AddCondition(
                AnimatorConditionMode.IfNot,
                0,
                PARAM_HIT
            );
        }


        // =========================================
        // Attack
        // =========================================

        for (int i = 0;
             i < attackClips.Count;
             i++)
        {
            AnimationClip attackClip =
                attackClips[i];

            // Clip은 attack1
            // Parameter 값은 0
            int attackValue = i;

            AnimatorState attackState =
                stateMachine.AddState(
                    $"Attack{i + 1}"
                );

            attackState.motion =
                attackClip;


            // Idle → Attack
            AnimatorStateTransition idleToAttack =
                idleState.AddTransition(
                    attackState
                );

            SetupTransition(
                idleToAttack
            );

            idleToAttack.AddCondition(
                AnimatorConditionMode.Equals,
                attackValue,
                PARAM_ATTACK
            );

            // Hit 중에는 Attack 진입 방지
            idleToAttack.AddCondition(
                AnimatorConditionMode.IfNot,
                0,
                PARAM_HIT
            );

            // Death 중에는 Attack 진입 방지
            idleToAttack.AddCondition(
                AnimatorConditionMode.IfNot,
                0,
                PARAM_DEATH
            );


            // Attack → Idle
            AnimatorStateTransition attackToIdle =
                attackState.AddTransition(
                    idleState
                );

            SetupTransition(
                attackToIdle
            );

            attackToIdle.AddCondition(
                AnimatorConditionMode.Equals,
                -1,
                PARAM_ATTACK
            );
        }


        // =========================================
        // Death
        // =========================================

        if (deathClip != null)
        {
            AnimatorState deathState =
                stateMachine.AddState(
                    "Death"
                );

            deathState.motion =
                deathClip;


            // Any State → Death
            AnimatorStateTransition toDeath =
                stateMachine.AddAnyStateTransition(
                    deathState
                );

            SetupTransition(toDeath);

            toDeath.canTransitionToSelf = false;

            toDeath.AddCondition(
                AnimatorConditionMode.If,
                0,
                PARAM_DEATH
            );

            // Death에서는 Transition 없음
        }


        EditorUtility.SetDirty(controller);
    }


    // =============================================
    // 모든 Transition 공통 설정
    // =============================================

    private static void SetupTransition(
        AnimatorStateTransition transition)
    {
        // 사용자가 정한 규칙
        transition.hasExitTime = false;

        // 즉시 전환
        transition.duration = 0f;

        transition.offset = 0f;

        transition.hasFixedDuration = true;
    }


    private static AnimationClip FindClip(
        List<AnimationClip> clips,
        string clipName)
    {
        return clips.FirstOrDefault(
            c => string.Equals(
                c.name,
                clipName,
                System.StringComparison.OrdinalIgnoreCase
            )
        );
    }


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