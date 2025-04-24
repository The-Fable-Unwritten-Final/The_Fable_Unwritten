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
            if (PlayerManager.Instance.GetPlayerData(characterClass) != null)
            {
                // 셋업 해주기
                var data = PlayerManager.Instance.GetPlayerData(characterClass);
                controller.Setup(data);
            }
            else
            {
                slot.gameObject.SetActive(false);

                // 슬롯 비활성화로 변경
                //// 없으면 비활성화 처리 (알파값 0)
                //var sprite = slot.GetComponent<SpriteRenderer>();
                //if (sprite != null)
                //{
                //    var color = sprite.color;
                //    color.a = 0f;
                //    sprite.color = color;
                //}
            }
        }
    }
}
