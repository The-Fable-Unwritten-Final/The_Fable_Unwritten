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

/// <summary>
/// Stage별 EnemySet 데이터 보관용 SO (NormalBattle)
/// </summary>
[CreateAssetMenu(fileName = "EnemyStageSpawnData", menuName = "SpawnData/EnemyStageSpawnData")]
public class EnemyStageSpawnData : ScriptableObject
{
    public int stageIndex;
    public NodeType type;
    public List<EnemySpawnSet> spawnSets;

    /// <summary>
    /// 랜덤한 EnemySet 가져오는 매서드
    /// </summary>
    public EnemySpawnSet GetRandomSet()
    {
        if (spawnSets == null || spawnSets.Count == 0) return null;

        var selected = spawnSets[UnityEngine.Random.Range(0, spawnSets.Count)];

        Debug.Log($"[EnemySpawnSet] Stage {stageIndex}에서 '{selected.setName}' 세트 선택됨");

        return selected;
    }
}
