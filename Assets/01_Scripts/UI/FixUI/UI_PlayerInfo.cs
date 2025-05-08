using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerInfo : MonoBehaviour
{
    [Header("UIInfo")]
    [SerializeField] TextMeshProUGUI sophiaInfo;
    [SerializeField] TextMeshProUGUI kaylaInfo;
    [SerializeField] TextMeshProUGUI leonInfo;
    [SerializeField] TextMeshProUGUI currentIndfo;

    [Header("EliteClearInfo")]
    [SerializeField] GameObject courageBadge;
    [SerializeField] GameObject loveBadge;
    [SerializeField] GameObject wisdomBadge;

    [Header("CardInfo")]
    [SerializeField] GameObject currentDeck;
    [SerializeField] GameObject cardPrefap;
    [SerializeField] Transform cardsRoot;

    private Dictionary<CharacterClass, TextMeshProUGUI> charInfoText;

    private void Start()
    {
        charInfoText = new Dictionary<CharacterClass, TextMeshProUGUI>
        {
            { CharacterClass.Sophia, sophiaInfo },
            { CharacterClass.Kayla, kaylaInfo },
            { CharacterClass.Leon, leonInfo }
        };

        SetEndingBadge();
        UPdatePlayerInfoUI();

        RegisterHpUpdateEvent();
    }

    private void RegisterHpUpdateEvent()
    {
        var players = PlayerManager.Instance.activePlayers;

        foreach (var kvp in players)
        {
            CharacterClass character = kvp.Key;
            PlayerData playerData = kvp.Value;

            if (charInfoText.TryGetValue(character, out var textObj))
            {
                // 체력 변경 이벤트 등록
                playerData.OnHpChanged += (currentHp, maxHp) =>
                {
                    textObj.text = $"{currentHp}/{maxHp}";
                };
            }
        }
    }

    public void OnSophiaClicked() => ShowCards(CharacterClass.Sophia);
    public void OnKaylaClicked() => ShowCards(CharacterClass.Kayla);
    public void OnLeonClicked() => ShowCards(CharacterClass.Leon);

    // 카드 외 선택 시 CardPanel 비활성화
    public void OnClickCardExept()
    {
        currentDeck.SetActive(false);
    }

    /// <summary>
    /// 선택한 캐릭터의 현재 보유중인 카드 보여주기
    /// </summary>
    public void ShowCards(CharacterClass characterClass)
    {
        currentDeck.SetActive(true);

        ClearCards();

        var deck = CurrentCharacterDeck(characterClass);

        foreach (var card in deck)
        {
            var go = Instantiate(cardPrefap, cardsRoot);
            var cardUI = go.GetComponent<CampCard>();
            var onclick = go.GetComponent<Button>();

            cardUI.SetCard(card);
        }
    }

    // 현재 캐릭터의 보유 카드 확인
    private List<CardModel> CurrentCharacterDeck(CharacterClass characterClass)
    {
        var player = GameManager.Instance.playerDatas
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

    public void UPdatePlayerInfoUI()
    {
        var players = PlayerManager.Instance.activePlayers;

        foreach (var text in charInfoText)
        {
            var character = text.Key;
            var textObj = text.Value;

            bool hasPlayer = players.TryGetValue(character, out var playerData);

            // 부모 오브젝트 활성/비활성
            textObj.transform.parent.gameObject.SetActive(hasPlayer);

            if (hasPlayer)
            {
                textObj.text = $"{playerData.currentHP}/{playerData.MaxHP}";
            }
        }
    }

    private void SetEndingBadge()
    {
        var stageSetting = ProgressDataManager.Instance;

        courageBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Courage));
        loveBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Love));
        wisdomBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Wisdom));
    }
}
