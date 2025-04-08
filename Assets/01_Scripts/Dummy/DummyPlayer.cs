using UnityEngine;

public class DummyPlayer : IStatusReceiver
{
    public CharacterClass CharacterClass { get; private set; }
    public DeckModel Deck { get; private set; } = new();
    public float Health { get; private set; } = 30;

    public bool IsIgnited => false;
    public string CurrentStance => "중단";

    public DummyPlayer(CharacterClass characterClass)
    {
        CharacterClass = characterClass;
    }

    public void ApplyStatusEffect(StatusEffect effect) => Debug.Log($"{CharacterClass}에 {effect.statType} 적용: {effect.value}");
    public float ModifyStat(BuffStatType statType, float baseValue) => baseValue; // 단순 적용
    public void TakeDamage(float amount)
    {
        Health -= amount;
        Debug.Log($"{CharacterClass} 피해: {amount}, 남은 체력: {Health}");
    }

    public void Heal(float amount)
    {
        Health += amount;
        Debug.Log($"{CharacterClass} 회복: {amount}, 현재 체력: {Health}");
    }

    public bool IsAlive() => Health > 0;
}