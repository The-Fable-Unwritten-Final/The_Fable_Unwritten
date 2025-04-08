using System.Collections.Generic;
using UnityEngine;

public class BattleFlowController : MonoBehaviour
{
    public Dictionary<CharacterClass, DeckModel> decksByCharacter = new();
    public List<IStatusReceiver> playerParty;
    public List<IStatusReceiver> enemyParty;

    private int currentMana;
    public int startMana = 3;

    private Dictionary<CharacterClass, IStatusReceiver> characterMap = new();

    void Start()
    {
        CacheCharacters();
        StartBattle();
    }

    private void CacheCharacters()
    {
        characterMap.Clear();
        foreach (var member in playerParty)
        {
            characterMap[member.CharacterClass] = member;
        }
    }

    public void StartBattle()
    {
        currentMana = startMana;

        foreach (CharacterClass character in System.Enum.GetValues(typeof(CharacterClass)))
        {
            var deck = new DeckModel();
            deck.Initialize(GetInitialCards(character));

            if (GetCharacterByClass(character)?.IsAlive() == true)
            {
                deck.Draw(3);
            }

            decksByCharacter[character] = deck;
        }
    }

    private IStatusReceiver GetCharacterByClass(CharacterClass character)
    {
        return characterMap.TryGetValue(character, out var receiver) ? receiver : null;
    }

    public void UseCard(CardModel card, IStatusReceiver caster, IStatusReceiver target)
    {
        if (!card.IsUsable(currentMana) || caster == null || target == null || !caster.IsAlive() || !target.IsAlive())
            return;

        currentMana -= card.manaCost;
        card.Play(caster, target);

        if (decksByCharacter.TryGetValue(card.characterClass, out var deck))
            deck.Discard(card);
    }

    public IReadOnlyList<CardModel> GetHandByCharacter(CharacterClass character)
    {
        return decksByCharacter.TryGetValue(character, out var deck) ? deck.Hand : null;
    }

    public void RestoreMana() => currentMana = startMana;

    public void DrawHands()
    {
        foreach (var character in decksByCharacter.Keys)
        {
            var receiver = GetCharacterByClass(character);
            if (receiver != null && receiver.IsAlive())
            {
                var deck = decksByCharacter[character];
                int currentHandSize = deck.Hand.Count;

                if(currentHandSize < DeckModel.startSize)
                {
                    int cardstoDraws = DeckModel.startSize - currentHandSize;
                    deck.Draw(cardstoDraws);
                }
            }
        }
    }

    public void DiscardHandsToLimit()
    {
        foreach (var deck in decksByCharacter.Values)
            deck.DiscardHandToThree();
    }

    public void ExecuteEnemyAI()
    {
        foreach (var enemy in enemyParty)
        {
            if (!enemy.IsAlive()) continue;

            //적 관련 로직
        }
    }

    private CardModel GetEnemySkill(IStatusReceiver enemy)
    {
        return null;
    }

    private List<CardModel> GetInitialCards(CharacterClass character)
    {
        return new List<CardModel>();       //시작 카드 초기화하기
    }
}