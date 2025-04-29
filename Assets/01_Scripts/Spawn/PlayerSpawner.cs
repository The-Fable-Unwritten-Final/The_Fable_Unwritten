using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerSpawn 관리자
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] playerSlots; // 플레이어 생성 위치

    private void Start()
    {
        var ownedPlayerDatas = GameManager.Instance.playerDatas;

        foreach (var slot in playerSlots)
        {
            var controller = slot.GetComponent<PlayerController>();
            if (controller == null) return;

            var characterClass = controller.playerData.CharacterClass;

            // PlayerManager의 activePlayers에 해당 캐릭터가 있는지 확인
            if (PlayerManager.Instance.activePlayers.TryGetValue(characterClass, out var playerData))
            {                

                controller.Setup(playerData);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}
