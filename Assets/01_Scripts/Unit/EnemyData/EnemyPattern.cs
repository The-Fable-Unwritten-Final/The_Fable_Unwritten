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
        {
            yield break;
        }

        // 1. 사용할 스킬 선택
        var skill = ChooseSkill(enemyComponent);
        if (skill == null)
        {
            Debug.LogWarning($"[EnemyPattern] {enemyComponent.enemyData.EnemyName}의 스킬 데이터 없음.");
            yield break;
        }

        // 2. 스킬 데이터 가져오기
        var actData = DataManager.Instance.EnemyActDict[skill.skillIndex];
        if (actData == null)
        {
            Debug.LogWarning($"[EnemyPattern] 스킬 {skill.skillIndex}에 대한 act 데이터가 없습니다.");
            yield break;
        }

        // 3. 타겟 선택 (수호 리다이렉트 적용)
        var targets = ResolveTargetsWithGuard(actData, enemyComponent);  // 여기 수정!
        yield return new WaitForSeconds(0.3f);

        var lowestSkill = enemyComponent.enemyData.SkillList
            .OrderBy(s => s.skillIndex)
            .FirstOrDefault();

        // 4. 공격 애니메이션
        int attackType = skill.skillIndex % lowestSkill.skillIndex;
        enemyComponent.PlayAttackAnimation(attackType);
        yield return new WaitForSeconds(0.3f);

        // 5. 스킬 효과 적용
        foreach (var t in targets)
        {
            yield return ApplySkillToTarget(enemyComponent, t, skill, actData);
        }
    }

    /// <summary>
    /// 수호 리다이렉트 적용된 타겟 선택
    /// </summary>
    private static List<IStatusReceiver> ResolveTargetsWithGuard(EnemyAct actData, Enemy enemy)
    {
        var originalTargets = TargetResolver.ResolveForEnemy(actData, enemy);
        var finalTargets = new List<IStatusReceiver>();

        var battleFlow = GameManager.Instance.turnController.battleFlow;
        PlayerController leon = null;

        // 레온 찾기
        foreach (var player in battleFlow.playerParty)
        {
            if (player.ChClass == CharacterClass.Leon && player.IsAlive())
            {
                leon = player as PlayerController;
                break;
            }
        }

        // 수호 체크
        bool hasGuard = false;
        if (leon != null)
        {
            var guardEffect = leon.GetGuardEffect();
            hasGuard = guardEffect.Item1;
        }

        foreach (var target in originalTargets)
        {
            if (hasGuard && target is PlayerController pc && pc.ChClass != CharacterClass.Leon)
            {
                Debug.Log($"[Guard] {pc.ChClass} → Leon으로 타겟 전환");
                finalTargets.Add(leon);
                continue;
            }
            finalTargets.Add(target);
        }

        return finalTargets;
    }

    /// <summary>
    /// 개별 타겟에게 스킬 적용
    /// </summary>
    private static IEnumerator ApplySkillToTarget(Enemy enemy, IStatusReceiver target, EnemySkill skill, EnemyAct actData)
    {
        target.PlayHitAnimation();
        yield return new WaitForSeconds(0.2f);

        string effectName = (target.ChClass == CharacterClass.Enemy)
            ? enemy.enemyData.AllySkillEffect
            : enemy.enemyData.AttackSkillEffect;

        // 사운드 재생
        if (target.ChClass == CharacterClass.Enemy)
            SoundManager.Instance.PlaySFX(SoundCategory.Enemy, 0);
        else
            SoundManager.Instance.PlaySFX(SoundCategory.Enemy, enemy.enemyData.IDNum);

        float scaleFactor = DetermineEffectScale(enemy.enemyData.type);

        if (!string.IsNullOrEmpty(effectName))
        {
            DataManager.Instance.CardEffects.TryGetValue(effectName, out var animInfo);

            if (animInfo != null && animInfo.animationType == AnimationType.Projectile)
            {
                // Projectile 이펙트
                GameManager.Instance.turnController.battleFlow.effectManage.PlayProjectileEffect(
                    effectName,
                    enemy.CachedTransform,
                    target.CachedTransform,
                    scaleFactor,
                    () => ApplyDamageAndEffects(enemy, target, skill, actData)
                );
            }
            else
            {
                // 일반 이펙트
                GameManager.Instance.turnController.battleFlow.effectManage.PlayEffect(
                    effectName,
                    enemy.CachedTransform,
                    target.CachedTransform,
                    true,
                    scaleFactor
                );

                ApplyDamageAndEffects(enemy, target, skill, actData);
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            ApplyDamageAndEffects(enemy, target, skill, actData);
        }

        yield return new WaitForSeconds(0.3f);

        // 사망 처리
        CheckDeathAndHandle(target);
    }

    /// <summary>
    /// 데미지 및 상태효과 적용
    /// </summary>
    private static void ApplyDamageAndEffects(Enemy enemy, IStatusReceiver target, EnemySkill skill, EnemyAct actData)
    {
        target.PlayHitAnimation();

        float damage = skill.damage;

        // 빙결 적용 (적이 빙결 상태면 데미지 감소)
        float freezePenalty = enemy.GetFreezePenalty();
        if (freezePenalty > 0)
        {
            damage = Mathf.Max(0, damage - freezePenalty);
            Debug.Log($"[Freeze] 빙결로 데미지 {freezePenalty} 감소");
        }

        // 수호 피해 감소 적용
        if (target is PlayerController pc && pc.ChClass == CharacterClass.Leon)
        {
            var (shouldRedirect, damageReduction) = pc.GetGuardEffect();
            if (shouldRedirect && damageReduction > 0)
            {
                damage = Mathf.Max(0, damage - damageReduction);
                Debug.Log($"[Guard] 수호로 데미지 {damageReduction} 감소");
                pc.ConsumeGuard();
            }
        }

        target.TakeDamage(damage);
        ApplyStatusEffect(target, actData);
    }

    /// <summary>
    /// 사망 체크 및 처리
    /// </summary>
    private static void CheckDeathAndHandle(IStatusReceiver target)
    {
        if (!target.IsAlive() && target is MonoBehaviour mb && mb.gameObject.activeSelf)
        {
            mb.gameObject.SetActive(false);

            if (target is Enemy e)
            {
                ProgressDataManager.Instance.CurrentExp += e.enemyData.exp;
                GameManager.Instance.turnController.battleFlow.totalExp += e.enemyData.exp;
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
