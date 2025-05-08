using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static CombatLightingController;

/// <summary>
/// EnemySpawn 관리자
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    //[SerializeField] Image backGround; // stage별 백그라운드 설정
    [SerializeField] GameObject background;
    [SerializeField] private Transform[] enemySlots;                    // 몬스터 생성 위치
    [SerializeField] private List<EnemyStageSpawnData> stageSpawnDatas; // 각스테이지 스폰데이터 저장 공간

    int stageIndex;
    
    private void Start()
    {
        stageIndex = ProgressDataManager.Instance.StageIndex;
        var theme = ProgressDataManager.Instance.CurrentTheme;
        var node = ProgressDataManager.Instance.CurrentBattleNode;

        //backGround.sprite = GameManager.Instance.GetBackgroundForStage(stageIndex);

        // 백그라운드 & 조명 설정
        background.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", DataManager.Instance.GetBackground(stageIndex).texture);
        GameManager.Instance.CombatLightingController.SetLighting((LightingState)(stageIndex - 1));

        stageSpawnDatas = DataManager.Instance.GetEnemySpawnData(theme, node.type);      

        var stageData = stageSpawnDatas.FirstOrDefault();

        //Debug.Log($"{theme} 테마 입니다.");

        if (stageData == null) return;

        if (stageIndex == 1)
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
        int columnIndex = ProgressDataManager.Instance.VisitedNodes.Last().columnIndex;
        int index = columnIndex - 1;

        if (index < 0 || index >= stageData.spawnSets.Count) return;

        var selectedSet = stageData.spawnSets[index];
        ApplyEnemyDataToSlots(selectedSet);
    }

    private void RandomSetting(EnemyStageSpawnData stageData)
    {
        EnemySpawnSet selectedSet;

        if (ProgressDataManager.Instance.SavedEnemySetIndex >= 0 &&
            ProgressDataManager.Instance.SavedEnemySetIndex < stageData.spawnSets.Count)
        {
            selectedSet = stageData.spawnSets[ProgressDataManager.Instance.SavedEnemySetIndex];
        }
        else
        {
            // 새로 랜덤 선택하고 저장
            selectedSet = stageData.GetRandomSet();
            int selectedIndex = stageData.spawnSets.IndexOf(selectedSet);
            ProgressDataManager.Instance.SaveEnemySetIndex(selectedIndex);
            Debug.Log($"[EnemySpawner] 랜덤으로 선택한 EnemySet 인덱스: {selectedIndex}");
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
                    copydata.UpgradeEnemybyStage(stageIndex);

                    enemy.SetData(copydata);
                    slot.gameObject.SetActive(true);


                    enemyParty.Add(enemy as IStatusReceiver);
                    continue;
                }
            }
            
            // enemy가 없거나 데이터가 없으면 null 삽입
            enemyParty.Add(null);         
        }

        for (int i = 0; i < enemyParty.Count; i++)
        {
            var enemy = enemyParty[i];

            if (enemy is Enemy e && e.enemyData != null)
            {
                Debug.Log($"[EnemyParty] Index {i}: {e.enemyData.EnemyName} (ID: {e.enemyData.IDNum})");
            }
            else
            {
                Debug.Log($"[EnemyParty] Index {i}: null");
            }
        }
    }
}
