using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardBook : MonoBehaviour,IBookControl
{
    // 페이지 인터페이스
    public int maxPageCount { get; set; } // 페이지 수
    public int currentPage { get; set; } = 0; // 현재 페이지

    // 카드 북 변수들
    [SerializeField] PopupUI_Book mainBook;
    [SerializeField] List<BookCards> bookCards;
    List<CardModel> cards; // 정렬되어 직접 사용할 카드 정보들.

    // 카드 상호작용 UI
    [SerializeField] RectTransform cardInfoPopup; // 카드 정보 팝업
    [SerializeField] BookCards cardInfoDisplay; // 카드 정보 표시 카드
    [SerializeField] TextMeshProUGUI cardInfoDesc; // 카드 정보 표시 카드 설명
    [SerializeField] RectTransform leftArrow; // 왼쪽 화살표
    [SerializeField] RectTransform rightArrow; // 오른쪽 화살표
    enum SortType
    {
        Index,
        TypeUp,
        TypeDown,
        CostUp,
        CostDown
    }
    SortType sortType = SortType.Index; // 카드 정렬 타입
    [SerializeField] TextMeshProUGUI sortTypeText; // 타입 정렬 버튼의 텍스트
    [SerializeField] string sortTypeTextString; // 타입 정렬 버튼의 기본 텍스트 (버튼 텍스트 로컬라이징 고려)

    [SerializeField] TextMeshProUGUI sortCostText; // 코스트 정렬 버튼의 텍스트
    [SerializeField] string sortCostTextString; // 코스트 정렬 버튼의 기본 텍스트 (버튼 텍스트 로컬라이징 고려)

    Dictionary<int, CardModel> cardForShopia = new();
    Dictionary<int, CardModel> cardForKayla = new();
    Dictionary<int, CardModel> cardForLeon = new();

    // 다음 페이지 화살표를 누르면 외부에서 호출하는 함수 (IBookControl 인터페이스)
    public void OnclickPageBefore()
    {
        if (currentPage > 0)
        {
            currentPage--;
            Debug.Log("이전 페이지로 이동: " + currentPage);
        }
        else
        {
            Debug.Log("첫 페이지입니다.");
        }
        UpdateCard(currentPage);// 페이지에 맞는 카드 정보 세팅.
        UpdateArrow(); // 화살표 업데이트
    }
    public void OnclickPageAfter()
    {
        if (currentPage < maxPageCount - 1)
        {
            currentPage++;
            Debug.Log("다음 페이지로 이동: " + currentPage);
        }
        else
        {
            Debug.Log("마지막 페이지입니다.");
        }
        UpdateCard(currentPage);// 페이지에 맞는 카드 정보 세팅.
        UpdateArrow(); // 화살표 업데이트
    }

    public void InitDictionary()
    {
        List<CardModel> allcards = CardSystemInitializer.Instance.loadedCards;
        foreach (var card in allcards)
        {
            if (card.characterClass == CharacterClass.Sophia)
            {
                cardForShopia.Add(card.index, card);
            }
            else if (card.characterClass == CharacterClass.Kayla)
            {
                cardForKayla.Add(card.index, card);
            }
            else if (card.characterClass == CharacterClass.Leon)
            {
                cardForLeon.Add(card.index, card);
            }
        }
    }// 각 캐릭터별 카드 데이터 초기화.
    public void CardsSet(int index)
    {
        switch (index)
        {
            case 0:// 소피아 카드 페이지
                // 페이지 업데이트
                maxPageCount = Mathf.CeilToInt(cardForShopia.Count / 8f);
                currentPage = 0;
                // 페이지내 카드 정보 업데이트 (카드의 정보, 카드 보유 유무)
                cards = SortByIndex(cardForShopia);// 초기 카드 설정 (인덱스 순서 정렬)
                UpdateCard(0);// 페이지에 맞는 카드 정보 세팅.
                UpdateArrow(); // 화살표 업데이트
                break;
            case 1:// 카일라 카드 페이지
                maxPageCount = Mathf.CeilToInt(cardForKayla.Count / 8f);
                currentPage = 0;
                cards = SortByIndex(cardForKayla);
                UpdateCard(0);// 페이지에 맞는 카드 정보 세팅.
                UpdateArrow(); // 화살표 업데이트
                break;
            case 2:// 레온 카드 페이지
                maxPageCount = Mathf.CeilToInt(cardForLeon.Count / 8f);
                currentPage = 0;
                cards = SortByIndex(cardForLeon);
                UpdateCard(0);// 페이지에 맞는 카드 정보 세팅.
                UpdateArrow(); // 화살표 업데이트
                break;
            default:
                Debug.LogError("Invalid index for CardsSet: " + index);
                break;
        }
    }// 해당하는 카드 페이지의 데이터에 따라 bookCards의 카드 데이터 최초 초기화 + 페이지 세팅.
    
    // 정렬 관련 해서는, 추후 카드의 수가 많아지면 한번더 리팩토링 + sorting 에 따른 화살표 표시 기능 추가하기.
    List<CardModel> SortByIndex(Dictionary<int, CardModel> cardDictionary) // 카드 데이터 정렬 (카드 index 순)
    {
        List<KeyValuePair<int, CardModel>> sortedList = new List<KeyValuePair<int, CardModel>>(cardDictionary);
        sortedList.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));// key값을 index로 받았기에 key값으로 정렬.
        sortType = SortType.Index; // 정렬 타입 초기화.
        sortTypeText.text = sortTypeTextString; // 타입 정렬 텍스트 초기화.
        sortCostText.text = sortCostTextString; // 코스트 정렬 텍스트 초기화.

        return sortedList.ConvertAll(pair => pair.Value);// 딕셔너리의 value값만 list로 반환.
    }
    public void ClickToSortByType() // 카드 데이터 정렬 (카드 타입 순)
    {
        List<CardModel> sortedCards = new List<CardModel>(cards); // 이미 cards 로 정렬된 카드 리스트를 사용.
        if(sortType == SortType.TypeUp) // 이미 타입 정렬이 적용 중인 경우 역순
        {
            sortType = SortType.TypeDown; // 타입 역순 정렬.
            sortTypeText.text = sortTypeTextString + " \u25BC"; // 아래 화살표 표시
            sortCostText.text = sortCostTextString; // 코스트 정렬 텍스트 초기화.
            sortedCards.Sort((card1, card2) => card2.type.CompareTo(card1.type)); // 카드 타입으로 역순 정렬
        }
        else
        {
            sortType = SortType.TypeUp; // 타입 정렬.
            sortTypeText.text = sortTypeTextString + " \u25B2"; // 위 화살표 표시
            sortCostText.text = sortCostTextString; // 코스트 정렬 텍스트 초기화.
            sortedCards.Sort((card1, card2) => card1.type.CompareTo(card2.type)); // 카드 타입으로 정렬
        }

        cards = sortedCards; // 정렬된 카드 리스트로 업데이트
        UpdateCard(currentPage); // 페이지 업데이트
    }
    public void ClickToSortByCost() // 카드 데이터 정렬 (카드 코스트 순)
    {
        List<CardModel> sortedCards = new List<CardModel>(cards); // 이미 cards 로 정렬된 카드 리스트를 사용.
        if(sortType == SortType.CostUp) // 이미 기본 코스트 정렬이 적용 중인 경우 역순
        {
            sortType = SortType.CostDown; // 코스트 역순 정렬.
            sortCostText.text = sortCostTextString + " \u25BC"; // 아래 화살표 표시
            sortTypeText.text = sortTypeTextString; // 타입 정렬 텍스트 초기화.
            sortedCards.Sort((card1, card2) => card2.manaCost.CompareTo(card1.manaCost)); // 카드 코스트로 역순 정렬
        }
        else
        {
            sortType = SortType.CostUp; // 코스트 정렬.
            sortCostText.text = sortCostTextString + " \u25B2"; // 위 화살표 표시
            sortTypeText.text = sortTypeTextString; // 타입 정렬 텍스트 초기화.
            sortedCards.Sort((card1, card2) => card1.manaCost.CompareTo(card2.manaCost)); // 카드 코스트로 정렬
        }

        cards = sortedCards; // 정렬된 카드 리스트로 업데이트
        UpdateCard(currentPage); // 페이지 업데이트
    }
    public void ClickToShowCardInfo(Transform t)
    {
        BookCards card = t.GetComponent<BookCards>();
        if (card.isEmpty == true) return; // 빈 카드를 누르면 리턴.
        card.GiveCardInfo(cardInfoDisplay); // 카드 정보 표시 카드
        cardInfoPopup.gameObject.SetActive(true);// 카드 정보 팝업 활성화
    }
    public void ClickToOffCardInfo() // 카드 정보 팝업 비활성화
    {
        cardInfoPopup.gameObject.SetActive(false);// 카드 정보 팝업 비활성화
    }

    void UpdateCard(int i)// 해당 페이지(int i)의 카드의 갯수에 따라 활성화 비활성화 여부 설정 , 카드 해금 여부에 따른 정보 표시 유무.
    {
        if (cards == null) return;

        // 전체 카드 한번 데이터 리셋
        foreach (var card in bookCards)
        {
            card.SetCardInfo(null);
            card.gameObject.SetActive(false);
        }

        // 카드의 갯수에 따라 빈 카드 칸 활성화 비활성화 여부 설정
        int startIndex = i * 8;
        int endIndex = Mathf.Min(startIndex + 8, cards.Count);
        for (int j = startIndex; j < endIndex; j++)
        {
            bookCards[j].gameObject.SetActive(true);
            bookCards[j].SetCardInfo(cards[j]);
        }

        // 카드 해금 여부에 따른 정보 표시 유무
        for (int j = startIndex; j < endIndex; j++)
        {
            int slotIndex = j - startIndex; // 슬롯 인덱스

            if (!cards[j].isUnlocked) // 카드가 아직 해금되지 않았다면.
            {
                bookCards[slotIndex].SetCardInfo(null);// 카드에 null 값 대입.
            }
            else
            {
                bookCards[slotIndex].SetCardInfo(cards[j]);// 카드에 카드 정보 대입.
            }
        }
    }
    void UpdateArrow()
    {
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
}

