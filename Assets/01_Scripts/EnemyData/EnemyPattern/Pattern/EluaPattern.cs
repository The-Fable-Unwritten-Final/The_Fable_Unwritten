using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EluaMechanic : EnemyMechanicBase
{
    private enum VoiceType
    {
        Soprano,
        Mezzo,
        Alto
    }

    private VoiceType voice;

    public override void OnBattleStart()
    {
        AssignVoice();
    }

    private void AssignVoice()
    {
        var eluas = battleFlow.enemyParty
            .OfType<Enemy>()
            .Where(e => e != null && e.Mechanic is EluaMechanic)
            .ToList();

        int index = eluas.IndexOf(owner);

        voice = index switch
        {
            0 => VoiceType.Soprano,
            1 => VoiceType.Mezzo,
            _ => VoiceType.Alto
        };

        Debug.Log($"[Elua] {owner.name} 성부 = {voice}");
    }

    public override void OnEnemyTurnStart()
    {
        if (!IsCoordinator())
            return;

        var aliveEluas = GetAliveEluas();

        int aliveCount = aliveEluas.Count;

        if (aliveCount <= 0)
            return;

        // 알토 → 메조 → 소프라노 순서
        ExecuteVoice(aliveEluas, VoiceType.Alto, aliveCount);
        ExecuteVoice(aliveEluas, VoiceType.Mezzo, aliveCount);
        ExecuteVoice(aliveEluas, VoiceType.Soprano, aliveCount);
    }

    private bool IsCoordinator()
    {
        var coordinator = battleFlow.enemyParty
            .OfType<Enemy>()
            .FirstOrDefault(e =>
                e != null &&
                e.IsAlive() &&
                e.Mechanic is EluaMechanic);

        return coordinator == owner;
    }

    private List<Enemy> GetAliveEluas()
    {
        return battleFlow.enemyParty
            .OfType<Enemy>()
            .Where(e =>
                e != null &&
                e.IsAlive() &&
                e.Mechanic is EluaMechanic)
            .ToList();
    }

    private void ExecuteVoice(List<Enemy> aliveEluas, VoiceType targetVoice, int aliveCount)
    {
        Enemy singer = aliveEluas.FirstOrDefault(e =>
            e.Mechanic is EluaMechanic mechanic &&
            mechanic.voice == targetVoice);

        if (singer == null)
            return;

        switch (targetVoice)
        {
            case VoiceType.Alto:
                ExecuteAlto(singer, aliveEluas, aliveCount);
                break;

            case VoiceType.Mezzo:
                ExecuteMezzo(singer, aliveEluas, aliveCount);
                break;

            case VoiceType.Soprano:
                ExecuteSoprano(aliveCount);
                break;
        }
    }

    private void ExecuteSoprano(int aliveCount)
    {
        float damage = aliveCount switch
        {
            3 => 4f,
            2 => 6f,
            _ => 8f
        };

        foreach (var player in battleFlow.playerParty)
        {
            if (player == null || !player.IsAlive())
                continue;

            if (player is PlayerController pc && pc.IsTemporarilyAbsent)
                continue;

            player.TakeDamage(damage);
        }

        Debug.Log($"[Elua] 소프라노 / {aliveCount}체 생존 / 전체 피해 {damage}");
    }

    private void ExecuteMezzo(Enemy singer, List<Enemy> aliveEluas, int aliveCount)
    {
        if (aliveCount == 3)
        {
            foreach (var elua in aliveEluas)
            {
                ApplyTick(elua, BuffStatType.Defend, 1);
                ApplyTick(elua, BuffStatType.Attack, 1);
            }

            Debug.Log("[Elua] 메조 / 합창 / 전체 방어 +1, 공격 +1");
            return;
        }

        if (aliveCount == 2)
        {
            foreach (var elua in aliveEluas)
                ApplyTick(elua, BuffStatType.Defend, 2);

            Debug.Log("[Elua] 메조 / 2중창 / 전체 방어 +2");
            return;
        }

        ApplyTick(singer, BuffStatType.Defend, 2);
        ApplyTick(singer, BuffStatType.Attack, 2);

        Debug.Log("[Elua] 메조 / 독창 / 자신 방어 +2, 공격 +2");
    }

    private void ExecuteAlto(Enemy singer, List<Enemy> aliveEluas, int aliveCount)
    {
        if (aliveCount == 3)
        {
            foreach (var elua in aliveEluas)
                elua.Heal(4);

            Debug.Log("[Elua] 알토 / 합창 / 전체 회복 4");
            return;
        }

        if (aliveCount == 2)
        {
            Enemy lowest = aliveEluas
                .OrderBy(e => e.currentHP)
                .FirstOrDefault();

            lowest?.Heal(8);

            Debug.Log("[Elua] 알토 / 2중창 / 최저 체력 회복 8");
            return;
        }

        singer.Heal(12);
        ApplyTick(singer, BuffStatType.Defend, 1);

        Debug.Log("[Elua] 알토 / 독창 / 자신 회복 12, 방어 +1");
    }

    private void ApplyTick(IStatusReceiver target, BuffStatType statType, int value)
    {
        target.ApplyStatusEffect(
            new TickEffect
            {
                statType = statType,
                value = value,
                duration = 2
            }
        );
    }
}