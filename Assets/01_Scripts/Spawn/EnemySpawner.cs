using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EnemySpawn 관리자
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    //[SerializeField] Image backGround; // stage별 백그라운드 설정
    [SerializeField] private Transform[] enemySlots;                    // 몬스터 생성 위치
    [SerializeField] private List<EnemyStageSpawnData> stageSpawnDatas; // 각스테이지 스폰데이터 저장 공간

    int stageIndex;
    
    private void Start()
    {
        stageIndex = ProgressDataManager.Instance.StageIndex;
        var theme = ProgressDataManager.Instance.CurrentTheme;
        var node = ProgressDataManager.Instance.CurrentBattleNode;

        stageSpawnDatas = DataManager.Instance.GetEnemySpawnData(theme, node.type);      

        var stageData = stageSpawnDatas.FirstOrDefault();

        if (stageData == null) return;

        if (stageIndex == 1)
        {
            FixedStage1Setting(stageData);
        }
        else
        {
            RandomSetting(stageData);
        }

        ProgressDataManager.Instance.SaveProgress();
    }


    private void FixedStage1Setting(EnemyStageSpawnData stageData)
    {
        var currentNode = ProgressDataManager.Instance.CurrentBattleNode;
        if (currentNode == null) return;

        int columnIndex = currentNode.columnIndex;
        int index = columnIndex - 1;

        if (index < 0 || index >= stageData.spawnSets.Count) return;

        var selectedSet = stageData.spawnSets[index];
        ApplyEnemyDataToSlots(selectedSet);
    }

    private void RandomSetting(EnemyStageSpawnData stageData)
    {
        EnemySpawnSet selectedSet;

        if (ProgressDataManager.Instance.SavedEnemySetIndex > 0)
        {
            selectedSet = stageData.spawnSets[ProgressDataManager.Instance.SavedEnemySetIndex];
        }
        else
        {
            // 새로 랜덤 선택하고 저장
            
            selectedSet = stageData.GetRandomSet();
            int selectedIndex = stageData.spawnSets.IndexOf(selectedSet);
            ProgressDataManager.Instance.SaveEnemySetIndex(selectedIndex);
        }
        ApplyEnemyDataToSlots(selectedSet);
    }

    /// <summary>
    /// 슬롯에 EnemyData를 적용하고 빈슬롯 비활성화
    /// </summary>
    private void ApplyEnemyDataToSlots(EnemySpawnSet selectedSet)
    {
        var enemyParty = GameManager.Instance.turnController.battleFlow.enemyParty;
        EnemyDataContainer enemyDataContainer = DataManager.Instance.enemyDataContainer;
        // slot 맵핑
        Dictionary<int, EnemySlotData> slotMap = new();

        // slotMap 각 슬롯 등록
        foreach (var slotData in selectedSet.slots)
        {
            slotMap[slotData.slotIndex] = slotData;
        }

        // InGameSlot & BattleFlow 정보 등록
        for (int i = 0; i < enemySlots.Length; i++)
        {
            var slot = enemySlots[i];
            slot.gameObject.SetActive(false); // 기본 비활성화

            if (slotMap.TryGetValue(i, out var slotData))
            {
                var enemy = slot.GetComponent<Enemy>();
                var origndata = enemyDataContainer.GetData(slotData.enemyId);

                if (enemy != null && origndata != null)
                {
                    var copydata = ScriptableObject.Instantiate(origndata);
                    copydata.SkillList = origndata.SkillList.Select(skill => skill.Clone()).ToList();
                    //copydata.UpgradeEnemybyStage(stageIndex);     //스테이지에 따라 HP 성장 계수 추가

                    enemy.SetData(copydata);
                    slot.gameObject.SetActive(true);


                    enemyParty[i] = enemy as IStatusReceiver;
                    continue;
                }
            }
            
            // enemy가 없거나 데이터가 없으면 null 삽입
            enemyParty[i] = null;         
        }

        for (int i = 0; i < enemyParty.Count; i++)
        {
            var enemy = enemyParty[i];

            if (enemy is Enemy e && e.enemyData != null)
            {
                //Debug.Log($"[EnemyParty] Index {i}: {e.enemyData.EnemyName} (ID: {e.enemyData.IDNum})");
            }
            else
            {
               // Debug.Log($"[EnemyParty] Index {i}: null");
            }
        }
    }
}
