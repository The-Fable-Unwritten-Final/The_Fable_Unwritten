using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerInfo : MonoBehaviour
{
    [Header("Info")]

    [SerializeField] Image shopiaHpBar;
    [SerializeField] TextMeshProUGUI sophiaHp;
    [SerializeField] Image kylaHpBar;
    [SerializeField] TextMeshProUGUI kylaHp;
    [SerializeField] Image leonHpBar;
    [SerializeField] TextMeshProUGUI leonHp;
    [SerializeField] TextMeshProUGUI currentExp;

    [Header("EliteClearInfo")]
    [SerializeField] GameObject courageBadge;
    [SerializeField] GameObject loveBadge;
    [SerializeField] GameObject wisdomBadge;

    [Header("CardInfo")]
    [SerializeField] GameObject currentDeck;
    [SerializeField] GameObject cardPrefap;
    [SerializeField] Transform cardsRoot;

    private Dictionary<CharacterClass, TextMeshProUGUI> charInfoText;
    private Dictionary<CharacterClass, Image> charhpBar;
    private Coroutine changeHpCoroutine_Sho;
    private Coroutine changeHpCoroutine_Ky;
    private Coroutine changeHpCoroutine_Le;

    private void Start()
    {
        charInfoText = new Dictionary<CharacterClass, TextMeshProUGUI>
        {
            { CharacterClass.Sophia, sophiaHp },
            { CharacterClass.Kayla, kylaHp },
            { CharacterClass.Leon, leonHp }
        };

        charhpBar = new Dictionary<CharacterClass, Image>
        {
            { CharacterClass.Sophia, shopiaHpBar },
            { CharacterClass.Kayla, kylaHpBar },
            { CharacterClass.Leon, leonHpBar }
        };

        SetEndingBadge();
        UpdatePlayerInfoUI();

        RegisterHpUpdateEvent();
    }

    void OnDisable()
    {
        foreach (var kvp in PlayerManager.Instance.activePlayers)
        {
            CharacterClass character = kvp.Key;
            PlayerData playerData = kvp.Value;

            if (charInfoText.ContainsKey(character))
            {
                // 체력 변경 이벤트 해제
                if(playerData != null)
                playerData.OnHpChanged -= (currentHp, maxHp) =>
                {
                    charInfoText[character].text = $"{currentHp}/{maxHp}";
                    ChangeHpBar(charhpBar[character], currentHp, maxHp, character);
                };
            }
        }
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
                // 초기 체력 설정 (애니메이션 없이 바로 적용)
                if (charhpBar.TryGetValue(character, out var hpBar))
                {
                    ChangeHpBar(hpBar, playerData.currentHP, playerData.MaxHP, character, animate: false);
                }

                // 체력 변경 이벤트 등록 (애니메이션 포함)
                playerData.OnHpChanged += (currentHp, maxHp) =>
                {
                    textObj.text = $"{currentHp}/{maxHp}";
                    ChangeHpBar(charhpBar[character], currentHp, maxHp, character, animate: true);
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
        OnClickButtonSound();
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

        // 애널리틱스
        GameManager.Instance.analyticsLogger.LogDeckButtonClick((int)characterClass + 1);
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

    public void UpdatePlayerInfoUI()
    {
        var players = PlayerManager.Instance.activePlayers;
        var exp = ProgressDataManager.Instance.CurrentExp;

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

        currentExp.text = exp.ToString();
    }

    public void ChangeHpBar(Image hpBar, float hp, float maxHp, CharacterClass character, bool animate = true)
    {
        if (hpBar == null) return;

        float targetFill = hp / maxHp;

        // 애니메이션 없이 바로 적용
        if (!animate)
        {
            hpBar.fillAmount = targetFill;
            return;
        }

        // 캐릭터별 코루틴 선택 및 중지
        Coroutine currentCoroutine = character switch
        {
            CharacterClass.Sophia => changeHpCoroutine_Sho,
            CharacterClass.Kayla => changeHpCoroutine_Ky,
            CharacterClass.Leon => changeHpCoroutine_Le,
            _ => null
        };

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        // 새 코루틴 시작 및 저장
        var newCoroutine = StartCoroutine(AnimateHpBarChange(hpBar, targetFill, 0.4f));
        
        switch (character)
        {
            case CharacterClass.Sophia:
                changeHpCoroutine_Sho = newCoroutine;
                break;
            case CharacterClass.Kayla:
                changeHpCoroutine_Ky = newCoroutine;
                break;
            case CharacterClass.Leon:
                changeHpCoroutine_Le = newCoroutine;
                break;
        }
    }

    private IEnumerator AnimateHpBarChange(Image hpBar,float targetFill, float duration)
    {
        float startFill = hpBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            hpBar.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }

        hpBar.fillAmount = targetFill;
    }


    private void SetEndingBadge()
    {
        var stageSetting = ProgressDataManager.Instance;

        courageBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Courage));
        loveBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Love));
        wisdomBadge.SetActive(stageSetting.IsEliteClear(StageTheme.Wisdom));
    }
    public void OnClickButtonSound()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
    }
}
