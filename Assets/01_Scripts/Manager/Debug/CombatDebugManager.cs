using UnityEngine;

public class CombatDebugManager : MonoBehaviour
{
    [SerializeField] private BattleFlowController battleFlow;

    [Header("Enemy")]
    [SerializeField] private Enemy enemySlot1;
    [SerializeField] private Enemy enemySlot2;
    [SerializeField] private Enemy enemySlot3;

    public void StartDebugBattle()
    {
        if (battleFlow == null)
        {
            Debug.LogError("[CombatDebug] BattleFlowController가 없습니다.");
            return;
        }

        battleFlow.InitializeDebugPlayers();

        foreach (var player in battleFlow.playerParty)
        {
            if (player is PlayerController pc)
                Debug.Log($"[CombatDebug] {pc.ChClass} / Deck: {pc.Deck.unusedDeck.Count} / Data: {pc.playerData?.CharacterName}");
        }

        RegisterDebugEnemies();
        battleFlow.StartBattle();

        Debug.Log("[CombatDebug] 전투 초기화 완료");
    }

    private void RegisterDebugEnemies()
    {
        battleFlow.enemyParty.Clear();

        AddEnemy(enemySlot1);
        AddEnemy(enemySlot2);
        AddEnemy(enemySlot3);

        while (battleFlow.enemyParty.Count < 3)
            battleFlow.enemyParty.Add(null);
    }

    private void AddEnemy(Enemy enemy)
    {
        if (enemy == null || enemy.enemyData == null)
            return;

        var data = enemy.enemyData;

        enemy.SetData(enemy.enemyData);

        Debug.Log(
                $"[DebugEnemyPos] {enemy.name}" +
                $" / Root World:{enemy.transform.position}" +
                $" / Root Local:{enemy.transform.localPosition}" +
                $" / Sprite World:{enemy.spriteRenderer.transform.position}" +
                $" / Sprite Local:{enemy.spriteRenderer.transform.localPosition}"
            );

        battleFlow.enemyParty.Add(enemy);
    }

}