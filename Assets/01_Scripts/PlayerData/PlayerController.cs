using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IStatusReceiver
{
    public PlayerData playerData;
    public DeckModel deckModel;
    public CharacterClass characterClass;

    public DeckModel Deck => deckModel;
    public bool IsIgnited => false;  // 점화 여부 - 추후 확장
    public string CurrentStance => playerData.currentStance.stencType.ToString();
    public CharacterClass CharacterClass => characterClass;

    public void ApplyStatusEffect(StatusEffect effect)
    {
        Debug.Log($"[버프 적용] {playerData.CharacterName} 에게 {effect.statType} +{effect.value} ({effect.duration}턴)");
        //버프 적용 로직
    }

    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        // 공격력/방어력 버프 계산
        return baseValue;
    }

    public void TakeDamage(float amount)
    {
        playerData.Health -= amount;
        Debug.Log($"{playerData.CharacterName} 피해: {amount}, 현재 체력: {playerData.Health}");
    }

    public void Heal(float amount)
    {
        playerData.Health += amount;
        Debug.Log($"{playerData.CharacterName} 회복: {amount}, 현재 체력: {playerData.Health}");
    }

    public bool IsAlive()
    {
        return playerData.Health > 0;
    }

    public void Initialize(PlayerData data, CharacterClass charClass)
    {
        playerData = data;
        characterClass = charClass;
        deckModel = new DeckModel(); // 덱은 여기서 직접 생성하거나 외부에서 주입
    }

    public void PrintDeckState()
    {
        Debug.Log($"[{playerData.CharacterName}] Hand: {Deck.Hand.Count}, Used: {Deck.UsedCount()}");
    }
}
