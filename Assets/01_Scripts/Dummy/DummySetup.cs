using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummySetup : MonoBehaviour
{
    public BattleFlowController battleFlowController;

    public PlayerController sophiaPrefab;
    public PlayerController kaylaPrefab;
    public PlayerController leonPrefab;

    public PlayerData sophiaData;
    public PlayerData kaylaData;
    public PlayerData leonData;

    public Enemy enemy1Prefab;
    public Enemy enemy2Prefab;

    [Header("CSV로부터 로드된 카드 데이터")]
    public CardSystemInitializer cardSystemInitializer;

    void Start()
    {
        // 카드가 먼저 로드되었는지 확인
        if (cardSystemInitializer == null || cardSystemInitializer.loadedCards == null || cardSystemInitializer.loadedCards.Count == 0)
        {
            Debug.LogError("[DummySetup] 카드 데이터가 로드되지 않았습니다!");
            return;
        }

        // 플레이어 인스턴스 생성 및 초기화
        var sophia = Instantiate(sophiaPrefab);
        var kayla = Instantiate(kaylaPrefab);
        var leon = Instantiate(leonPrefab);

        sophia.Initialize(sophiaData, CharacterClass.Sophia);
        kayla.Initialize(kaylaData, CharacterClass.Kayla);
        leon.Initialize(leonData, CharacterClass.Leon);

        // 덱 세팅 (index 기반)
        sophia.Deck.Initialize(GetDeckByIndexes(new List<int> { 1000, 1001,1002 ,1006, 1007 }));
        kayla.Deck.Initialize(GetDeckByIndexes(new List<int> { 1001, 1002,1003, 1004, 1009 }));
        leon.Deck.Initialize(GetDeckByIndexes(new List<int> { 1002, 1003,1004, 1005, 1008 }));

        // 적 인스턴스 생성
        var enemy1 = Instantiate(enemy1Prefab);
        var enemy2 = Instantiate(enemy2Prefab);

        // 파티 등록
        battleFlowController.playerParty = new List<IStatusReceiver> { sophia, kayla, leon };
        battleFlowController.enemyParty = new List<IStatusReceiver> { enemy1, enemy2 };

        // 전투 시작
        battleFlowController.Initialize();
        battleFlowController.StartBattle();

        StartCoroutine(AutoCardTest());

    }

    /// <summary>
    /// 인덱스 기준으로 CardModel을 찾아오는 유틸
    /// </summary>
    List<CardModel> GetDeckByIndexes(List<int> indexes)
    {
        List<CardModel> result = new();
        foreach (int index in indexes)
        {
            var card = cardSystemInitializer.loadedCards.Find(c => c.index == index);
            if (card != null)
                result.Add(card);
            else
                Debug.LogWarning($"[DummySetup] 카드 인덱스 {index}를 찾을 수 없습니다.");
        }
        return result;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (battleFlowController.currentTurn == TurnState.PlayerTurn)
            {
                Debug.Log($"▶ 스페이스 입력 감지 → 플레이어 턴 종료");
                battleFlowController.EndPlayerTurn();
            }
            else
            {
                Debug.Log("▶ 아직 적 턴입니다. 기다려주세요.");
            }
        }
    }



    IEnumerator AutoCardTest()
    {
        yield return new WaitForSeconds(1f);

        var sophia = battleFlowController.playerParty[0] as PlayerController;
        var target = battleFlowController.enemyParty[0];

        Debug.Log($"Sophia 핸드 수: {sophia.Deck.Hand.Count}");
        foreach (var card in new List<CardModel>(sophia.Deck.Hand))
        {
            battleFlowController.UseCard(card, sophia, target);
            yield return new WaitForSeconds(0.5f);
        }
    }

}
