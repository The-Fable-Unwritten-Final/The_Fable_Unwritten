using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatUIController : MonoBehaviour
{
    [SerializeField] CardDisplay cardDisplay; // 카드 디스플레이
    [SerializeField] BattleFlowController battleFlow;

    private void Awake()
    {
        GameManager.Instance.RegisterCombatUI(this);
    }
    private void Start()
    {
        // 구독 순서 설정을 위해 cardDisplay의 메서드를 이곳에서 구독.
        CardStatusUpdate += cardDisplay.SetCardCanDrag; // 카드 상태 업데이트(CanDrag 체킹을 위함)
        CardStatusUpdate += cardDisplay.AllCardInfoUpdate; // 카드 상태 업데이트(코스트, 설명(효과 수치) 업데이트)
        GameManager.Instance.turnController.OnPlayerTurn += CardStatusUpdate;// 카드 상태 업데이트(CanDrag 체킹을 위함)
        GameManager.Instance.turnController.OnEnemyTurn += CardStatusUpdate;// 카드 상태 업데이트(CanDrag 체킹을 위함)
    }

    // 게임 매니저에서 해당 클라스 등록과 해제 활성
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// 카드의 코스트, 설명(효과 수치), 카드의 MouseOver,Drag 가능 상태 업데이트.
    /// 전투중 카드를 사용하거나, 코스트 또는 능력치에 변동이 생길 행동시 호출.
    /// </summary>
    public Action CardStatusUpdate;

    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.Instance.UnregisterCombatUI();// 씬 나갈 때 UI 컨트롤러 해제
    }
    
    public void DrawCard(CardModel card)
    {
        cardDisplay.AddCard(card); // 드로우 한 카드 UI 업데이트.
    }
    public bool UsedCard(CardModel card, IStatusReceiver target)
    {
        var caster = battleFlow.GetCharacter(card.characterClass);
        Debug.Log($"[CombatUI] UsedCard 호출 : {card.cardName} 사용 시전 캐릭터 : {caster} / 타겟 : {target}");
        if (caster == null || target == null) return false;

        if (battleFlow.CanUseCard(card, caster, target, battleFlow.currentMana))
        {
            Debug.Log("카드 사용 가능");
            battleFlow.TryUseCard(card, caster.ChClass, target);
            CardStatusUpdate?.Invoke(); // 상태 갱신
            return true;
        }
        else
        {
            Debug.LogWarning($"[CombatUI] {card.cardName} 사용 조건 불충족 (UsedCard 호출)");
            return false;
        }
    }
    public void ThrowCard(CardModel card)
    {
        var caster = battleFlow.GetCharacter(card.characterClass);
        if (caster == null) return;

        // 핸드에서 제거하고 사용 덱에 추가
        caster.Deck.Discard(card);
        CardStatusUpdate?.Invoke(); // 상태 갱신
    }
}
