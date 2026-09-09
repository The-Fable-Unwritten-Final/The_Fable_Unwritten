using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupUI_Book : BasePopupUI
{
    [SerializeField] List<Transform> bookClip;
    [SerializeField] List<RectTransform> pages; // 0: 카드 페이지, 1 : 이상 실현 페이지, 2 : 일기 페이지 3: 카드 팝업 페이지
    [SerializeField] List<PlayerData> playerDatas; // 플레이어 데이터들 0 : 소피아, 1 : 카일라, 2 : 레온
    [SerializeField] List<Image> characterButtons; // 캐릭터 버튼 3개 (소피아, 카일라, 레온)

    int currentPageType = 0; // 현재 페이지
    int currentCharacterIndex = 0; // 현재 활성화된 캐릭터 인덱스

    private void Awake()
    {
        playerDatas = ProgressDataManager.Instance.PlayerDatas;// 플레이어 데이터 받아오기.
    }


    private void OnEnable()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.UI, 0); // 책 활성화 시 사운드
        // 처음 책을 열었을때 최초 페이지 == 0번 인덱스의 카드페이지
        OnClick0();
        currentPageType = 0; // 패이지 초기화.

        // 애널리틱스
        GameManager.Instance.analyticsLogger.LogBookButtonClick();
    }

    private void OnDisable()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.UI, 1); // 책 비활성화 시 사운드
    }

    // 클릭한 책갈피가 펼쳐줄 캐릭터 카드 종류(해당 데이터를 cardPage 에서 받아서 보여줄 카드 업데이트) 1: 소피아, 2: 카일라, 3: 레온
    public void OnClick0()
    {
        currentCharacterIndex = 0;
        UpdateCharacterButtonColors();
        OnClickClip(bookClip[0], 0);
    }
    public void OnClick1()
    {
        currentCharacterIndex = 1;
        UpdateCharacterButtonColors();
        OnClickClip(bookClip[1], 1);
    }
    public void OnClick2()
    {
        currentCharacterIndex = 2;
        UpdateCharacterButtonColors();
        OnClickClip(bookClip[2], 2);
    }
    void OnClickClip(Transform t, int i)
    {
        //SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        SetAllPageClose(); // 모든 페이지 비활성화
        pages[0].gameObject.SetActive(true); // 카드 페이지 활성화
        pages[0].GetComponent<CardBook>().CardsSet(i); // i번 캐릭터에 맞는 카드 페이지 init.
        //SetLastSibling(t); // 클릭한 책갈피를 가장 위로 올림
    }// i번 캐릭터에 맞는 카드 페이지 펼치기 래핑. (i == 0 : 소피아, 1 : 카일라, 2: 레온)

    public void OnClickIdle(Transform t)// 이상 실현 버튼
    {
        currentPageType = 1;
        //SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        SetAllPageClose(); // 모든 페이지 비활성화
        pages[1].gameObject.SetActive(true); // 이상 실현 페이지 활성화
        // 이상 실현 페이지 초기화.
        //SetLastSibling(t);
    }
    public void OnClickDiary()// 일기장 버튼
    {
        currentPageType = 2;
        SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        SetAllPageClose(); // 모든 페이지 비활성화
        pages[2].gameObject.SetActive(true); // 일기장 페이지 활성화
        // 일기장 페이지 초기화.
    }
    public void OnClickLeftArrow()
    {
        pages[currentPageType].GetComponent<IBookControl>().OnclickPageBefore();
    }// 왼쪽 화살표 클릭시 페이지 넘기기
    public void OnClickRightArrow()
    {
        pages[currentPageType].GetComponent<IBookControl>().OnclickPageAfter();
    }// 오른쪽 화살표 클릭시 페이지 넘기기

    // 카드 북 버튼 소리 처리(이상현실 제외)
    public void OnClickBookButton()
    {
        SoundManager.Instance.PlaySFX(SoundCategory.Button, 2);
    }

    public void FilterByCardType(int cardTypeIndex)
    {
        CardType selectedType = (CardType)cardTypeIndex;
        Dictionary<int, CardModel> currentCharacterCards = GetCurrentCharacterCards();

        // 현재 캐릭터가 해당 타입의 카드를 가지고 있는지 확인
        bool hasType = currentCharacterCards.Values.Any(card => card.type == selectedType);

        if (!hasType)
        {
            // 해당 타입을 가진 캐릭터 찾기
            int targetCharacterIndex = FindCharacterWithCardType(selectedType);
            if (targetCharacterIndex < 0)
            {
                return; // 타입을 가진 캐릭터 없음
            }
            // 페이지 전환 (이미 CardBook의 CardsSet을 호출함)
            switch (targetCharacterIndex)
            {
                case 0:
                    OnClick0();
                    break;
                case 1:
                    OnClick1();
                    break;
                case 2:
                    OnClick2();
                    break;
            }
        }

        // CardBook에 필터링 적용
        pages[0].GetComponent<CardBook>().ApplyCardTypeFilter(cardTypeIndex);
    }

    Dictionary<int, CardModel> GetCurrentCharacterCards()
    {
        return currentCharacterIndex switch
        {
            0 => (Dictionary<int, CardModel>)DataManager.Instance.CardForShopia,
            1 => (Dictionary<int, CardModel>)DataManager.Instance.CardForKayla,
            2 => (Dictionary<int, CardModel>)DataManager.Instance.CardForLeon,
            _ => new Dictionary<int, CardModel>()
        };
    }

    int FindCharacterWithCardType(CardType cardType)
    {
        // 소피아(0) 확인
        if (DataManager.Instance.CardForShopia.Values.Any(card => card.type == cardType))
            return 0;

        // 카일라(1) 확인
        if (DataManager.Instance.CardForKayla.Values.Any(card => card.type == cardType))
            return 1;

        // 레온(2) 확인
        if (DataManager.Instance.CardForLeon.Values.Any(card => card.type == cardType))
            return 2;

        return -1; // 타입을 가진 캐릭터 없음
    }

    void SetLastSibling(Transform t)
    {
        t.SetAsLastSibling();
    }// 클릭한 책갈피를 가장 위로 올림
    void SetAllToFirst()
    {
        foreach (var item in bookClip)
        {
            item.SetAsFirstSibling();
        }
    }// 모든 책갈피를 가장 아래로 내림
    void SetAllPageClose()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].gameObject.SetActive(false); // 모든 페이지 비활성화
        }
    }// 책의 모든 페이지 비활성화

    void UpdateCharacterButtonColors()
    {
        Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color activeColor = Color.white;
        
        if (characterButtons == null || characterButtons.Count == 0)
            return;

        for (int i = 0; i < characterButtons.Count; i++)
        {
            if (characterButtons[i] == null)
                continue;

            if (i == currentCharacterIndex)
            {
                characterButtons[i].color = activeColor; // 선택됨
            }
            else
            {
                characterButtons[i].color = dimColor; // 선택 안됨
            }
        }
    }
}

