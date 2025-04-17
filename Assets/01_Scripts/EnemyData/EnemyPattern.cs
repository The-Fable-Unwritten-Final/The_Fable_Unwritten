using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Enemy의 턴마다 스킬 또는 공격을 실행하는 패턴 제어 클래스
/// EnemyData에 저장된 스킬 정보와 EnemyAct를 기반으로 실행
/// </summary>
public class EnemyPattern : MonoBehaviour
{
    // 전투 중 현재 플레이어 파티와 적 파티 참조
    private List<IStatusReceiver> playerParty = GameManager.Instance.turnController.battleFlow.playerParty;
    private List<IStatusReceiver> enemyParty = GameManager.Instance.turnController.battleFlow.enemyParty;


    // StanceType 개수 (한 번만 계산)
    private static readonly int stanceCount =
        System.Enum.GetValues(typeof(PlayerData.StancType)).Length;
    
    
    /// <summary>
    /// 외부에서 호출되는 메인 메서드 - 적이 턴에 행동을 수행함
    /// </summary>
    public void ExecutePattern(IStatusReceiver enemy)
    {
        // 받은 친구가 Enemy 타입인지 확인
        if (enemy is not Enemy enemyComponent)
        {
            Debug.LogError("[EnemyPattern] 전달된 IStatusReceiver는 Enemy가 아닙니다.");
            return;
        }
        // 1. 자세 변경
        SetRandomStance(enemyComponent);
        
        // 2. 사용할 스킬 선택
        var skill = ChooseSkill(enemyComponent);
        if (skill == null)
        {
            Debug.LogWarning($"[EnemyPattern] {enemyComponent.enemyData.EnemyName}의 스킬 데이터 없음.");
            return;
        }

        // 3. 스킬 데이터 가져오기 
        var actData = EnemySkillDatabase.Instance.Get(skill.skillIndex);
        if (actData == null)
        {
            Debug.LogWarning($"[EnemyPattern] 스킬 {skill.skillIndex}에 대한 act 데이터가 없습니다.");
            return;
        }

        // 4. 타겟 선택
        var targets = ChooseTargetsFromActData(actData, enemyComponent);

        // 5. 타겟에게 데미지 및 추가 효과 적용
        foreach (var t in targets)
        {
            t.TakeDamage(skill.damage);
            ApplyStatusEffect(t, actData);
            Debug.Log($"[EnemyPattern] {enemyComponent.enemyData.EnemyName} → {t.ChClass}에게 스킬 {skill.skillIndex} 사용 (데미지 {skill.damage})");
        }
    }


    /// <summary>
    /// 스킬 데이터에 따라서 상대 및 아군 효과 대상을 선택
    /// </summary>
    /// <param name="actData"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    private List<IStatusReceiver> ChooseTargetsFromActData(EnemyAct actData, Enemy self)
    {
        var targets = new List<IStatusReceiver>();
        var candidates = new List<IStatusReceiver>();

        // 1. 타겟 그룹 설정 (적 기준: Ally → 플레이어, Enemy → 적 자신 포함)
        List<IStatusReceiver> targetGroup = actData.targetType == TargetType.Ally
            ? playerParty
            : enemyParty;


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


    /// <summary>
    /// 랜덤한 StanceType을 설정하고 로그를 출력
    /// </summary>
    private void SetRandomStance(Enemy enemy)
    {
        var stance = (PlayerData.StancType)Random.Range(0, stanceCount);
        enemy.enemyData.currentStance = (EnemyData.StancValue.EStancType)stance;
        Debug.Log($"[EnemyPattern] {enemy.enemyData.EnemyName} 자세 → {stance}");
    }

    /// <summary>
    /// 에너미 데이터에 따라 사용할 스킬을 랜덤으로 선택
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    private EnemySkill ChooseSkill(Enemy enemy)
    {
        if (enemy.enemyData.SkillDict == null || enemy.enemyData.SkillDict.Count == 0)
            return null; // 스킬 없으면 기본 공격

        float total = 0;                
        foreach (var s in enemy.enemyData.SkillDict.Values)         //스킬 스킬 공격 확률에 따라 스킬 선택
            total += s.percentage;

        float rand = Random.Range(0f, 1);
        float cumulative = 0;

        foreach (var skill in enemy.enemyData.SkillDict.Values)
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
    private void ApplyStatusEffect(IStatusReceiver target, EnemyAct act)
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
