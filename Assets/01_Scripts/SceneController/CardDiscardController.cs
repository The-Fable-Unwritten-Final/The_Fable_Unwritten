using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CardInHand;

public class CardDiscardController : MonoBehaviour
{
    [SerializeField] GameObject discardDisplay; // 카드 버리기 UI
    [SerializeField] TextMeshProUGUI discardInfoText; // 카드 버리기 UI 텍스트
    [SerializeField] CardDisplay cardDisplay; // 카드 디스플레이

    // 카드 버리기 카운트
    public bool isAll; // 전체 카드 버리기인지 확인하는 변수
    int reqTotalCount; // 전체 카드 버리기 카운트

    int reqSophiaCount; // 소피아 카드 버리기 카운트
    int totalSophiaCount; // 소피아 카드 (버려야 할) 총 수
    int reqKylaCount; // 카일라 카드 버리기 카운트
    int totalKylaCount; // 카일라 카드 (버려야 할) 총 수
    int reqLeonCount; // 레온 카드 버리기 카운트
    int totalLeonCount; // 레온 카드 (버려야 할) 총 수

    // 외부에 공개할 읽기 전용 프로퍼티
    public int ReqTotalCount => reqTotalCount;
    public int ReqSophiaCount => reqSophiaCount;
    public int ReqKylaCount => reqKylaCount;
    public int ReqLeonCount => reqLeonCount;

