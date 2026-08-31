using System.Linq;
using UnityEngine;

public class ParmanoMechanic : EnemyMechanicBase
{
    private const int MaxScroll = 3;
    private const float ExecutionDamage = 8f;

    // 실제 Flip ID
    private const int FlipEnemyId = 22;

    private int scroll;

    public override void OnBattleStart()
    {
        scroll = 0;
    }

    public override void OnPlayerTurnEnd()
    {
        if (GetAliveFlip() == null)
            return;

        scroll = Mathf.Min(scroll + 1, MaxScroll);

        Debug.Log($"[Parmano] 두루마리 {scroll}/{MaxScroll}");
    }

    public override void OnEnemyTurnStart()
    {
        if (scroll < MaxScroll)
            return;

        var flip = GetAliveFlip();

        if (flip == null)
            return;

        ExecuteFlip(flip);

        scroll = 0;
    }

    public override void OnEnemyTurnEnd()
    {
        if (scroll >= MaxScroll)
            return;

        TrySummonFlip();
    }

    private Enemy GetAliveFlip()
    {
        foreach (var e in battleFlow.enemyParty.OfType<Enemy>())
        {
            if (e == null || e.enemyData == null)
                continue;

            if (e.enemyData.IDNum != FlipEnemyId)
                continue;

            Debug.Log(
                $"[Parmano] Flip 발견 / " +
                $"IsAlive={e.IsAlive()} / " +
                $"HP={e.enemyData.CurrentHP} / " +
                $"Active={e.gameObject.activeSelf}"
            );
        }

        return battleFlow.enemyParty
            .OfType<Enemy>()
            .FirstOrDefault(e =>
                e != null &&
                e.IsAlive() &&
                e.enemyData != null &&
                e.enemyData.IDNum == FlipEnemyId);
    }

    private void ExecuteFlip(Enemy flip)
    {
        flip.TakeTrueDamage(flip.currentHP);
        flip.TryFinalizeDeath();

        foreach (var player in battleFlow.playerParty)
        {
            if (player == null || !player.IsAlive())
                continue;

            player.TakeDamage(ExecutionDamage);
        }

        Debug.Log("[Parmano] Flip 처형 / 전체 8 피해");
    }
    private void TrySummonFlip()
    {
        var spawner = Object.FindFirstObjectByType<EnemySpawner>();

        if (spawner == null)
        {
            Debug.LogWarning("[Parmano] EnemySpawner 없음");
            return;
        }

        var flip = spawner.SpawnEnemyToEmptySlot(FlipEnemyId);

        if (flip != null)
            Debug.Log("[Parmano] Flip 1마리 소환");
    }
}