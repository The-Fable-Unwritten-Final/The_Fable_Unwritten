using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class MarnasMechanic : EnemyMechanicBase
{
    private const int MaxRuneShell = 3;
    private const int TidalWaveSkillIndex = 233;

    private int runeShell;
    private bool forceTidalWave;

    public override void OnBattleStart()
    {
        runeShell = MaxRuneShell;
        forceTidalWave = false;
    }

    public override void OnDamaged(DamageContext context)
    {
        // 카드 공격 피해만 갑각 감소
        if (!context.IsAttackDamage)
            return;

        // 실제 피해가 들어오지 않았다면 감소하지 않음
        if (context.FinalDamage <= 0)
            return;

        // 이미 깨져 있으면 더 감소하지 않음
        if (runeShell <= 0)
            return;

        runeShell--;

        UnityEngine.Debug.Log($"룬 갑각 {runeShell}");

        if (runeShell == 0)
        {
            forceTidalWave = true;
        }
    }
    public override float ModifyStat(BuffStatType statType, float value)
    {
        if (statType == BuffStatType.Defense && runeShell > 0)
            value += 2;

        if (statType == BuffStatType.Attack && forceTidalWave)
            value += 2;

        return value;
    }

    public override EnemySkill GetForcedSkill()
    {
        if (!forceTidalWave)
            return null;

        UnityEngine.Debug.Log("해류 파동 발동!");

        return owner.enemyData.SkillList
            .FirstOrDefault(x => x.skillIndex == TidalWaveSkillIndex);
    }

    public override void OnEnemyTurnEnd()
    {
        if (!forceTidalWave)
            return;

        runeShell = MaxRuneShell;
        forceTidalWave = false;
    }
}