using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EluaMechanic : EnemyMechanicBase
{

    // =========================
    // Soprano
    // =========================
    private const int SopranoTrioDamage = 4;
    private const int SopranoDuetDamage = 6;
    private const int SopranoSoloDamage = 8;

    // =========================
    // Mezzo-Soprano
    // =========================
    private const int MezzoTrioAttack = 1;
    private const int MezzoTrioDefend = 1;

    private const int MezzoDuetDefend = 2;

    private const int MezzoSoloAttack = 2;
    private const int MezzoSoloDefend = 2;

    // =========================
    // Alto
    // =========================
    private const int AltoTrioHeal = 4;
    private const int AltoDuetHeal = 8;

    private const int AltoSoloHeal = 12;
    private const int AltoSoloDefend = 1;

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
            3 => SopranoTrioDamage,
            2 => SopranoDuetDamage,
            _ => SopranoSoloDamage
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
                ApplyTick(elua, BuffStatType.Defend, MezzoTrioDefend);
                ApplyTick(elua, BuffStatType.Attack, MezzoTrioAttack);
            }

            Debug.Log("[Elua] 메조 / 합창 / 전체 방어 +1, 공격 +1");
            return;
        }

        if (aliveCount == 2)
        {
            foreach (var elua in aliveEluas)
                ApplyTick(elua, BuffStatType.Defend, MezzoDuetDefend);

            Debug.Log("[Elua] 메조 / 2중창 / 전체 방어 +2");
            return;
        }

        ApplyTick(singer, BuffStatType.Defend, MezzoSoloDefend);
        ApplyTick(singer, BuffStatType.Attack, MezzoSoloAttack);

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

            lowest?.Heal(AltoDuetHeal);

            Debug.Log("[Elua] 알토 / 2중창 / 최저 체력 회복 8");
            return;
        }

        singer.Heal(12);
        ApplyTick(singer, BuffStatType.Defend, AltoSoloDefend);

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

    public override IReadOnlyList<EnemyMechanicDisplayData> GetDisplayStatuses()
    {
        int aliveCount = GetAliveEluaCount();

        if (aliveCount <= 0)
            return System.Array.Empty<EnemyMechanicDisplayData>();

        string formationText = GetFormationText(aliveCount);
        string effectText = GetVoiceEffectText(aliveCount);

        string type = voice switch
        {
            VoiceType.Soprano => "Soprano",
            VoiceType.Mezzo => "MezzoSoprano",
            VoiceType.Alto => "Alto",
            _ => null
        };

        if (string.IsNullOrEmpty(type))
            return System.Array.Empty<EnemyMechanicDisplayData>();

        return new[]
        {
            new EnemyMechanicDisplayData(
                type,
                0,
                true,
                30,
                formationText, // 최종 Tooltip {0}
                effectText     // 최종 Tooltip {1}
            )
        };
    }

    private int GetAliveEluaCount()
    {
        return battleFlow.enemyParty.OfType<Enemy>().Count(e => e != null && e.IsAlive() && e.Mechanic is EluaMechanic);
    }

    private string GetFormationText(int aliveCount)
    {
        string key = aliveCount switch
        {
            3 => "Type_Name_33_04", // 트리오
            2 => "Type_Name_33_05", // 듀엣
            _ => "Type_Name_33_06"  // 솔로
        };

        return LocaleDataManager.GetLocalizedStringTable(
            "Elite Table",
            key
        );
    }

    private string GetVoiceEffectText(int aliveCount)
    {
        string key;
        object[] args;

        switch (voice)
        {
            case VoiceType.Soprano:
                key = aliveCount switch
                {
                    3 => "Type_Text_33_04",
                    2 => "Type_Text_33_05",
                    _ => "Type_Text_33_06"
                };

                args = new object[]
                {
                aliveCount switch
                {
                    3 => SopranoTrioDamage,
                    2 => SopranoDuetDamage,
                    _ => SopranoSoloDamage
                }
                };
                break;

            case VoiceType.Mezzo:
                if (aliveCount == 3)
                {
                    key = "Type_Text_33_07";
                    args = new object[] { MezzoTrioAttack, MezzoTrioDefend };
                }
                else if (aliveCount == 2)
                {
                    key = "Type_Text_33_08";
                    args = new object[] { MezzoDuetDefend };
                }
                else
                {
                    key = "Type_Text_33_09";
                    args = new object[] {  MezzoSoloAttack, MezzoSoloDefend };
                }
                break;

            case VoiceType.Alto:
                if (aliveCount == 3)
                {
                    key = "Type_Text_33_10";
                    args = new object[] { AltoTrioHeal };
                }
                else if (aliveCount == 2)
                {
                    key = "Type_Text_33_11";
                    args = new object[] { AltoDuetHeal };
                }
                else
                {
                    key = "Type_Text_33_12";
                    args = new object[] {  AltoSoloHeal, AltoSoloDefend };
                }
                break;

            default:
                return string.Empty;
        }

        string text = LocaleDataManager.GetLocalizedStringTable("Elite Table", key);

        return string.Format(text, args);
    }
}