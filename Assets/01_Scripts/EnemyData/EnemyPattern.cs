using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Enemy의 턴마다 스킬 또는 공격을 실행하는 패턴 제어 클래스
/// EnemyData에 저장된 스킬 정보와 EnemyAct를 기반으로 실행
/// </summary>
public static class EnemyPattern
{

    // StanceType 개수 (한 번만 계산)
    private static readonly int stanceCount =
        System.Enum.GetValues(typeof(PlayerData.StancType)).Length;
    
    
    /// <summary>
    /// 외부에서 호출되는 메인 메서드 - 적이 턴에 행동을 수행함
    /// </summary>
    public static IEnumerator ExecutePattern(IStatusReceiver enemy)
    {
        // 받은 친구가 Enemy 타입인지 확인
        if (enemy is not Enemy enemyComponent)
        {
            Debug.LogError("[EnemyPattern] 전달된 IStatusReceiver는 Enemy가 아닙니다.");
            yield break;
        }
        // 1. 자세 변경
        SetRandomStance(enemyComponent);
        yield return new WaitForSeconds(0.3f); // 자세 변경 연출 대기

        // 2. 사용할 스킬 선택
        var skill = ChooseSkill(enemyComponent);

        if (skill == null)
        {
            Debug.LogWarning($"[EnemyPattern] {enemyComponent.enemyData.EnemyName}의 스킬 데이터 없음.");
            yield break;
        }

        // 3. 스킬 데이터 가져오기 
        var actData = EnemySkillDatabase.Instance.Get(skill.skillIndex);
        if (actData == null)
        {
            Debug.LogWarning($"[EnemyPattern] 스킬 {skill.skillIndex}에 대한 act 데이터가 없습니다.");
            yield break;
        }

        // 4. 타겟 선택
        var targets = ChooseTargetsFromActData(actData, enemyComponent);
        yield return new WaitForSeconds(0.3f);

        // 5. 타겟에게 데미지 및 추가 효과 적용

        enemyComponent.PlayAttackAnimation();
        yield return new WaitForSeconds(0.3f);

        foreach (var t in targets)
        {
            // 1. 피격 애니메이션 재생
            t.PlayHitAnimation();
            yield return new WaitForSeconds(0.2f);

            string effectname = enemyComponent.enemyData.skillEffect;
            // 2. 스킬 이펙트 재생
            if (!string.IsNullOrEmpty(effectname))
            {
                Vector3 spawnPos = (t != null) ? t.CachedTransform.position : t.CachedTransform.position;
                GameManager.Instance.turnController.battleFlow.effectManage.PlayEffect(effectname, spawnPos, true);
            }
            // 3. 피격 데미지 적용
            t.TakeDamage(skill.damage);
            ApplyStatusEffect(t, actData);

            Debug.Log($"[EnemyPattern] {enemyComponent.enemyData.EnemyName} → {t.ChClass}에게 스킬 {skill.skillIndex} 사용 (데미지 {skill.damage})");

            yield return new WaitForSeconds(0.3f); // 타격 연출용 대기
        }
    }


