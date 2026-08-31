using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        System.Enum.GetValues(typeof(StancType)).Length;


    /// <summary>
    /// 외부에서 호출되는 메인 메서드 - 적이 턴에 행동을 수행함
    /// </summary>
    public static IEnumerator ExecutePattern(IStatusReceiver enemy)
    {
        if (enemy is not Enemy enemyComponent)
        {
            Debug.LogError("[EnemyPattern] 전달된 IStatusReceiver는 Enemy가 아닙니다.");
            yield break;
        }

        if (enemyComponent.IsStunned())
            yield break;

        var skill = enemyComponent.Mechanic?.GetForcedSkill() ?? ChooseSkill(enemyComponent);
        if (skill == null)
        {
            Debug.LogWarning($"[EnemyPattern] {enemyComponent.enemyData.EnemyName}의 스킬 데이터 없음.");
            yield break;
        }

        var actData = DataManager.Instance.EnemyActDict[skill.skillIndex];
        if (actData == null)
        {
            Debug.LogWarning($"[EnemyPattern] 스킬 {skill.skillIndex}에 대한 act 데이터가 없습니다.");
            yield break;
        }

        var targets = ChooseTargetsFromActData(actData, enemyComponent);
        if (targets == null || targets.Count == 0)
            yield break;

        yield return new WaitForSeconds(0.3f);

        var lowestSkill = enemyComponent.enemyData.SkillList
            .OrderBy(s => s.skillIndex)
            .FirstOrDefault();

        if (lowestSkill == null || lowestSkill.skillIndex == 0)
        {
            Debug.LogWarning("[EnemyPattern] lowestSkill 계산 실패");
            yield break;
        }

        int attackType = skill.skillIndex % lowestSkill.skillIndex;

        bool hitTriggered = false;

        enemyComponent.PlayAttackAnimation(attackType, () =>
        {
            if (hitTriggered)
                return;

            hitTriggered = true;

            foreach (var t in targets)
            {
                if (t == null || !t.IsAlive())
                    continue;

                string effectname = (t.ChClass == CharacterClass.Enemy)
                    ? enemyComponent.enemyData.AllySkillEffect
                    : enemyComponent.enemyData.AttackSkillEffect;

                // 이펙트가 없는 경우에도 데미지/상태이상은 들어가게
                if (string.IsNullOrEmpty(effectname))
                {
                    if (t.IsAlive())
                        t.PlayHitAnimation();

                    ApplyDamageAndStatus(enemyComponent, t, skill, actData);
                    continue;
                }

                if (t == enemy)
                    SoundManager.Instance.PlaySFX(SoundCategory.Enemy, 0);
                else
                    SoundManager.Instance.PlaySFX(SoundCategory.Enemy, enemyComponent.enemyData.IDNum);

                float scaleFactor = DetermineEffectScale(enemyComponent.enemyData.type);

                if (!DataManager.Instance.CardEffects.TryGetValue(effectname, out var animInfo) || animInfo == null)
                {
                    if (t.IsAlive())
                        t.PlayHitAnimation();

                    ApplyDamageAndStatus(enemyComponent, t, skill, actData);
                    continue;
                }

                if (animInfo.animationType == AnimationType.Projectile)
                {
                    GameManager.Instance.turnController.battleFlow.effectManage.PlayProjectileEffect(
                        effectname,
                        enemyComponent,
                        t,
                        scaleFactor,
                        () =>
                        {
                            if (t.IsAlive())
                                t.PlayHitAnimation();

                            ApplyDamageAndStatus(enemyComponent, t, skill, actData);
                        }
                    );
                }
                else
                {
                    GameManager.Instance.turnController.battleFlow.effectManage.PlayEffect(
                        effectname,
                        enemyComponent,
                        t,
                        true,
                        scaleFactor
                    );

                    if (t.IsAlive())
                        t.PlayHitAnimation();

                    ApplyDamageAndStatus(enemyComponent, t, skill, actData);
                }
            }
        });

        // 실제 히트 프레임까지 기다림
        yield return new WaitUntil(() => hitTriggered);

        // 투사체/피격 연출 마무리 대기
        yield return new WaitForSeconds(0.6f);

        foreach (var t in targets)
        {
            if (t == null)
                continue;

            if (!t.IsAlive() && t is MonoBehaviour mb && mb.gameObject.activeSelf)
            {
                t.TryFinalizeDeath();

                if (t is Enemy e)
                {
                    ProgressDataManager.Instance.CurrentExp += e.enemyData.exp;
                    GameManager.Instance.turnController.battleFlow.totalExp += e.enemyData.exp;
                }
            }
        }
    }

    private static float DetermineEffectScale(EnemyType type)
    {
        float baseScale = 1f;
        return type switch
        {
            EnemyType.normal => baseScale * 0.5f,
            EnemyType.elite => baseScale * 1f,
            EnemyType.boss => baseScale * 1.5f,
            _ => baseScale
        };
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
                if (target == null) continue;
                if (!target.IsAlive()) continue;
                if (target is PlayerController pc && pc.IsTemporarilyAbsent) continue;

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
            for (int i = 0; i < targetGroup.Count; i++)
            {
                if ((i == 0 && actData.target_front)||
                    (i == 1 && actData.target_center)||
                    (i == 2 && actData.target_back))
                {
                    if (targetGroup[i] == null) continue;

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

    private static void ApplyDamageAndStatus(Enemy enemyComponent, IStatusReceiver target, EnemySkill skill, EnemyAct actData)
    {
        if (target is PlayerController pc)
        {
            pc.TakeDamage(skill.damage);
        }
        else
        {
            target.TakeDamage(skill.damage);
        }

        ApplyStatusEffect(target, actData);
    }


    private static EnemySkill ChooseSkill(Enemy enemy)
    {
        var skills = enemy.enemyData.SkillList;
        if (skills == null || skills.Count == 0)
            return null;        // 스킬 없으면 기본 공격

        int currentStage = ProgressDataManager.Instance.StageIndex;
        float total = 0f;
        var validSkills = new List<EnemySkill>();

        //스킬 사용 조건 확인
        for (int i = 0; i < skills.Count; i++)
        {
            var skill = skills[i];
            
            bool isLocked = false;
            // 스테이지 조건 제한
            switch (skills.Count)
            {
                case 3:
                    isLocked = (i == 1 && currentStage < 3) || (i == 2 && currentStage < 4);
                    break;
                case 4:
                    isLocked = (i == 2 && currentStage < 3) || (i == 3 && currentStage < 4);
                    break;
                case 5:
                    isLocked = (i == 3 && currentStage < 3) || (i == 4 && currentStage < 4);
                    break;
            }

            if (isLocked)
                continue;

            var actData = DataManager.Instance.EnemyActDict.GetValueOrDefault(skill.skillIndex);
            if (actData == null)
                continue;

            validSkills.Add(skill);
            total += skill.percentage;
        }

        if (validSkills.Count == 0)
            return null;
        //스킬 뽑기
        float rand = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var skill in validSkills)
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
            target.ApplyStatusEffect(new TickEffect
            {
                statType = BuffStatType.Attack,
                value = act.atk_buff,
                duration = act.buff_time
            });
            //Debug.Log($"[EnemyPattern] {target.ChClass} 추가 공격력 {act.atk_buff} 효과 적용");
        }

        if (act.def_buff != 0)
        {
            target.ApplyStatusEffect(new TickEffect
            {
                statType = BuffStatType.Defense,
                value = act.def_buff,
                duration = act.buff_time
            });
            //Debug.Log($"[EnemyPattern] {target.ChClass} 추가 방어력 {act.def_buff} 효과 적용");
        }

        if (act.block)
        {
            //target.hasBlock = true;
            //Debug.Log($"[EnemyPattern] {target.ChClass} 블록 효과 적용");
        }

        if (act.stun > 0)
        {
            target.ApplyStatusEffect(new TickEffect
            {
                statType = BuffStatType.Stun,
                value = -999,
                duration = act.buff_time
            }); 
            //Debug.Log($"[EnemyPattern] {target.ChClass} 스턴 적용 ({act.stun}턴)");
        }
    }
}
