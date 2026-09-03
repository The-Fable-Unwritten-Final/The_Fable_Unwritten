using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class EnemyPattern
{
    public static IEnumerator ExecutePattern(IStatusReceiver enemy)
    {
        if (enemy is not Enemy enemyComponent)
        {
            Debug.LogError("[EnemyPattern] 전달된 IStatusReceiver는 Enemy가 아닙니다.");
            yield break;
        }
        
        if (!enemyComponent.IsAlive())
        {
            enemyComponent.TryFinalizeDeath();
            yield break;
        }

        if (enemyComponent.IsStunned())
            yield break;

        if (enemyComponent.Mechanic?.SkipNormalActionThisTurn == true)
            yield break;

        var skill = enemyComponent.Mechanic?.GetForcedSkill() ?? ChooseSkill(enemyComponent);

        if (skill == null)
        {
            Debug.LogWarning($"[EnemyPattern] {enemyComponent.enemyData.EnemyName}의 스킬 데이터 없음.");
            yield break;
        }

        if (!DataManager.Instance.EnemyActDict.TryGetValue(skill.skillIndex, out var actData))
        {
            Debug.LogWarning($"[EnemyPattern] 스킬 {skill.skillIndex}에 대한 act 데이터가 없습니다.");
            yield break;
        }

        var targets = ChooseTargetsFromActData(actData,enemyComponent);
        targets ??= new List<IStatusReceiver>();
        yield return new WaitForSeconds(0.3f);

        int attackType =enemyComponent.enemyData.SkillList.FindIndex(s => s.skillIndex == skill.skillIndex);

        if (attackType < 0)
        {
            Debug.LogWarning($"[EnemyPattern] SkillList에서 스킬을 찾지 못함: {skill.skillIndex}");
            yield break;
        }

        yield return ExecuteV2Skill(enemyComponent,actData,targets,attackType);
    }


    private static IEnumerator ExecuteV2Skill(Enemy caster, EnemyAct actData, List<IStatusReceiver> skillTargets, int attackType)
    {
        bool hitTriggered = false;
        int pendingImpacts = 0;

        Debug.Log($"[EnemySkill] 시작 index={actData.index}, attackType={attackType}");

        caster.PlayAttackAnimation(attackType,
            () =>
            {
                Debug.Log($"[EnemySkill] 애니메이션 콜백 index={actData.index}");

                if (hitTriggered)
                    return;

                hitTriggered = true;

                ApplyNonSkillTargetEffects(caster, actData);

                foreach (var target in skillTargets)
                {
                    if (target == null || !target.IsAlive())
                        continue;

                    pendingImpacts++;

                    Debug.Log($"[EnemySkill] Impact 시작 index={actData.index}, pending={pendingImpacts}");

                    PlayV2Impact(caster, actData, target, 
                        () =>
                        {
                            Debug.Log($"[EnemySkill] Impact 완료 index={actData.index}");

                            if (target.IsAlive() && target.ChClass != CharacterClass.Enemy)
                                target.PlayHitAnimation();

                            ApplySkillTargetEffects(caster, target, actData);
                            pendingImpacts--;
                        }
                    );
                }
            }
        );

        yield return new WaitUntil(() => hitTriggered);

        Debug.Log($"[EnemySkill] hitTriggered 통과 index={actData.index}");

        yield return new WaitUntil(() => pendingImpacts <= 0);

        Debug.Log($"[EnemySkill] pendingImpacts 통과 index={actData.index}");

        yield return new WaitForSeconds(0.3f);

        FinalizeDeaths();
    }

    private static float DetermineEffectScale(EnemyType type)
    {
        float baseScale = 1f;

        return type switch
        {
            EnemyType.normal => baseScale * 0.5f,
            EnemyType.elite => baseScale,
            EnemyType.boss => baseScale * 1.5f,
            _ => baseScale
        };
    }


    private static List<IStatusReceiver> ChooseTargetsFromActData(EnemyAct actData,Enemy self)
    {
        var targets = new List<IStatusReceiver>();
        var candidates = new List<IStatusReceiver>();

        // V2 Self / 전역 효과는 SkillTarget이 없음
        if (actData.targetType == TargetType.None)
            return targets;

        List<IStatusReceiver> targetGroup =
            actData.targetType == TargetType.Ally
                ? GameManager.Instance.turnController.battleFlow.playerParty
                : GameManager.Instance.turnController.battleFlow.enemyParty;

        if (actData.targetType == TargetType.Ally)
        {
            foreach (var target in targetGroup)
            {
                if (target == null)
                    continue;

                if (!target.IsAlive())
                    continue;

                if (target is PlayerController pc && pc.IsTemporarilyAbsent)
                    continue;

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
            for (int i = 0; i < targetGroup.Count; i++)
            {
                if ((i == 0 && actData.target_front) || (i == 1 && actData.target_center) || (i == 2 && actData.target_back))
                {
                    if (targetGroup[i] == null)
                        continue;

                    if (targetGroup[i].IsAlive())
                        candidates.Add(targetGroup[i]);
                }
            }
        }

        int count = Mathf.Min(actData.targetNum,candidates.Count);

        while (targets.Count < count)
        {
            var chosen = candidates[Random.Range(0,candidates.Count)];

            if (!targets.Contains(chosen))
                targets.Add(chosen);
        }

        return targets;
    }


    private static EnemySkill ChooseSkill(Enemy enemy)
    {
        var skills = enemy.enemyData.SkillList;

        if (skills == null || skills.Count == 0)
            return null;

        float total = 0f;
        var validSkills = new List<EnemySkill>();

        foreach (var skill in skills)
        {
            if (skill == null)
                continue;

            if (skill.percentage <= 0f)
                continue;

            if (!DataManager.Instance.EnemyActDict.TryGetValue(skill.skillIndex,out var actData))
                continue;

            if (!CanUseSkill(enemy, actData))
                continue;

            // useCondition 처리는 다음 단계에서 추가
            validSkills.Add(skill);

            total += skill.percentage;
        }

        if (validSkills.Count == 0 || total <= 0f)
            return null;

        float rand = Random.Range(0f,total);

        float cumulative = 0f;

        foreach (var skill in validSkills)
        {
            cumulative += skill.percentage;

            if (rand < cumulative)
                return skill;
        }

        return validSkills[^1];
    }

    private static void ApplySkillTargetEffects(Enemy caster, IStatusReceiver target, EnemyAct actData)
    {
        if (actData.arg_target1 == EnemyEffectTarget.SkillTarget)
        {
            int value = ResolveEffectValue(caster, target, actData, actData.arg1);
            value = caster.Mechanic?.ModifySkillEffectValue(target, actData.arg_effect1,actData.arg_target1,value) ?? value;
            ApplyV2Effect(caster, target, actData.arg_effect1, value);
        }

        if (actData.arg_target2 == EnemyEffectTarget.SkillTarget)
        {
            int value = actData.arg2;
            value = caster.Mechanic?.ModifySkillEffectValue(target, actData.arg_effect2,actData.arg_target2,value) ?? value;
            ApplyV2Effect(caster, target, actData.arg_effect2, value);
        }
    }

    private static void ApplyNonSkillTargetEffects(Enemy caster,EnemyAct actData)
    {
        if (actData.arg_target1 != EnemyEffectTarget.SkillTarget)
            ApplyV2EffectByTargetType(caster,actData.arg_effect1,actData.arg1,actData.arg_target1);

        if (actData.arg_target2 != EnemyEffectTarget.SkillTarget)
            ApplyV2EffectByTargetType(caster,actData.arg_effect2,actData.arg2,actData.arg_target2);
    }

    private static void ApplyV2EffectByTargetType(Enemy caster,EnemyEffectType effectType,int value,EnemyEffectTarget targetType)
    {
        if (effectType == EnemyEffectType.None)
            return;

        if (targetType == EnemyEffectTarget.None)
        {
            if (effectType == EnemyEffectType.Potential)
                ApplyPotentialToParty(value);
            return;
        }

        switch (targetType)
        {
            case EnemyEffectTarget.Self:
                ApplyV2Effect(caster,caster,effectType,value);
                return;

            case EnemyEffectTarget.Allies:
                foreach (var ally in GameManager.Instance.turnController.battleFlow.enemyParty)
                {
                    if (ally == null || !ally.IsAlive())
                        continue;

                    ApplyV2Effect(caster,ally,effectType,value);
                }
                return;

            case EnemyEffectTarget.LowestHpAlly:
                ApplyToLowestHpAlly(caster,effectType,value);
                return;
        }
    }
    
    private static void ApplyV2Effect(Enemy caster,IStatusReceiver target,EnemyEffectType effectType,int value)
    {
        if (target == null || !target.IsAlive())
            return;

        switch (effectType)
        {
            case EnemyEffectType.None:
                return;


            // =====================================================
            // 즉시 효과
            // =====================================================

            case EnemyEffectType.Damage:
                target.TakeDamage(value);
                return;

            case EnemyEffectType.Heal:
                target.Heal(value);
                return;


            // =====================================================
            // 턴 지속 수치
            // =====================================================

            case EnemyEffectType.Attack:
                ApplyTickEffect(target,BuffStatType.Attack,value);
                return;

            case EnemyEffectType.Defense:
                ApplyTickEffect(target,BuffStatType.Defend,value);
                return;


            // =====================================================
            // 공통 InstanceEffect
            // =====================================================

            case EnemyEffectType.Burn:
                ApplyInstanceEffect(target, BuffStatType.Burn, value);
                return;

            case EnemyEffectType.Freeze:
                ApplyInstanceEffect(target, BuffStatType.Freeze, value);
                return;

            case EnemyEffectType.Activate:
                ApplyInstanceEffect(target, BuffStatType.Activate, value);
                return;

            case EnemyEffectType.Bless:
                ApplyInstanceEffect(target, BuffStatType.Bless, value);
                return;

            case EnemyEffectType.Crime:
                ApplyInstanceEffect(target, BuffStatType.Sin, value);
                return;

            case EnemyEffectType.Penance:
                ApplyInstanceEffect(target, BuffStatType.Penance, value);
                return;

            case EnemyEffectType.Scar:
                ApplyInstanceEffect(target, BuffStatType.Scar, value);
                return;

            case EnemyEffectType.Stun:
                ApplyInstanceEffect(target, BuffStatType.Stun, value);
                return;

            case EnemyEffectType.Guard:
                ApplyInstanceEffect(target, BuffStatType.Guard, value);
                return;


            // =====================================================
            // 직접 처리 효과
            // =====================================================

            case EnemyEffectType.Block:
                ApplyBlock(target);
                return;

            case EnemyEffectType.Resist:
                ApplyResist(target);
                return;

            case EnemyEffectType.CleanseDebuff:
                CleanseDebuffs(target);
                return;

            case EnemyEffectType.Potential:
                ApplyPotential(target, value);
                return;


            // =====================================================
            // 적 전용 / 기믹 상태
            // =====================================================

            case EnemyEffectType.Reflect:
                ApplyInstanceEffect(target, BuffStatType.Reflect, value);
                return;

            case EnemyEffectType.Mark:
                ApplyMark(target, value);
                return;

            case EnemyEffectType.AssaultReady:
                ApplyInstanceEffect(target, BuffStatType.AssaultReady, value);
                return;

            case EnemyEffectType.ShieldTactic:
                ApplyInstanceEffect(target, BuffStatType.ShieldTactic, value);
                return;

            case EnemyEffectType.Combo:
                ApplyInstanceEffect(target, BuffStatType.Combo, value);
                return;

            case EnemyEffectType.TargetMark:
                ApplyInstanceEffect(target, BuffStatType.TargetMark, value);
                return;

            case EnemyEffectType.Formation:
                ApplyInstanceEffect(target, BuffStatType.Formation, value);
                return;

            case EnemyEffectType.Hap:
                ApplyInstanceEffect(target, BuffStatType.Hap, value);
                return;

            default:
                Debug.LogWarning(
                    $"[EnemyPattern/V2] 처리되지 않은 Effect: {effectType}"
                );
                return;
        }
    }


    private static void PlayV2Impact(Enemy caster, EnemyAct actData, IStatusReceiver target,Action onImpact)
    {
        string effectName = !string.IsNullOrEmpty(actData.skilleffect) ? actData.skilleffect : target.ChClass == 
            CharacterClass.Enemy ? caster.enemyData.AllySkillEffect : caster.enemyData.AttackSkillEffect;


        if (string.IsNullOrEmpty(effectName))
        {
            onImpact?.Invoke();
            return;
        }

        if (target == caster)
            SoundManager.Instance.PlaySFX(SoundCategory.Enemy,0);
        else
            SoundManager.Instance.PlaySFX(SoundCategory.Enemy,caster.enemyData.IDNum);

        float scaleFactor =DetermineEffectScale(caster.enemyData.type);

        if (!DataManager.Instance.CardEffects.TryGetValue(effectName,out var animInfo) || animInfo == null)
        {
            onImpact?.Invoke();
            return;
        }

        if (animInfo.animationType == AnimationType.Projectile)
        {
            GameManager.Instance.turnController.battleFlow.effectManage.PlayProjectileEffect(effectName,caster,target,scaleFactor,onImpact);
            return;
        }

        GameManager.Instance.turnController.battleFlow.effectManage.PlayEffect(effectName,caster,target,true,scaleFactor);
        onImpact?.Invoke();
    }


    private static void FinalizeDeaths()
    {
        var battleFlow =GameManager.Instance.turnController.battleFlow;

        foreach (var target in battleFlow.playerParty.Concat(battleFlow.enemyParty))
        {
            if (target == null)
                continue;

            if (target.IsAlive())
                continue;

            if (target is not MonoBehaviour mb)
                continue;

            if (!mb.gameObject.activeSelf)
                continue;

            target.TryFinalizeDeath();

            if (target is Enemy enemy)
            {
                ProgressDataManager.Instance.CurrentExp += enemy.enemyData.exp;
                battleFlow.totalExp += enemy.enemyData.exp;
            }
        }
    }


    private static void ApplyTickEffect(IStatusReceiver target, BuffStatType statType, int value, int duration = 2)
    {
        target.ApplyStatusEffect(
            new TickEffect
            {
                statType = statType,
                value = value,

                // 현재 EnemyActV2에는 duration 컬럼이 없기 때문에
                // 우선 기존 적 스킬의 기본 지속시간을 1로 사용
                duration = duration
            }
        );
    }

    private static void ApplyInstanceEffect(IStatusReceiver target, BuffStatType statType, int value, bool isMaintain = false)
    {
        target.ApplyStatusEffect(
            new InstanceEffect
            {
                statType = statType,
                value = value,
                isMaintain = isMaintain
            }
        );
    }

    private static void ApplyBlock(IStatusReceiver target)
    {
        if (target is PlayerController pc)
        {
            pc.hasBlock = true;
            return;
        }

        if (target is Enemy enemy)
        {
            enemy.hasBlock = true;
        }
    }

    private static void ApplyResist(IStatusReceiver target)
    {
        if (target is PlayerController pc)
        {
            pc.hasResist = true;
            return;
        }

        if (target is Enemy enemy)
            enemy.hasResist = true;
    }

    private static void CleanseDebuffs(IStatusReceiver target)
    {
        if (target is PlayerController pc)
        {
            pc.tickEffects.RemoveAll(e => Debuff.IsDebuff(e.statType, e.value));
            pc.instantEffects.RemoveAll(e => Debuff.IsDebuff(e.statType, e.value));

            return;
        }

        if (target is Enemy enemy)
        {
            enemy.tickEffects.RemoveAll(e => Debuff.IsDebuff(e.statType, e.value));
            enemy.instantEffects.RemoveAll(e => Debuff.IsDebuff(e.statType, e.value));
        }
    }

    private static void ApplyMark(IStatusReceiver target, int value)
    {
        var players = GameManager.Instance.turnController.battleFlow.playerParty;

        foreach (var player in players)
        {
            if (player is not PlayerController pc)
                continue;

            var mark = pc.instantEffects.Find(e => e.statType == BuffStatType.Mark);

            if (mark != null)
                mark.value = 0;
        }

        ApplyInstanceEffect(target,BuffStatType.Mark,value);
    }

    private static void ApplyPotential(IStatusReceiver target, int value)
    {
        if (target is not PlayerController pc)
            return;

        if (pc.StanceSystem == null)
            return;

        if (value > 0) pc.StanceSystem.IncreaseGauge(value);
        else if (value < 0) pc.StanceSystem.DecreaseGauge(-value);
    }

    private static void ApplyPotentialToParty(int value)
    {
        var players = GameManager.Instance.turnController.battleFlow.playerParty;

        foreach (var target in players)
        {
            if (target is not PlayerController pc)
                continue;

            if (!pc.IsAlive())
                continue;

            if (pc.StanceSystem == null) 
                continue;

            if (value > 0) 
                pc.StanceSystem.IncreaseGauge(value);
            else if (value < 0) 
                pc.StanceSystem.DecreaseGauge(-value);
        }
    }

    private static void ApplyToLowestHpAlly(Enemy caster,EnemyEffectType effectType,int value)
    {
        var enemies = GameManager.Instance.turnController.battleFlow.enemyParty;

        Enemy lowest = null;

        foreach (var target in enemies)
        {
            if (target is not Enemy enemy)
                continue;

            if (!enemy.IsAlive())
                continue;

            if (lowest == null || enemy.enemyData.CurrentHP < lowest.enemyData.CurrentHP)
                lowest = enemy;
        }

        if (lowest != null)
            ApplyV2Effect(caster, lowest, effectType, value);
    }

    private static bool CanUseSkill(Enemy caster,EnemyAct actData)
    {
        switch (actData.useCondition)
        {
            case EnemyUseCondition.None:
                return true;

            case EnemyUseCondition.SelfHpBelow:
                {
                    if (caster.enemyData.MaxHP <= 0)
                        return false;

                    float hpPercent = caster.enemyData.CurrentHP / caster.enemyData.MaxHP * 100f;
                    return hpPercent <= actData.useConditionValue;
                }

            default:
                return true;
        }
    }

    private static int ResolveEffectValue(Enemy caster, IStatusReceiver target, EnemyAct actData, int baseValue)
    {
        switch (actData.valueModifier)
        {
            case EnemyValueModifier.None:
                return baseValue;

            case EnemyValueModifier.AddTargetStatusValue:
                return baseValue + GetEnemyEffectValue(target, actData.modifierStatus);

            case EnemyValueModifier.AddSelfStatusValueMultiply:
                return baseValue + GetEnemyEffectValue(caster, actData.modifierStatus) * actData.modifierValue;

            case EnemyValueModifier.AddValueIfSelfHpBelow:
                {
                    if (caster.enemyData.MaxHP <= 0)
                        return baseValue;

                    float hpPercent = caster.enemyData.CurrentHP / caster.enemyData.MaxHP * 100f;

                    if (hpPercent <= actData.modifierConditionValue)
                        return baseValue + actData.modifierValue;

                    return baseValue;
                }

            default:
                return baseValue;
        }
    }

    private static int GetEnemyEffectValue(IStatusReceiver target, EnemyEffectType effectType)
    {
        BuffStatType statType = effectType switch
        {
            EnemyEffectType.Scar => BuffStatType.Scar,
            EnemyEffectType.Hap => BuffStatType.Hap,
            EnemyEffectType.Mark => BuffStatType.Mark,
            EnemyEffectType.TargetMark => BuffStatType.TargetMark,
            EnemyEffectType.Combo => BuffStatType.Combo,
            EnemyEffectType.Formation => BuffStatType.Formation,
            _ => BuffStatType.None
        };

        if (statType == BuffStatType.None)
            return 0;

        if (target is PlayerController pc)
            return Mathf.RoundToInt(pc.GetEffectValue(statType));

        if (target is Enemy enemy)
            return Mathf.RoundToInt(enemy.GetEffectValue(statType));

        return 0;
    }

}
