using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class ParmanoMechanic : EnemyMechanicBase
{


    [SerializeField]
    private RuntimeAnimatorController parmanoFlipController;

    private const int MaxScroll = 3;
    private const float ExecutionDamage = 8f;

    // 실제 Flip ID
    private const int FlipEnemyId = 22;

    private int scroll;

    public override void OnBattleStart()
    {
        scroll = 0;

        foreach (var enemy in battleFlow.enemyParty.OfType<Enemy>())
        {
            if (enemy == null || enemy.enemyData == null)
                continue;

            if (enemy.enemyData.IDNum == FlipEnemyId)
                ApplyParmanoFlipAnimator(enemy);
        }
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
        {
            ApplyParmanoFlipAnimator(flip);
            Debug.Log("[Parmano] Flip 1마리 소환");
        }
    }

    public override IReadOnlyList<EnemyMechanicDisplayData> GetDisplayStatuses()
    {
        return new[]
        {
            new EnemyMechanicDisplayData(
                "Scroll",
                scroll,                  // 아이콘 숫자: 0 → 1 → 2 → 3
                false,
                30,
                MaxScroll,               // Tooltip {0}
                Mathf.RoundToInt(ExecutionDamage) // Tooltip {1}
            )
        };
    }

    private const string FlipControllerPath = "EnemyControllers/Parmano_s Flip";

    private void ApplyParmanoFlipAnimator(Enemy flip)
    {
        if (flip == null || flip.animator == null)
            return;

        var controller = Resources.Load<RuntimeAnimatorController>(FlipControllerPath);

        if (controller == null)
        {
            Debug.LogError("[Parmano] Flip Controller 로드 실패");
            return;
        }

        flip.animator.runtimeAnimatorController = controller;
        flip.animator.Play("Spawn", 0, 0f);
    }
}