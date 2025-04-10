using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (controller == null || controller.EnemyData == null) return;

            int id = controller.EnemyData.IDNum;

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
