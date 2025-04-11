using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// EnemySpawn 관리자
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] enemySlots;                    // 몬스터 생성 위치
    [SerializeField] private List<EnemyStageSpawnData> stageSpawnDatas; // 각스테이지 스폰데이터 저장 공간
    [SerializeField] private EnemyPrefabData enemyPrefabData;           // 몬스터 프리팹 저장 공간

    void Start()
    {

        int currentStage = GameManager.Instance.stageIndex;
        var node = GameManager.Instance.currentBattleNode;

        var stageData = stageSpawnDatas.FirstOrDefault(x => x.stageIndex == currentStage && x.type == node.type);

        if (stageData == null) return;

        if (currentStage == 1)
        {
            FixedStage1Setting(stageData);
        }
        else
        {
            RandomSetting(stageData);
        }   
    }

    // 1스테이지 전투 세팅
    private void FixedStage1Setting(EnemyStageSpawnData stageData)
    {
        int columnIndex = GameManager.Instance.savedVisitedNodes.Last().columnIndex;

        // spawnSet는 배열이므로 0부터 확인 해야하므로 해당열에서 -1
        int index = columnIndex - 1;

        if (index < 0 || index >= stageData.spawnSets.Count) return;

        var selectedSet = stageData.spawnSets[index];
        SpawnEnemies(selectedSet);
    }

    // 렌덤세팅
    private void RandomSetting(EnemyStageSpawnData stageData)
    {
        var selectedSet = stageData.GetRandomSet();
        SpawnEnemies(selectedSet);      
    }

    // 각 슬롯에 맞는 적 스폰해주는 매서드
    private void SpawnEnemies(EnemySpawnSet selectedSet)
    {
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