    List<CardInHand> cardInHands = new List<CardInHand>(); // 버리기에 선택된 손안의 카드들

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
            DiscardCard(3); // 테스트용 카드 버리기 호출
        if(Input.GetKeyDown(KeyCode.P))
            DiscardCard(1, 1, 0); // 테스트용 카드 버리기 호출
    }
    private void Awake()
    {
        GameManager.Instance.RegisterCardDiscardController(this);
    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.Instance.UnregisterCardDiscardController();
    }
    private void UpdateText()
    {
        string text = "";

        if (isAll)
        {
            text = $"카드를 {reqTotalCount}장 버려야 합니다.";
        }
        else
        {
            text = "버릴 카드를 선택하세요.\n\n";
            if(totalSophiaCount > 0)
            {
                text += $"<sprite=0> {reqSophiaCount}/{totalSophiaCount} \n";
            }
            if(totalKylaCount > 0 )
            {
                text += $"\n<sprite=1> {reqKylaCount}/{totalKylaCount} \n";
            }
            if(totalLeonCount > 0)
            {
                text += $"\n<sprite=2> {reqLeonCount}/{totalLeonCount} ";
            }
        }

        discardInfoText.text = text; // 카드 버리기 UI 텍스트 업데이트
    }
    private void ResetCount()
    {
        reqTotalCount = 0; // 전체 버리기 카운트 초기화

        reqSophiaCount = 0; // 소피아 카드 버리기 카운트 초기화
        totalSophiaCount = 0; // 소피아 카드 (버려야 할) 총 수 초기화
        reqKylaCount = 0; // 카일라 카드 버리기 카운트 초기화
        totalKylaCount = 0; // 카일라 카드 (버려야 할) 총 수 초기화
        reqLeonCount = 0; // 레온 카드 버리기 카운트 초기화
        totalLeonCount = 0; // 레온 카드 (버려야 할) 총 수 초기화
    }
    private void DiscardCounting(CardInHand card)
    {
        switch (card.cardData.characterClass)
        {
            case CharacterClass.Sophia:
                reqTotalCount--;
                reqSophiaCount--;
                break;
            case CharacterClass.Kayla:
                reqTotalCount--;
                reqKylaCount--;
                break;
            case CharacterClass.Leon:
                reqTotalCount--;
                reqLeonCount--;
                break;
        }
    }
    private void RevertCounting(CardInHand card)
    {
        switch (card.cardData.characterClass)
        {
            case CharacterClass.Sophia:
                reqTotalCount++;
                reqSophiaCount++;
                break;
            case CharacterClass.Kayla:
                reqTotalCount++;
                reqKylaCount++;
                break;
            case CharacterClass.Leon:
                reqTotalCount++;
                reqLeonCount++;
                break;
        }
    }

    /// <summary>
    /// 카드 사용 or 특수 효과로 전체 카드에서 카드를 버려야할 때 호출
    /// </summary>
    /// <param name="req">버려야하는 총 카드수</param>
    public void DiscardCard(int req)
    {
        isAll = true; // 전체 카드 버리기 설정
        ResetCount(); // 카운트 초기화
        reqTotalCount = req; // 요구 버리기 카운트 설정
        discardDisplay.SetActive(true); // 카드 버리기 UI 활성화
        UpdateText(); // 카드 버리기 UI 텍스트 업데이트

        cardDisplay.SetCardCanDiscard(); // 카드 상태 업데이트
    }
    /// <summary>
    /// 턴이 끝날 때 3장을 초과하는 카드를 버릴때 호출
    /// </summary>
    /// <param name="sop">버려야하는 소피아 카드</param>
    /// <param name="ky">버려야하는 카일라 카드</param>
    /// <param name="leo">버려야하는 레온 카드</param>
    public void DiscardCard(int sop, int ky, int leo) 
    {
        isAll = false; // 전체 카드 버리기 해제
        ResetCount(); // 카운트 초기화
        reqTotalCount = sop + ky + leo; // 요구 버리기 카운트 설정
        reqSophiaCount = sop; // 요구 버리기 카운트 설정
        totalSophiaCount = sop; // 요구 버리기 카운트 설정
        reqKylaCount = ky; // 요구 버리기 카운트 설정
        totalKylaCount = ky; // 요구 버리기 카운트 설정
        reqLeonCount = leo; // 요구 버리기 카운트 설정
        totalLeonCount = leo; // 요구 버리기 카운트 설정
        discardDisplay.SetActive(true); // 카드 버리기 UI 활성화
        UpdateText(); // 카드 버리기 UI 텍스트 업데이트

        cardDisplay.SetCardCanDiscard(); // 카드 상태 업데이트
    }
    /// <summary>
    /// 버릴 카드로 추가
    /// </summary>
    /// <param name="card">버릴 카드 리스트에 추가할 카드</param>
    public void AddDiscardCard(CardInHand card)
    {
        DiscardCounting(card); // 카드 카운트 감소
        cardInHands.Add(card); // 카드 리스트에 추가
        UpdateText(); // 카드 버리기 UI 텍스트 업데이트
        card.SetCardState(CardState.OnDiscard); // 카드 상태를 버리기 상태로 변경
    }
    public void RemoveDiscardCard(CardInHand card)
    {
        RevertCounting(card); // 카드 카운트 복구
        cardInHands.Remove(card); // 카드 리스트에서 제거
        UpdateText(); // 카드 버리기 UI 텍스트 업데이트
        card.SetCardState(CardState.CanDiscard); // 버리기 가능 상태로 변경
    }
    /// <summary>
    /// 턴 종료시 카드수를 체크해 카드 버리기 실행 호출.
    /// </summary>
    public bool CheckCountOk()
    {
        bool check3Under = GameManager.Instance.turnController.battleFlow.playerParty.All(decks => decks.Deck.Hand.Count <= DeckModel.startSize); // 모든 덱의 카드 수가 3장 이하인지 확인
        int sop = 0;
        int ky = 0;
        int leo = 0;

        if (!check3Under)// 3장 초과인 경우 카드 버리기 실행
        {
            foreach(var player in GameManager.Instance.turnController.battleFlow.playerParty)
            {
                switch (player.ChClass)
                {
                    case CharacterClass.Sophia:
                        sop = Mathf.Max(sop, 0, player.Deck.Hand.Count - DeckModel.startSize); // 소피아 카드 초과 수 
                        break;
                    case CharacterClass.Kayla:
                        ky = Mathf.Max(ky, 0, player.Deck.Hand.Count - DeckModel.startSize); // 카일라 카드 초과 수
                        break;
                    case CharacterClass.Leon:
                        leo = Mathf.Max(leo, 0, player.Deck.Hand.Count - DeckModel.startSize); // 레온 카드 초과 수
                        break;
                }
            }

            DiscardCard(sop,ky,leo); // 카드 버리기 UI 호출
            return false;
        }
        else
            return true; // 카드 수가 3장 이하인 경우 턴 종료
    }
    /// <summary>
    /// UI에서 카드 버리기를 확정하는 버튼
    /// </summary>
    public void ConfirmDiscard()
    {
        if (isAll)
        {
            if (reqTotalCount != 0) return; // 전체 카드 버리기 + 카운트가 0이 아닌 경우 리턴
        }
        else if (reqTotalCount != 0 || reqSophiaCount != 0 || reqKylaCount != 0 || reqLeonCount != 0) return; // 카드 버리기 카운트가 0이 아닌 경우 리턴


        foreach (var card in cardInHands)
        {
            cardDisplay.ThrowAwayCard(card); // 카드 버리기
        }
        cardInHands.Clear(); // 카드 리스트 초기화
        discardDisplay.SetActive(false); // 카드 버리기 UI 비활성화

        if(!isAll) GameManager.Instance.turnController.AtPlayerTurn(); // 턴 종료 버튼과 동일한 효과. (isAll 이 아닌 경우, 턴 종료시의 호출이기 때문.)
    }
}
