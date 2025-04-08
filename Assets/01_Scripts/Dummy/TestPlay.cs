using System.Collections.Generic;
using UnityEngine;

public class TestBattleRunner : MonoBehaviour
{
    public List<CardModel> testCards;

    void Start()
    {
        var players = new List<DummyPlayer>
        {
            new DummyPlayer(CharacterClass.Sophia),
            new DummyPlayer(CharacterClass.Kayla),
            new DummyPlayer(CharacterClass.Leon),
        };

        foreach (var player in players)
        {
            player.Deck.Initialize(testCards); // 모든 카드 넣기
            player.Deck.Draw(3);               // 3장 뽑기
        }

        foreach (var player in players)
        {
            Debug.Log($"{player.CharacterClass} 핸드 카드 목록:");
            foreach (var card in player.Deck.Hand)
            {
                Debug.Log($"- {card.cardName}");
                card.Play(player, player); // 대상은 자기 자신 (테스트용)
            }
        }
    }
}