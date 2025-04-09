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

    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    public void ApplyStatusEffect(StatusEffect effect)
    {
        Debug.Log($"[버프 적용] {playerData.CharacterName} 에게 {effect.statType} +{effect.value} ({effect.duration}턴)");
        activeEffects.Add(new StatusEffect
        {
            statType = effect.statType,
            value = effect.value,
            duration = effect.duration
        });
    }

    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        float modifiedValue = baseValue;
        foreach (var effect in activeEffects)
        {
            if (effect.statType == statType)
                modifiedValue += effect.value;
        }
        return modifiedValue;
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

    public void TickStatusEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].duration--;
            if (activeEffects[i].duration <= 0)
            {
                Debug.Log($"[버프 종료] {playerData.CharacterName} 의 {activeEffects[i].statType} 효과 종료");
                activeEffects.RemoveAt(i);
            }
        }
    }

    public bool HasEffect(BuffStatType type)
    {
        return activeEffects.Exists(e => e.statType == type && e.duration > 0);
    }

    public bool IsStunned() => HasEffect(BuffStatType.stun);
}
