using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterEventEffects : EventEffects
{
    public int battle;
    public override void Apply()
    {
        Debug.Log("EncounterEvent");

        string targetSetName = battle.ToString();
        int foundIndex = -1;

        foreach (var spawnData in DataManager.Instance.enemySpawnData)
        {
            foundIndex = spawnData.spawnSets.FindIndex(set => set.setName == targetSetName);
            if (foundIndex >= 0)
            {
                ProgressDataManager.Instance.SavedEnemySetIndex = foundIndex;
                ProgressDataManager.Instance.CurrentBattleNode.type = NodeType.NormalBattle;
                break;
            }
        }
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.CombatScene);
    }
    public override void UnApply()
    {
    }
    public override EventEffects Clone()
    {
        return new EncounterEventEffects
        {
            index = this.index,
            text = this.text,
            eventType = this.eventType,
            duration = this.duration,
            battle = this.battle
        };
    }
}
