using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyPrefab
{
    public int IdNum;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "EnemyPrefabData", menuName = "PrefapData/EnemyPrefabData")]
public class EnemyPrefabData : ScriptableObject
{
    [SerializeField] private List<EnemyPrefab> prefabs;
    private Dictionary<int, GameObject> prefabDict;

    private void OnEnable()
    {
        prefabDict = new();
        foreach (var p in prefabs)
            if (!prefabDict.ContainsKey(p.IdNum))
                prefabDict.Add(p.IdNum, p.prefab);
    }

    public GameObject GetPrefab(int id)
    {
        prefabDict.TryGetValue(id, out var result);
        return result;
    }
}
