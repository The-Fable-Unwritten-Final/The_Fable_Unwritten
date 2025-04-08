using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn } //적 턴인지 아군 턴인지 판별자

public class BattleFlowController : MonoBehaviour
{
    public List<IStatusReceiver> playerParty;   //플레이어 파티
    public List<IStatusReceiver> enemyParty;    //적 파티

    public int startMana = 3; //시작 마나
    public int currentMana;   //현재 마나

    private Dictionary<CharacterClass, DeckModel> decksByCharacter = new();     //캐릭터 마다의 사용, 미사용, 핸드 덱
    private Dictionary<CharacterClass, IStatusReceiver> characterMap = new();   //캐릭터클래스에 대한 정보
    private Dictionary<IStatusReceiver, CardModel> enemyPlannedSkill = new();   //적의 스킬 예측 정보

    private bool isBattleEnded = true;
    public TurnState currentTurn;
    private int turn = 1;

    /// <summary>
    /// 캐릭터 및 적 초기화(배틀 처음 진입시 시작할 것.)
    /// </summary>
    public void Initialize()
    {
        turn = 1;
        characterMap.Clear();
        decksByCharacter.Clear();
        enemyPlannedSkill.Clear();

        foreach (var player in playerParty)
        {
            characterMap[player.CharacterClass] = player;
            decksByCharacter[player.CharacterClass] = player.Deck;
        }
    }

    /// <summary>
    /// 배틀 시작 시 마나 및 덱 세팅
    /// </summary>
    public void StartBattle()
    {
        isBattleEnded = false;
        currentMana = startMana;
        currentTurn = TurnState.PlayerTurn;

        foreach(var player in playerParty)
        {
            if(player.IsAlive())            //모두 덱 초기화 후 3장 뽑기
            {
                player.Deck.Initialize(player.Deck.GetUsedCards());         
                player.Deck.Draw(DeckModel.startSize);
            }
        }

        PlanEnemySkills();      //적 스킬 목록 설정
        ExecutePlayerTurn();    //플레이어 턴 실행
    }

    /// <summary>
    /// 플레이어 턴 행동
    /// </summary>
    public void ExecutePlayerTurn()
    {
        if (isBattleEnded) return;      //전투 종료 명령 확인 시 전투 종료

        if (currentMana < startMana)    //마나가 시작 마나보다 적을 시 시작 마나로 초기화
            currentMana = startMana;

        DrawMissingHands();             //각각 패가 3장이 되도록(살아 있을 경우에만) 드로우

        Debug.Log("플레이어 턴 시작");

        // 이후 카드 사용 → 외부에서 UseCard 호출
    }

    /// <summary>
    /// 카드 사용하기
    /// </summary>
    /// <param name="card">사용하는 카드</param>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    public void UseCard(CardModel card, IStatusReceiver caster, IStatusReceiver target)
    {
        if (!card.IsUsable(currentMana) || !caster.IsAlive() || !target.IsAlive())      //사용 가능하지 않거나 적 또는 사용자가 죽어 있다면 생략하기
            return;
        //사용 가능할 시
        currentMana -= card.manaCost;         //마나 사용
        card.Play(caster, target);          //카드 사용(효과 적용)
        caster.Deck.Discard(card);          //사용 카드를 사용한 카드 덱으로 보내기

        Debug.Log($"{caster.CharacterClass} 가 {card.name} 사용 → {target.CharacterClass}");
    }

    /// <summary>
    /// 플레이어 턴 종료시 카드 3장으로 맞춤 처리 (미완)
    /// </summary>
    public void EndPlayerTurn()
    {
        foreach (var player in playerParty)
        {
            if (player.IsAlive())
            {
                var hand = player.Deck.Hand;

                if (hand.Count > DeckModel.startSize)
                {
                    Debug.LogWarning($"{player.CharacterClass} 카드가 3장을 초과합니다. 버릴 카드 선택이 필요합니다.");

                    // todo: 외부에서 선택한 카드 전달 필요
                    // 여기선 임시로 가장 뒤의 카드부터 자동으로 버린다고 가정
                    List<CardModel> toDiscard = new(hand);
                    toDiscard.Reverse();
                    player.Deck.DiscardHandToThree(toDiscard);
                }
            }
        }

        currentTurn = TurnState.EnemyTurn;          //적 턴으로 이행
        ExecuteEnemyTurn();
    }

    public void ExecuteEnemyTurn()
    {
        //적 행동 설정;

        CheckBattleEnd();       //배틀 종료 확인

        if (!isBattleEnded)     //배틀 종료 되지 않았으면 플레이어 턴으로
        {
            turn++;
            currentTurn = TurnState.PlayerTurn;
            ExecutePlayerTurn();
        }
    }

    private void PlanEnemySkills()
    {
        //적 스킬 설정
    }

    private void DrawMissingHands()
    {
        foreach (var player in playerParty)
        {
            if (!player.IsAlive()) continue;

            int toDraw = DeckModel.startSize - player.Deck.Hand.Count;  //손패가 3장 보다 적다면 3장이 될때까지 드로우
            if (toDraw > 0)
                player.Deck.Draw(toDraw);
        }
    }


    private void CheckBattleEnd()
    {
        bool allPlayersDead = playerParty.TrueForAll(p => !p.IsAlive());
        bool allEnemiesDead = enemyParty.TrueForAll(p => !p.IsAlive());

        if (allPlayersDead)
        {
            isBattleEnded = true;
            Debug.Log("▶ 전투 패배");
        }
        else if (allEnemiesDead)
        {
            isBattleEnded = true;
            Debug.Log("▶ 전투 승리");
        }
    }


    /// <summary>
    /// 전투 포기 메서드
    /// </summary>
    /// <param name="playerGaveUp">플레이어가 현재 전투를 포기 하였는지 확인</param>
    public void ForceEndBattle(bool playerGaveUp)
    {
        isBattleEnded = true;
        if (playerGaveUp)
        {
            Debug.Log("▶ 플레이어 전투 포기 → 타이틀로 이동");
            // todo : 타이틀 화면으로
        }
    }
}