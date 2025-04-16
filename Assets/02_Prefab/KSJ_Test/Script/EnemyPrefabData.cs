using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyPrefab 담는 SO
/// </summary>
[CreateAssetMenu(fileName = "EnemyPrefabData", menuName = "PrefapData/EnemyPrefabData")]
public class EnemyPrefabData : ScriptableObject
{
    [SerializeField] private List<GameObject> prefabs;

    private Dictionary<int, GameObject> prefabDict;

    private void OnEnable()
    {
        prefabDict = new();

        foreach (var enemyPrefap in prefabs)
        {
            if (enemyPrefap == null) return;

            var controller = enemyPrefap.GetComponent<Enemy>();
            if (controller == null || controller.enemyData == null) return;

            int id = controller.enemyData.IDNum;

            if (!prefabDict.ContainsKey(id))
            {
                prefabDict.Add(id, enemyPrefap);
            }
        }
    }

    public GameObject GetPrefab(int id)
    {
        prefabDict.TryGetValue(id, out var result);
        return result;
    }
}
