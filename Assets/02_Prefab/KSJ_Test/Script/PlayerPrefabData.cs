using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class PlayerPrefap
{
    public int IdNum;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "PlayerPrefabData", menuName = "PrefapData/PlayerPrefabData")]
public class PlayerPrefabData : ScriptableObject
{
    [SerializeField] private List<PlayerPrefap> playerPrefaps;

    private Dictionary<int, GameObject> prefabDict;

    private void OnEnable()
    {
        prefabDict = new();
        foreach (var prefap in playerPrefaps)
        {
            if (!prefabDict.ContainsKey(prefap.IdNum))
            {
                prefabDict.Add(prefap.IdNum, prefap.prefab);
            }
        }
    }

    public GameObject GetPrefab(int id)
    {
        prefabDict.TryGetValue(id, out var prefab);
        return prefab;
    }
}
