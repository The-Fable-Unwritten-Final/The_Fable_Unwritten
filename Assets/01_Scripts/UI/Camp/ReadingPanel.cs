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
    [SerializeField] GameObject cardBook;
    [SerializeField] Transform cardsRoot;
    [SerializeField] int chageCardExp = 5;

    public int currentCardIndex;
    public int chageCardIndex;
    CharacterClass selectClass;

    public void OnSophiaClicked() => ShowCards(CharacterClass.Sophia);
    public void OnKaylaClicked() => ShowCards(CharacterClass.Kayla);
    public void OnLeonClicked() => ShowCards(CharacterClass.Leon);

    // 카드 외 선택 시 CardPanel 비활성화
    public void OnClickCardExept()
    {
        changeCardPanel.SetActive(false);
    }

    // 카드 외 선택 시 CardPanel 비활성화
    public void OnClickBookExept()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.UI, 1); // 책 비활성화 시 사운드
        cardBook.SetActive(false);
    }

    /// <summary>
    /// 선택한 캐릭터의 현재 보유중인 카드 보여주기
    /// </summary>
    public void ShowCards(CharacterClass characterClass)
    {
        changeCardPanel.SetActive(true);

        ClearCards();

        selectClass = characterClass;
        var deck = CurrentCharacterDeck(characterClass);

        foreach (var card in deck)
        {
            var go = Instantiate(campCardPrefap, cardsRoot);
            go.AddComponent<CursorHoverHandler>();
            var cardUI = go.GetComponent<CampCard>();
            var onclick = go.GetComponent<Button>();

            cardUI.SetCard(card);
            onclick.onClick.AddListener(() => ShowBook(characterClass, card));
        }

        // 애널리틱스
        GameManager.Instance.analyticsLogger.LogCampActInfo((int)characterClass + 1);
    }

    // 현재 캐릭터의 보유 카드 확인
    private List<CardModel> CurrentCharacterDeck(CharacterClass characterClass)
    {
        var player = ProgressDataManager.Instance.PlayerDatas
            .FirstOrDefault(p => p.CharacterClass == characterClass);

        if (player == null) return new();

        var allCards = DataManager.Instance.AllCards;

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

    private void ShowBook(CharacterClass characterClass, CardModel selectCard)
    {
        SoundManager.Instance.PlaySFX(SoundCategory.UI, 0); // 책 비활성화 시 사운드
        currentCardIndex = selectCard.index;
        cardBook.GetComponent<CampCardBook>().Character = characterClass;
        cardBook.SetActive(true);

       // 카드 중복 보유 방지 코드 (기획자분 확인 중)
       // var player = GameManager.Instance.playerDatas
       //.FirstOrDefault(p => p.CharacterClass == characterClass);

       // if (player == null) return;

       // var currentDeck = player.currentDeckIndexes; // 현재 보유 카드 인덱스들

       // var bookCards = cardBook.GetComponentsInChildren<BookCards>();

       // foreach (var bookCard in bookCards)
       // {
       //     var button = bookCard.GetComponent<Button>();
       //     if (button == null) continue;

       //     // 현재 덱에 이미 들어 있는 카드라면 클릭 불가
       //     if (currentDeck.Contains(bookCard.cardIndex))
       //         button.interactable = false;
       //     else
       //         button.interactable = true;                      
       // }
    }

    public void SetChangeCardIndex(Transform selectCard)
    {
        if (selectCard.TryGetComponent<BookCards>(out var bookCard))
        {
            chageCardIndex = bookCard.cardIndex;

            // 변경 카드 두개가 같거나 사용 경험치가 부족하다면 실행x
            if (chageCardIndex == currentCardIndex || ProgressDataManager.Instance.CurrentExp < chageCardExp
                || chageCardIndex == 0) return;            

            var player = ProgressDataManager.Instance.PlayerDatas
          .FirstOrDefault(p => p.CharacterClass == cardBook.GetComponent<CampCardBook>().Character);

            // 인덱스 위치 찾아서 교체
            int targetIndex = player.currentDeckIndexes.IndexOf(currentCardIndex);
            if (targetIndex != -1)
            {
                player.currentDeckIndexes[targetIndex] = chageCardIndex;
                //Debug.Log($"[Card Swap] {currentCardIndex} → {chageCardIndex}로 교체 완료");

                changeCardPanel.SetActive(false);
                cardBook.SetActive(false);
            }
            SoundManager.Instance.PlaySFX(SoundCategory.Player, (int)selectClass);
            ProgressDataManager.Instance.CurrentExp -= chageCardExp;

            // 애널리틱스
            GameManager.Instance.analyticsLogger.LogAddedCardInfo(chageCardIndex);
        }
    }

}
