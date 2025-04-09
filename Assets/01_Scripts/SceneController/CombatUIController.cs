using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatUIController : MonoBehaviour
{
    [SerializeField] CardDisplay cardDisplay; // 카드 디스플레이

    private void Awake()
    {
        GameManager.Instance.RegisterCombatUI(this);
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
    public void UsedCard(CardModel card)
    {
        // deckmodel 에서 사용된 카드, 실제 덱에서도 제거해주기.
        // 카드쪽 관련 스크립트 여기에..
    }
    public void ThrowCard(CardModel card)
    {
        // 덱에서 버린 카드, 실제 덱에서도 제거해주기.
        // 카드쪽 관련 스크립트 여기에..
    }
}
