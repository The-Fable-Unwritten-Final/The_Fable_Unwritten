using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기존 TargetType, targetNum을 그대로 사용하는 통합 타겟 리졸버
/// - 카드 효과
/// - BattleFlow
/// - EnemyPattern
/// 모두 동일한 방식으로 타겟팅
/// </summary>
public static class TargetResolver
{

    /// <summary>
    /// 메인 리졸브 메서드
    /// </summary>
    /// <param name="targetType">타겟 타입 (None=자신/아군, Ally=아군, Enemy=적)</param>
    /// <param name="targetNum">타겟 수 (0=자신, 1=단일, 2=둘, 3=전체)</param>
    /// <param name="caster">시전자</param>
    /// <param name="selectedTarget">유저가 선택한 타겟 (있으면 우선)</param>
    /// <param name="positionFilter">위치 필터 (적 스킬용, null이면 전체)</param>
    /// <returns>최종 타겟 리스트</returns>
    public static List<IStatusReceiver> Resolve(
        TargetType targetType,
        int targetNum,
        IStatusReceiver caster,
        IStatusReceiver selectedTarget = null,
        PositionFilter? positionFilter = null)
    {
        var battleFlow = GameManager.Instance?.turnController?.battleFlow;
        if (battleFlow == null)
        {
            Debug.LogWarning("[TargetResolver] BattleFlow is null");
            return new List<IStatusReceiver>();
        }

        // 1. 시전자 기준으로 아군/적 풀 결정
        bool casterIsEnemy = caster.ChClass == CharacterClass.Enemy;

        List<IStatusReceiver> allyPool = casterIsEnemy
            ? battleFlow.enemyParty
            : battleFlow.playerParty;

        List<IStatusReceiver> enemyPool = casterIsEnemy
            ? battleFlow.playerParty
            : battleFlow.enemyParty;

        // 2. targetType에 따라 풀 선택
        List<IStatusReceiver> pool = targetType switch
        {
            TargetType.None => allyPool,   // 자기편
            TargetType.Ally => allyPool,   // 아군
            TargetType.Enemy => enemyPool, // 적
            _ => allyPool
        };

        // 3. 생존자만 필터링
        var alive = GetAliveUnits(pool);

        // 4. 위치 필터 적용 (있으면)
        if (positionFilter.HasValue)
        {
            alive = ApplyPositionFilter(alive, positionFilter.Value, caster);
        }

        // 5. targetNum에 따라 최종 선택
        return SelectByTargetNum(targetType, targetNum, alive, caster, selectedTarget);
    }

    /// <summary>
    /// EnemyAct용 편의 메서드
    /// </summary>
    public static List<IStatusReceiver> ResolveForEnemy(
        EnemyAct actData,
        IStatusReceiver enemyCaster)
    {
        var filter = CreatePositionFilter(
            actData.target_front,
            actData.target_center,
            actData.target_back
        );

        return Resolve(
            actData.targetType,
            actData.targetNum,
            enemyCaster,
            null,
            filter
        );
    }

    /// <summary>
    /// 카드 효과용 편의 메서드 (기존 int target 값 사용)
    /// </summary>
    public static List<IStatusReceiver> ResolveForEffect(
        int? effectTarget,
        IStatusReceiver caster,
        List<IStatusReceiver> cardTargets)
    {
        if (effectTarget == null)
            return cardTargets ?? new List<IStatusReceiver>();

        // 기존 effect.target 값 매핑
        return effectTarget.Value switch
        {
            0 => FindCharacter(CharacterClass.Sophia),
            1 => FindCharacter(CharacterClass.Kayla),
            2 => FindCharacter(CharacterClass.Leon),
            3 => Resolve(TargetType.Ally, 3, caster),  // 아군 전체
            4 => cardTargets ?? new List<IStatusReceiver>(),  // 카드 타겟
            5 => Resolve(TargetType.Enemy, 3, caster), // 적 전체
            6 => new List<IStatusReceiver> { caster }, // 시전자
            _ => cardTargets ?? new List<IStatusReceiver>()
        };
    }

    private static List<IStatusReceiver> SelectByTargetNum(
        TargetType targetType,
        int targetNum,
        List<IStatusReceiver> pool,
        IStatusReceiver caster,
        IStatusReceiver selectedTarget)
    {
        if (pool.Count == 0)
            return new List<IStatusReceiver>();

        // TargetType.None이고 targetNum=0이면 시전자 캐릭터
        if (targetType == TargetType.None && targetNum == 0)
        {
            return GetCasterCharacter(caster, pool);
        }

        return targetNum switch
        {
            0 => GetSingleTarget(pool, selectedTarget),      // 단일 (선택 우선)
            1 => GetSingleTarget(pool, selectedTarget),      // 단일
            2 => GetMultipleTargets(pool, 2, selectedTarget), // 2명
            3 => new List<IStatusReceiver>(pool),            // 전체
            _ => GetMultipleTargets(pool, targetNum, selectedTarget) // N명
        };
    }

