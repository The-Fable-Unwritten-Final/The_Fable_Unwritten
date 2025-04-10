using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] enemySlots;

    [SerializeField] private List<EnemyStageSpawnData> stageSpawnDatas;
    [SerializeField] private EnemyPrefabData enemyPrefabData;

    void Start()
    {
        int currentStage = GameManager.Instance.stageIndex;
        var stageData = stageSpawnDatas.FirstOrDefault(x => x.stageIndex == currentStage);

        if (stageData == null) return;

        var selectedSet = stageData.GetRandomSet();
        if (selectedSet == null) return;

        foreach (var slot in selectedSet.slots)
        {
            var prefab = enemyPrefabData.GetPrefab(slot.enemyId);
            if (prefab != null && slot.slotIndex < enemySlots.Length)
            {
                Instantiate(prefab, enemySlots[slot.slotIndex].position, Quaternion.identity, enemySlots[slot.slotIndex]);
            }
        }
    }
}
