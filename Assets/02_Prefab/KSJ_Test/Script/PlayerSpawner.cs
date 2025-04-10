using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] playerSlots;

    private void Start()
    {
        var playerDatas = GameManager.Instance.playerDatas;

        for (int i = 0; i < playerDatas.Count && i < playerSlots.Length; i++)
        {
            var playerData = playerDatas[i];
            var prefab = GameManager.Instance.GetPlayerPrefab(playerData.IDNum);

            if (prefab != null)
            {
                var obj = Instantiate(prefab, playerSlots[i].position, Quaternion.identity, playerSlots[i]);

                var controller = obj.GetComponent<PlayerController>();
                if (controller != null)
                    controller.playerData = playerData;
            }
        }
    }
}