    /// <summary>
    /// 스킬 데이터에 따라서 상대 및 아군 효과 대상을 선택
    /// </summary>
    /// <param name="actData"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    private static List<IStatusReceiver> ChooseTargetsFromActData(EnemyAct actData, Enemy self)
    {
        var targets = new List<IStatusReceiver>();
        var candidates = new List<IStatusReceiver>();

        // 1. 타겟 그룹 설정 (적 기준: Ally → 플레이어, Enemy → 적 자신 포함)
        List<IStatusReceiver> targetGroup = actData.targetType == TargetType.Ally
            ? GameManager.Instance.turnController.battleFlow.playerParty
            : GameManager.Instance.turnController.battleFlow.enemyParty;


        if (actData.targetType == TargetType.Ally)       // 적 기준 적 → 플레이어 파티 공격
        {
            // 플레이어 중에서 위치 조건에 맞는 대상만 후보로 추가
            foreach (var target in targetGroup)
            {
                if (!target.IsAlive()) continue;

                if ((target.ChClass == CharacterClass.Leon && actData.target_front) ||
                (target.ChClass == CharacterClass.Sophia && actData.target_center) ||
                (target.ChClass == CharacterClass.Kayla && actData.target_back))
                {
                    candidates.Add(target);
                }
            }
        }
        else
        {
            // 아군일 경우 enemyParty 순서대로 (0: front, 1: center, 2: back)
            for (int i = 0; i < 3; i++)
            {
                if ((i == 0 && actData.target_front)||
                    (i == 1 && actData.target_center)||
                    (i == 2 && actData.target_back))
                {
                    if (targetGroup[i].IsAlive())
                        candidates.Add(targetGroup[i]);
                }
            }
        }

        // 랜덤으로 targetNum만큼 선택하여 최종 적을 지정
        int count = Mathf.Min(actData.targetNum, candidates.Count);
        while (targets.Count < count)
        {
            var chosen = candidates[Random.Range(0, candidates.Count)];
            if (!targets.Contains(chosen))
                targets.Add(chosen);
        }

        return targets;
    }
    private static void SetRandomStance(Enemy enemy) //enemy 자세 변경
    {
        // 1) 파싱된 확률 데이터 사용
        if (!EnemyParseManager.ParsedDict.TryGetValue(enemy.enemyData.IDNum, out var parsed))
        {
            // 파싱 정보가 없으면 기존 균등 랜덤으로 대체 = 방어코드
            var fallback = (PlayerData.StancType)Random.Range(0, stanceCount);
            enemy.enemyData.currentStance = (EnemyData.StancValue.EStancType)fallback;
            Debug.LogWarning($"[EnemyPattern] ID {enemy.enemyData.IDNum} 파싱 데이터 없음 → 균등 랜덤 Stance={fallback}");
            return; // “High, Middle, Low 세 가지를 똑같은 확률(1/3씩) = 파싱 정보가 없을경우
        }

        // 2) 확률값 읽어오기
        float topP = parsed.topPercentage;      // High 확률
        float midP = parsed.middlePercentage;   // Middle 확률
        float botP = parsed.bottomPercentage;   // Low 확률

        // 3) 랜덤 값으로 분포 적용
        float r = Random.value;  // 0.0 ~ 1.0
        if (r < topP)
        {
            enemy.enemyData.currentStance = EnemyData.StancValue.EStancType.High;
            Debug.Log($"[EnemyPattern] {enemy.enemyData.EnemyName} 자세 → High (r={r:F2})");
        }
        else if (r < topP + midP)
        {
            enemy.enemyData.currentStance = EnemyData.StancValue.EStancType.Middle;
            Debug.Log($"[EnemyPattern] {enemy.enemyData.EnemyName} 자세 → Middle (r={r:F2})");
        }
        else
        {
            enemy.enemyData.currentStance = EnemyData.StancValue.EStancType.Low;
            Debug.Log($"[EnemyPattern] {enemy.enemyData.EnemyName} 자세 → Low (r={r:F2})");
        }
    }


    private static EnemySkill ChooseSkill(Enemy enemy)
    {
        var skills = enemy.enemyData.SkillList;
        if (skills == null || skills.Count == 0)
            return null;        // 스킬 없으면 기본 공격

        float total = 0;                 
        foreach (var s in skills) // 스킬 공격 확률에 따라 스킬 선택
            total += s.percentage;

        float rand = Random.Range(0f, 1);
        float cumulative = 0;

        foreach (var skill in skills)
        {
            cumulative += skill.percentage;
            if (rand <= cumulative)
                return skill;
        }

        return null;
    }

    /// <summary>
    /// 몬스터 스킬 사용 시 추가 효과 적용
    /// </summary>
    /// <param name="target">타겟</param>
    /// <param name="act">스킬 데이터</param>
    private static void ApplyStatusEffect(IStatusReceiver target, EnemyAct act)
    {
        if(act.atk_buff != 0)
        {
            target.ApplyStatusEffect(new StatusEffect
            {
                statType = BuffStatType.Attack,
                value = act.atk_buff,
                duration = act.buff_time
            });
            Debug.Log($"[EnemyPattern] {target.ChClass} 추가 공격력 {act.atk_buff} 효과 적용");
        }

        if (act.def_buff != 0)
        {
            target.ApplyStatusEffect(new StatusEffect
            {
                statType = BuffStatType.Defense,
                value = act.def_buff,
                duration = act.buff_time
            });
            Debug.Log($"[EnemyPattern] {target.ChClass} 추가 방어력 {act.def_buff} 효과 적용");
        }

        if (act.block)
        {
            //target.hasBlock = true;
            Debug.Log($"[EnemyPattern] {target.ChClass} 블록 효과 적용");
        }

        if (act.stun > 0)
        {
            target.ApplyStatusEffect(new StatusEffect
            {
                statType = BuffStatType.stun,
                value = -999,
                duration = act.buff_time
            }); 
            Debug.Log($"[EnemyPattern] {target.ChClass} 스턴 적용 ({act.stun}턴)");
        }
    }
}
