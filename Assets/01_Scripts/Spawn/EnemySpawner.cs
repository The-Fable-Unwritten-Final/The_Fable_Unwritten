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
    [SerializeField] private EnemyDataContainer enemyDataContainer;     // 몬스터 프리팹 저장 공간

    private void Start()
    {
        stageSpawnDatas = GameManager.Instance.enemyStageSpawnDatas;
        int currentStage = GameManager.Instance.stageIndex;
        var node = GameManager.Instance.currentBattleNode;

        var stageData = stageSpawnDatas
            .FirstOrDefault(x => x.stageIndex == currentStage && x.type == node.type);

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

    private void FixedStage1Setting(EnemyStageSpawnData stageData)
    {
        int columnIndex = GameManager.Instance.savedVisitedNodes.Last().columnIndex;
        int index = columnIndex - 1;

        if (index < 0 || index >= stageData.spawnSets.Count) return;

        var selectedSet = stageData.spawnSets[index];
        ApplyEnemyDataToSlots(selectedSet);
    }

    private void RandomSetting(EnemyStageSpawnData stageData)
    {
        var selectedSet = stageData.GetRandomSet();
        ApplyEnemyDataToSlots(selectedSet);
    }

    /// <summary>
    /// 슬롯에 EnemyData를 적용하고 시각적으로 알파값 처리까지 수행
    /// </summary>
    private void ApplyEnemyDataToSlots(EnemySpawnSet selectedSet)
    {
        foreach (var slot in enemySlots)
        {
            slot.gameObject.SetActive(false);
        }

        // Step 2. 실제 몬스터 데이터 설정
        foreach (var slotData in selectedSet.slots)
        {
            if (slotData.slotIndex >= enemySlots.Length) continue;

            var targetSlot = enemySlots[slotData.slotIndex];
            var enemy = targetSlot.GetComponent<Enemy>();
            var data = enemyDataContainer.GetData(slotData.enemyId);

            if (enemy != null && data != null)
            {
                enemy.enemyData = data;
            }
            targetSlot.gameObject.SetActive(true);
        }
    }
}
