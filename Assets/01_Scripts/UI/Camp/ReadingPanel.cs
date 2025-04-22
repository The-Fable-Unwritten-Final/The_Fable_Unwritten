using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReadingPanel : BaseCampPanel
{
    [Header("ReadingCardPanel")]
    [SerializeField] GameObject changeCardPanel;

    [Header("CardInfo")]
    [SerializeField] GameObject campCardPrefap;
    [SerializeField] Transform cardsRoot;

    public void OnSophiaClicked() => ShowCards(CharacterClass.Sophia);
    public void OnKaylaClicked() => ShowCards(CharacterClass.Kayla);
    public void OnLeonClicked() => ShowCards(CharacterClass.Leon);

    // 카드 외 선택 시 CardPanel 비활성화
    public void OnClickCardExept()
    {
        changeCardPanel.SetActive(false);
    }

    /// <summary>
    /// 선택한 캐릭터의 현재 보유중인 카드 보여주기
    /// </summary>
    public void ShowCards(CharacterClass characterClass)
    {
        changeCardPanel.SetActive(true);

        ClearCards();

        var deck = CurrentCharacterDeck(characterClass);

        foreach (var card in deck)
        {
            var go = Instantiate(campCardPrefap, cardsRoot);
            var cardUI = go.GetComponent<CampCard>();
            var onclick = go.GetComponent<Button>();

            cardUI.SetCard(card);
            onclick.onClick.AddListener(() => ShowBook(characterClass));
   
        }
    }

    // 현재 캐릭터의 보유 카드 확인
    private List<CardModel> CurrentCharacterDeck(CharacterClass characterClass)
    {
        var player = GameManager.Instance.playerDatas
            .FirstOrDefault(p => p.CharacterClass == characterClass);

        if (player == null) return new();

        var allCards = CardSystemInitializer.Instance.loadedCards;

        return player.currentDeckIndexes
            .Select(i => allCards.FirstOrDefault(c => c.index == i))
            .Where(c => c != null)
            .ToList();
    }

    private void ClearCards()
    {
        foreach (Transform child in cardsRoot)
            Destroy(child.gameObject);
    }

    private void ShowBook(CharacterClass characterClass)
    {
        UIManager.Instance.ShowPopupByName("PopupUI_Book");
    }
}
