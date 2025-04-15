using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test_AddPlayer : MonoBehaviour
{
    public void AddLeon()
    {
        TryAddPlayer(CharacterClass.Leon, "LeonPlayer");
        Debug.Log($"{PlayerManager.Instance.activePlayers.Count} 명 보유 중");
    }

    public void AddSopia()
    {
        TryAddPlayer(CharacterClass.Sophia, "SopiaPlayer");
        Debug.Log($"{PlayerManager.Instance.activePlayers.Count} 명 보유 중");
    }

    public void AddKayla()
    {
        TryAddPlayer(CharacterClass.Kayla, "KaylaPlayer");
        Debug.Log($"{PlayerManager.Instance.activePlayers.Count} 명 보유 중");
    }

    private void TryAddPlayer(CharacterClass chClass, string name)
    {
        var playerDatas = GameManager.Instance.playerDatas;

        var targetData = playerDatas.Find(p => p.CharacterClass == chClass);
        if (targetData == null) return;

        var obj = new GameObject(name);
        var controller = obj.AddComponent<PlayerController>();

        PlayerManager.Instance.AddPlayerDuringGame(
            controller,
            targetData,
            CardSystemInitializer.Instance.loadedCards
        );

        Debug.Log($"{chClass} 등록 완료");
    }
}
