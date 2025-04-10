using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySlotData
{
    public int slotIndex; 
    public int enemyId;
}

[Serializable]
public class EnemySpawnSet
{
    public string setName;
    public List<EnemySlotData> slots;
}

[CreateAssetMenu(fileName = "EnemyStageSpawnData", menuName = "SpawnData/EnemyStageSpawnData")]
public class EnemyStageSpawnData : ScriptableObject
{
    public int stageIndex;
    public List<EnemySpawnSet> spawnSets;

    public EnemySpawnSet GetRandomSet()
    {
        if (spawnSets == null || spawnSets.Count == 0)
            return null;

        var selected = spawnSets[UnityEngine.Random.Range(0, spawnSets.Count)];

        Debug.Log($"[EnemySpawnSet] Stage {stageIndex}에서 '{selected.setName}' 세트 선택됨");

        return selected;
    }
}
