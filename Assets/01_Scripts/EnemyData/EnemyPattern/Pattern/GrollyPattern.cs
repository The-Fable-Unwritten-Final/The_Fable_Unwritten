using System.Linq;
using UnityEngine;

public class GrollyMechanic : EnemyMechanicBase
{
    private const int MaxDevour = 24;

    private const float SwallowDamage = 6f;
    private const float DigestDamage = 10f;
    private const float DigestHeal = 10f;

    private int playerTurnEndCount;
    private int devour;

    private PlayerController swallowedPlayer;

    public override void OnBattleStart()
    {
        playerTurnEndCount = 0;
        devour = 0;
        swallowedPlayer = null;
    }

    public override void OnPlayerTurnEnd()
    {
        playerTurnEndCount++;

        // 이미 누군가를 삼킨 상태라면
        // 이번 플레이어 턴 종료에 소화 처리 후 뱉는다.
        if (swallowedPlayer != null)
        {
            ResolveDigest();
            return;
        }

        // 첫 삼킴: 2번째 플레이어 턴 종료
        // 이후: 3턴마다
        bool shouldSwallow =
            playerTurnEndCount == 2 ||
            (
                playerTurnEndCount > 2 &&
                (playerTurnEndCount - 2) % 3 == 0
            );

        if (!shouldSwallow)
            return;

        TrySwallow();
    }

    public override void OnDamaged(DamageContext context)
    {
        // 그롤리가 죽었다면 즉시 석방
        if (!owner.IsAlive())
        {
            ReleasePlayer();
            return;
        }

        // 삼킨 대상이 없으면 포식 감소 없음
        if (swallowedPlayer == null)
            return;

        // 공격 피해만 포식 감소
        if (!context.IsAttackDamage)
            return;

        if (context.FinalDamage <= 0)
            return;

        int decrease = Mathf.RoundToInt(context.FinalDamage);

        devour = Mathf.Max(0, devour - decrease);

        Debug.Log(
            $"[Grolly] 피해 {context.FinalDamage} → 포식 -{decrease} / 현재 {devour}/{MaxDevour}"
        );

        // 포식 0이면 즉시 뱉음
        if (devour <= 0)
            ReleasePlayer();
    }

    private void TrySwallow()
    {
        var target = battleFlow.playerParty
            .OfType<PlayerController>()
            .Where(p =>
                p != null &&
                p.CanParticipateInBattle())
            .OrderByDescending(p => p.currentHP)
            .FirstOrDefault();

        if (target == null)
            return;

        swallowedPlayer = target;
        devour = MaxDevour;

        float damage = target.TakeNonLethalDamage(SwallowDamage);

        target.SetTemporarilyAbsent(true);

        Debug.Log(
            $"[Grolly] {target.ChClass} 삼킴 / 피해 {damage} / 포식 {devour}"
        );
    }

    private void ResolveDigest()
    {
        if (swallowedPlayer == null)
            return;

        PlayerController target = swallowedPlayer;

        float damage = target.TakeNonLethalDamage(DigestDamage);

        owner.Heal(DigestHeal);

        Debug.Log(
            $"[Grolly] {target.ChClass} 소화 피해 {damage} / 그롤리 {DigestHeal} 회복"
        );

        ReleasePlayer();
    }

    private void ReleasePlayer()
    {
        if (swallowedPlayer == null)
            return;

        PlayerController target = swallowedPlayer;

        target.SetTemporarilyAbsent(false);

        swallowedPlayer = null;
        devour = 0;

        Debug.Log($"[Grolly] {target.ChClass} 뱉기");
    }
}