    private static List<IStatusReceiver> GetCasterCharacter(
        IStatusReceiver caster,
        List<IStatusReceiver> pool)
    {
        // 시전자와 같은 캐릭터 클래스 찾기
        foreach (var unit in pool)
        {
            if (unit != null && unit.IsAlive() && unit.ChClass == caster.ChClass)
            {
                return new List<IStatusReceiver> { unit };
            }
        }

        // 없으면 시전자 자신
        return new List<IStatusReceiver> { caster };
    }

    private static List<IStatusReceiver> GetSingleTarget(
        List<IStatusReceiver> pool,
        IStatusReceiver selectedTarget)
    {
        // 선택된 타겟 우선
        if (selectedTarget != null && pool.Contains(selectedTarget) && selectedTarget.IsAlive())
        {
            return new List<IStatusReceiver> { selectedTarget };
        }

        // 없으면 첫 번째 생존자
        foreach (var unit in pool)
        {
            if (unit != null && unit.IsAlive())
                return new List<IStatusReceiver> { unit };
        }

        return new List<IStatusReceiver>();
    }

    private static List<IStatusReceiver> GetMultipleTargets(
        List<IStatusReceiver> pool,
        int count,
        IStatusReceiver selectedTarget)
    {
        var result = new List<IStatusReceiver>();
        var candidates = new List<IStatusReceiver>(pool);

        // 선택된 타겟 우선 추가
        if (selectedTarget != null && candidates.Contains(selectedTarget) && selectedTarget.IsAlive())
        {
            result.Add(selectedTarget);
            candidates.Remove(selectedTarget);
        }

        // 나머지 랜덤 추가
        while (result.Count < count && candidates.Count > 0)
        {
            int idx = Random.Range(0, candidates.Count);
            var target = candidates[idx];

            if (target != null && target.IsAlive())
            {
                result.Add(target);
            }
            candidates.RemoveAt(idx);
        }

        return result;
    }

    /// <summary>
    /// 위치 필터 (비트 플래그)
    /// </summary>
    [System.Flags]
    public enum PositionFilter
    {
        None = 0,
        Front = 1 << 0,   // 앞줄: 레온 / enemyParty[0]
        Center = 1 << 1,  // 중앙: 소피아 / enemyParty[1]
        Back = 1 << 2,    // 뒷줄: 카일라 / enemyParty[2]
        All = Front | Center | Back
    }

    private static PositionFilter CreatePositionFilter(bool front, bool center, bool back)
    {
        var filter = PositionFilter.None;
        if (front) filter |= PositionFilter.Front;
        if (center) filter |= PositionFilter.Center;
        if (back) filter |= PositionFilter.Back;

        // 아무것도 없으면 전체
        return filter == PositionFilter.None ? PositionFilter.All : filter;
    }

    private static List<IStatusReceiver> ApplyPositionFilter(
        List<IStatusReceiver> pool,
        PositionFilter filter,
        IStatusReceiver caster)
    {
        if (filter == PositionFilter.All)
            return pool;

        var result = new List<IStatusReceiver>();
        var battleFlow = GameManager.Instance.turnController.battleFlow;

        foreach (var unit in pool)
        {
            if (unit == null || !unit.IsAlive()) continue;

            bool matches = false;

            if (unit.ChClass == CharacterClass.Enemy)
            {
                // 적은 인덱스로 위치 판단
                int index = battleFlow.enemyParty.IndexOf(unit);
                matches = IsPositionMatch(index, filter);
            }
            else
            {
                // 플레이어는 캐릭터 클래스로 판단
                matches = unit.ChClass switch
                {
                    CharacterClass.Leon => (filter & PositionFilter.Front) != 0,
                    CharacterClass.Sophia => (filter & PositionFilter.Center) != 0,
                    CharacterClass.Kayla => (filter & PositionFilter.Back) != 0,
                    _ => true
                };
            }

            if (matches)
                result.Add(unit);
        }

        return result;
    }

    private static bool IsPositionMatch(int index, PositionFilter filter)
    {
        return index switch
        {
            0 => (filter & PositionFilter.Front) != 0,
            1 => (filter & PositionFilter.Center) != 0,
            2 => (filter & PositionFilter.Back) != 0,
            _ => true
        };
    }


    private static List<IStatusReceiver> GetAliveUnits(List<IStatusReceiver> units)
    {
        var result = new List<IStatusReceiver>();
        if (units == null) return result;

        foreach (var unit in units)
        {
            if (unit != null && unit.IsAlive())
                result.Add(unit);
        }

        return result;
    }

    private static List<IStatusReceiver> FindCharacter(CharacterClass targetClass)
    {
        var battleFlow = GameManager.Instance?.turnController?.battleFlow;
        if (battleFlow == null) return new List<IStatusReceiver>();

        foreach (var unit in battleFlow.playerParty)
        {
            if (unit != null && unit.IsAlive() && unit.ChClass == targetClass)
            {
                return new List<IStatusReceiver> { unit };
            }
        }

        return new List<IStatusReceiver>();
    }
}