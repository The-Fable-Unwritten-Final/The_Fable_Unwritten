using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캠프 씬에서 단일 캐릭터의 상호작용 관리
/// ? 버튼: 스토리 대화 (다이어로그 시스템 사용) - UI_CampController에서 조건에 따라 표시
/// 휴식 버튼: 캐릭터 모션 + 체력 회복 - Kayla, Sophia, Leon
/// 정비 버튼: 덱 카드 제거 UI 표시 - Kayla, Sophia, Leon
/// </summary>
public class CampCharSelection : MonoBehaviour
{
    [Header("Character Data")]
    public PlayerData character; // 이 오브젝트가 관리하는 캐릭터 (Dorothy는 null)
    
    [Header("Button")]
    [SerializeField] private GameObject storyButton; // ? 버튼 (처음엔 비활성화)
    [SerializeField] private GameObject restButton; // 휴식 버튼
    [SerializeField] private GameObject maintenanceButton; // 정비 버튼

    // 캐릭터 상호작용 완료
    public System.Action OnCharacterInteractionComplete;

    private void Start()
    {
        // 스토리 버튼은 최초, 비활성화
        // UI_CampController에서 조건에 따라 활성화
        if (storyButton != null)
        {
            storyButton.SetActive(false);
        }
    }

    /// <summary>
    /// 휴식 버튼 클릭
    /// </summary>
    public void OnRestButtonClicked()
    {
        if (character == null) return;

        // 1. 캐릭터 모션 재생 (잠에 빠지는 모션)
        PlayRestAnimation();

        // 2. 체력 회복 (10)
        HealCharacter(10);

        // 3. 상호작용 완료 표시
        MarkCharacterInteractionComplete();

        Debug.Log($"[CampCharSelection] {character.CharacterName}이(가) 휴식 - 10 체력 회복");
    }

    /// <summary>
    /// 정비 버튼 클릭
    /// </summary>
    public void OnMaintenanceButtonClicked()
    {
        if (character == null) return;

        // PopupUI_Maintenance 팝업 열기 (캐릭터 & 현재 CampCharSelection 전달)
        PopupUI_Maintenance maintenancePopup = UIManager.Instance.ShowPopup<PopupUI_Maintenance>();
        if (maintenancePopup != null)
        {
            maintenancePopup.ShowMaintenance(character, this);
        }

        Debug.Log($"[CampCharSelection] {character.CharacterName}의 정비 팝업 표시");
    }

    /// <summary>
    /// 정비 완료 콜백 (카드 제거 UI에서 호출)
    /// </summary>
    public void OnMaintenanceComplete()
    {
        MarkCharacterInteractionComplete();
        Debug.Log($"[CampCharSelection] 정비 완료 - {character.CharacterName}");
    }

    /// <summary>
    /// 스토리 대화 버튼 클릭
    /// </summary>
    public void OnStoryButtonClicked()
    {
        // 대화 콘텍스트 리스트 중 랜덤 선택
        // 다이어로그 시스템 사용하여 대화 시작
        // 대화는 선택지 소모를 하지 않는 별개 행동

        Debug.Log($"[CampCharSelection] 스토리 대화 시작 (TODO)");
    }

    /// <summary>
    /// 캐릭터 체력 회복
    /// </summary>
    private void HealCharacter(int healAmount)
    {
        if (character == null) return;

        character.currentHP += healAmount;
        // Clamp은 PlayerData의 currentHP 프로퍼티 setter에서 자동 처리됨
    }

    /// <summary>
    /// 휴식 모션 재생
    /// </summary>
    private void PlayRestAnimation()
    {
        // 각 캐릭터의 자는 모션 연출
        Debug.Log($"[CampCharSelection] {character.CharacterName} 휴식 모션 재생");
    }

    /// <summary>
    /// 카드 제거 UI 표시 (현재 PopupUI_Maintenance에서 처리)
    /// </summary>
    private void ShowCardRemovalUI()
    {
        // 화면 중앙에 덱 카드 제거 UI 표시
    }

    /// <summary>
    /// 캐릭터 상호작용 완료 처리
    /// </summary>
    private void MarkCharacterInteractionComplete()
    {
        DisableInteractionButtons();
        OnCharacterInteractionComplete?.Invoke();
    }

    /// <summary>
    /// 상호작용 버튼 비활성화 (휴식, 정비)
    /// </summary>
    private void DisableInteractionButtons()
    {
        if (restButton != null)
            restButton.SetActive(false);
        if (maintenanceButton != null)
            maintenanceButton.SetActive(false);
    }

    /// <summary>
    /// 스토리 버튼 활성화/비활성화
    /// </summary>
    public void SetStoryButtonActive(bool active)
    {
        if (storyButton != null)
        {
            storyButton.SetActive(active);
        }
    }
}

