using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerSpawn 관리자
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] playerSlots; // 플레이어 생성 위치

    private void Start()
    {
        var playerDatas = GameManager.Instance.playerDatas; // 게임매니저에서 현재 플레이어가 보유하고 있는 캐릭터 확인

        for (int i = 0; i < playerDatas.Count && i < playerSlots.Length; i++)
        {
            var playerData = playerDatas[i];
            var prefab = GameManager.Instance.GetPlayerPrefab(playerData.IDNum);

            if (prefab != null)
            {
                var obj = Instantiate(prefab, playerSlots[i].position, Quaternion.identity, playerSlots[i]);

                var controller = obj.GetComponent<PlayerController>();
                if (controller != null) controller.playerData = playerData;
            }
        }
    }
}
