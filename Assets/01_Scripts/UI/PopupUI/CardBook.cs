using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardBook : MonoBehaviour,IBookControl
{
    // 페이지 인터페이스
    public int maxPageCount { get; set; } // 페이지 수
    public int currentPage { get; set; } = 0; // 현재 페이지

    // 카드 북 변수들
    [SerializeField] PopupUI_Book mainBook;
    [SerializeField] List<BookCards> bookCards;
    List<CardModel> cards; // 정렬되어 직접 사용할 카드 정보들.
    List<CardModel> originalCards; // 필터링 전 원본 카드 리스트

    // 카드 상호작용 UI
    [SerializeField] RectTransform cardInfoPopup; // 카드 정보 팝업
    [SerializeField] BookCards cardInfoDisplay; // 카드 정보 표시 카드
    [SerializeField] TextMeshProUGUI cardInfoDesc; // 카드 정보 표시 카드 설명
    [SerializeField] RectTransform leftArrow; // 왼쪽 화살표
    [SerializeField] RectTransform rightArrow; // 오른쪽 화살표
    [SerializeField] TextMeshProUGUI pageNum;
    
    // 필터 버튼들
    [SerializeField] List<Image> typeFilterButtons; // 카드 타입 필터 버튼 9개 (0-8)
    [SerializeField] List<Image> costFilterButtons; // 코스트 필터 버튼 4개 (0, 1, 2, 3+)
    
    CardType? filterCardType = null; // 현재 활성화된 카드 타입 필터
    int? filterManaCost = null; // 현재 활성화된 코스트 필터 (0, 1, 2, 3+)
    
    enum SortType
    {
        Index,
        TypeUp,
        TypeDown
    }
    SortType sortType = SortType.Index; // 카드 정렬 타입

    // 다음 페이지 화살표를 누르면 외부에서 호출하는 함수 (IBookControl 인터페이스)
    public void OnclickPageBefore()
    {
        if (currentPage > 0)
        {
            currentPage--;
        }
        else
        {
        }
        UpdateCard(currentPage);// 페이지에 맞는 카드 정보 세팅.
        UpdateArrow(); // 화살표 업데이트
        UpdatePageNum(); // 페이지 번호 업데이트
    }
    public void OnclickPageAfter()
    {
        if (currentPage < maxPageCount - 1)
        {
            currentPage++;
        }
        UpdateCard(currentPage);// 페이지에 맞는 카드 정보 세팅.
        UpdateArrow(); // 화살표 업데이트
        UpdatePageNum(); // 페이지 번호 업데이트
    }

    public void CardsSet(int index)
    {
        switch (index)
        {
            case 0:// 소피아 카드 페이지
                // 페이지 업데이트
                originalCards = SortByIndex((Dictionary<int,CardModel>)DataManager.Instance.CardForShopia);// 초기 카드 설정 (인덱스 순서 정렬)
                cards = new List<CardModel>(originalCards);
                maxPageCount = Mathf.CeilToInt(cards.Count / 8f);
                currentPage = 0;
                filterCardType = null; // 필터 초기화
                filterManaCost = null; // 코스트 필터 초기화
                // 페이지내 카드 정보 업데이트 (카드의 정보, 카드 보유 유무)
                UpdateCard(0);// 페이지에 맞는 카드 정보 세팅.
                UpdateArrow(); // 화살표 업데이트
                UpdatePageNum(); // 페이지 번호 업데이트
                break;
            case 1:// 카일라 카드 페이지
                originalCards = SortByIndex((Dictionary<int,CardModel>)DataManager.Instance.CardForKayla);
                cards = new List<CardModel>(originalCards);
                maxPageCount = Mathf.CeilToInt(cards.Count / 8f);
                currentPage = 0;
                filterCardType = null; // 필터 초기화
                filterManaCost = null; // 코스트 필터 초기화
                UpdateCard(0);// 페이지에 맞는 카드 정보 세팅.
                UpdateArrow(); // 화살표 업데이트
                UpdatePageNum(); // 페이지 번호 업데이트
                break;
            case 2:// 레온 카드 페이지
                originalCards = SortByIndex((Dictionary<int,CardModel>)DataManager.Instance.CardForLeon);
                cards = new List<CardModel>(originalCards);
                maxPageCount = Mathf.CeilToInt(cards.Count / 8f);
                currentPage = 0;
                filterCardType = null; // 필터 초기화
                filterManaCost = null; // 코스트 필터 초기화
                UpdateCard(0);// 페이지에 맞는 카드 정보 세팅.
                UpdateArrow(); // 화살표 업데이트
                UpdatePageNum(); // 페이지 번호 업데이트
                break;
            default:
                Debug.LogError("Invalid index for CardsSet: " + index);
                break;
        }
        ResetAllFilterButtons(); // 필터 버튼 색상 초기화
    }// 해당하는 카드 페이지의 데이터에 따라 bookCards의 카드 데이터 최초 초기화 + 페이지 세팅.
    
    // 정렬 관련 해서는, 추후 카드의 수가 많아지면 한번더 리팩토링 + sorting 에 따른 화살표 표시 기능 추가하기.
    List<CardModel> SortByIndex(Dictionary<int, CardModel> cardDictionary) // 카드 데이터 정렬 (카드 index 순)
    {
        List<KeyValuePair<int, CardModel>> sortedList = new List<KeyValuePair<int, CardModel>>(cardDictionary);
        sortedList.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));// key값을 index로 받았기에 key값으로 정렬.
        sortType = SortType.Index; // 정렬 타입 초기화.

        return sortedList.ConvertAll(pair => pair.Value);// 딕셔너리의 value값만 list로 반환.
    }
    public void ClickToSortByCost() // 카드 데이터 정렬 (카드 코스트 순)
    {
        List<CardModel> sortedCards = new List<CardModel>(cards); // 이미 cards 로 정렬된 카드 리스트를 사용.
        if(sortType == SortType.TypeUp) // 이미 타입 정렬이 적용 중인 경우 역순
        {
            sortType = SortType.TypeDown; // 타입 역순 정렬.
            sortedCards.Sort((card1, card2) => card2.type.CompareTo(card1.type)); // 카드 타입으로 역순 정렬
        }
        else
        {
            sortType = SortType.TypeUp; // 타입 정렬.
            sortedCards.Sort((card1, card2) => card1.type.CompareTo(card2.type)); // 카드 타입으로 정렬
        }

        cards = sortedCards; // 정렬된 카드 리스트로 업데이트
        UpdateCard(currentPage); // 페이지 업데이트
        UpdatePageNum(); // 페이지 번호 업데이트
    }
    public void ClickToShowCardInfo(Transform t)
    {
        BookCards card = t.GetComponent<BookCards>();
        if (card.isEmpty == true) return; // 빈 카드를 누르면 리턴.
        card.GiveCardInfo(cardInfoDisplay); // 카드 정보 표시 카드
        cardInfoDesc.text = card.flavorText;
        cardInfoPopup.gameObject.SetActive(true);// 카드 정보 팝업 활성화
    }
    public void ClickToOffCardInfo() // 카드 정보 팝업 비활성화
    {
        cardInfoPopup.gameObject.SetActive(false);// 카드 정보 팝업 비활성화
    }

    public void FilterByCardType(int cardTypeIndex)
    {
        // PopupUI_Book을 통해 캐릭터 전환 및 필터링 처리
        if (mainBook != null)
        {
            mainBook.FilterByCardType(cardTypeIndex);
            return;
        }

        // mainBook이 없는 경우 직접 필터링
        ApplyCardTypeFilter(cardTypeIndex);
    }

    public void ApplyCardTypeFilter(int cardTypeIndex)
    {
        if (originalCards == null || originalCards.Count == 0)
        {
            return;
        }

        CardType selectedType = (CardType)cardTypeIndex;

        if (filterCardType == selectedType)
        {
            // 같은 타입을 다시 클릭하면 필터 해제
            filterCardType = null;
            filterManaCost = null; // 코스트도 함께 해제
            cards = new List<CardModel>(originalCards);
        }
        else
        {
            // 새로운 타입으로 필터링
            filterCardType = selectedType;
            filterManaCost = null; // 코스트 필터 해제
            cards = originalCards.Where(card => card.type == selectedType).ToList();
        }

        currentPage = 0; // 페이지 초기화
        maxPageCount = Mathf.CeilToInt(cards.Count / 8f);
        UpdateCard(0); // 페이지에 맞는 카드 정보 세팅
        UpdateArrow(); // 화살표 업데이트
        UpdatePageNum(); // 페이지 번호 업데이트
        UpdateTypeFilterButtonColors(); // 타입 필터 버튼 색상 업데이트
        UpdateCostFilterButtonColors(); // 코스트 필터 버튼 색상 업데이트
    }

    public void FilterByCost0() // 코스트 0 필터링
    {
        FilterByCost(0);
    }

    public void FilterByCost1() // 코스트 1 필터링
    {
        FilterByCost(1);
    }

    public void FilterByCost2() // 코스트 2 필터링
    {
        FilterByCost(2);
    }

    public void FilterByCost3Plus() // 코스트 3 이상 필터링
    {
        FilterByCost(3);
    }

    public void FilterByCost(int cost)
    {
        if (originalCards == null || originalCards.Count == 0)
        {
            return;
        }

        if (filterManaCost == cost)
        {
            // 같은 코스트를 다시 클릭하면 필터 해제
            filterManaCost = null;
            filterCardType = null; // 타입도 함께 해제
            cards = new List<CardModel>(originalCards);
        }
        else
        {
            // 새로운 코스트로 필터링
            filterManaCost = cost;
            filterCardType = null; // 타입 필터 해제
            if (cost < 3)
            {
                // 정확한 코스트만 필터링
                cards = originalCards.Where(card => card.manaCost == cost).ToList();
            }
            else
            {
                // 3 이상인 카드 필터링
                cards = originalCards.Where(card => card.manaCost >= 3).ToList();
            }
        }

        currentPage = 0; // 페이지 초기화
        maxPageCount = Mathf.CeilToInt(cards.Count / 8f);
        UpdateCard(0); // 페이지에 맞는 카드 정보 세팅
        UpdateArrow(); // 화살표 업데이트
        UpdatePageNum(); // 페이지 번호 업데이트
        UpdateTypeFilterButtonColors(); // 타입 필터 버튼 색상 업데이트
        UpdateCostFilterButtonColors(); // 코스트 필터 버튼 색상 업데이트
    }

    public void CardsSetWithFilter(int characterIndex, int cardTypeIndex)
    {
        // 먼저 캐릭터 카드 로드
        CardsSet(characterIndex);
        // 그 다음 필터링 적용
        FilterByCardType(cardTypeIndex);
    }

    void UpdateCard(int i)// 해당 페이지(int i)의 카드의 갯수에 따라 활성화 비활성화 여부 설정 , 카드 해금 여부에 따른 정보 표시 유무.
    {
        if (cards == null) return;

        // 카드 1차 정렬 (해금 여부에 따라 정렬)
        var sortedCards = cards.OrderByDescending(card => card.isUnlocked).ToList();

        // 전체 카드 한번 데이터 리셋
        foreach (var card in bookCards)
        {
            card.SetCardInfo(null);
            card.gameObject.SetActive(false);
        }

        // 페이지 오프셋 및 실제로 표시할 카드 수 계산
        int offset = i * 8;
        int endCardIndex = Mathf.Min(sortedCards.Count - offset, 8);

        for (int j = 0; j < endCardIndex; j++)
        {
            int cardIndex = offset + j;
            var cardData = sortedCards[cardIndex];

            bookCards[j].gameObject.SetActive(true);

            if (!cardData.isUnlocked)
            {
                bookCards[j].SetCardInfo(null); // 잠김 카드
            }
            else
            {
                bookCards[j].SetCardInfo(cardData); // 해금된 카드
            }
        }
    }
    void UpdateArrow()
    {
        if(maxPageCount <= 1) // 페이지가 1페이지 이하일 경우 화살표 비활성화
        {
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            return;
        }
        if(currentPage == 0) // 첫 페이지일 경우 왼쪽 화살표 비활성화
        {
            leftArrow.gameObject.SetActive(false);
        }
        else
        {
            leftArrow.gameObject.SetActive(true);
        }

        if(currentPage == maxPageCount - 1) // 마지막 페이지일 경우 오른쪽 화살표 비활성화
        {
            rightArrow.gameObject.SetActive(false);
        }
        else
        {
            rightArrow.gameObject.SetActive(true);
        }
    }

    void UpdatePageNum()
    {
        pageNum.text = (currentPage + 1) + " / " + maxPageCount;
    }

    void ResetAllFilterButtons()
    {
        // 모든 필터 버튼을 어두운 색상으로 초기화
        Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        if (typeFilterButtons != null)
        {
            foreach (var btn in typeFilterButtons)
            {
                if (btn != null)
                    btn.color = dimColor;
            }
        }
        
        if (costFilterButtons != null)
        {
            foreach (var btn in costFilterButtons)
            {
                if (btn != null)
                    btn.color = dimColor;
            }
        }
    }

    void UpdateTypeFilterButtonColors()
    {
        Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color activeColor = Color.white;
        
        if (typeFilterButtons == null || typeFilterButtons.Count == 0)
            return;

        for (int i = 0; i < typeFilterButtons.Count; i++)
        {
            if (typeFilterButtons[i] == null)
                continue;

            if (filterCardType != null && i == (int)filterCardType)
            {
                typeFilterButtons[i].color = activeColor; // 선택됨
            }
            else
            {
                typeFilterButtons[i].color = dimColor; // 선택 안됨
            }
        }
    }

    void UpdateCostFilterButtonColors()
    {
        Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color activeColor = Color.white;
        
        if (costFilterButtons == null || costFilterButtons.Count == 0)
            return;

        for (int i = 0; i < costFilterButtons.Count; i++)
        {
            if (costFilterButtons[i] == null)
                continue;

            bool isActive = false;
            
            if (filterManaCost != null)
            {
                // i: 0=cost0, 1=cost1, 2=cost2, 3=cost3+
                if (i < 3 && filterManaCost == i)
                {
                    isActive = true;
                }
                else if (i == 3 && filterManaCost == 3)
                {
                    isActive = true;
                }
            }
            
            costFilterButtons[i].color = isActive ? activeColor : dimColor;
        }
    }
}

