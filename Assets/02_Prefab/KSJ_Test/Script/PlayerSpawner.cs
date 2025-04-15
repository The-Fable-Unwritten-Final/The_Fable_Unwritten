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
    [SerializeField] private RectTransform[] playerSlots; // 플레이어 생성 위치

    private void Start()
    {
        var ownedPlayerDatas = GameManager.Instance.playerDatas;

        foreach (var slot in playerSlots)
        {
            var controller = slot.GetComponent<PlayerController>();
            if (controller == null) return;

            // PlayerController 안의 playerData와 GameManager의 데이터 비교
            int id = controller.playerData.IDNum;
            var matchedData = ownedPlayerDatas.FirstOrDefault(data => data.IDNum == id);

            if (matchedData != null)
            {
                // 플레이어 보유 중일 경우 Setup 실행
                controller.Setup(matchedData);
            }
            else
            {
                // 보유하고 있지 않으면 이미지 알파값 0 처리
                var image = controller.GetComponent<Image>();
                if (image != null)
                {
                    var color = image.color;
                    color.a = 0f;
                    image.color = color;
                    image.raycastTarget = false;
                }
            }
        }
    }
}
