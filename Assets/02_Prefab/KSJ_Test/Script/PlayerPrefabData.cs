using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// PlayerPrefab 담는 SO
/// </summary>
[CreateAssetMenu(fileName = "PlayerPrefabData", menuName = "PrefapData/PlayerPrefabData")]
public class PlayerPrefabData : ScriptableObject
{
    [SerializeField] private List<GameObject> playerPrefaps;

    private Dictionary<int, GameObject> prefabDict;

    private void OnEnable()
    {
        prefabDict = new();
        
        foreach (var playerPrefap in playerPrefaps)
        {
            if (playerPrefap == null)
                return;

            var controller = playerPrefap.GetComponent<PlayerController>();
            if (controller == null || controller.playerData == null)
                return;

            int id = controller.playerData.IDNum;

            if (!prefabDict.ContainsKey(id))
            {
                prefabDict.Add(id, playerPrefap);
            }
        }
    }

    public GameObject GetPrefab(int id)
    {
        prefabDict.TryGetValue(id, out var prefab);
        return prefab;
    }
}
