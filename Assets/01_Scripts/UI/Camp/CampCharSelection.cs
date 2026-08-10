using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

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

    [Header("Dialog System")]
    [SerializeField] private CampTalkController campTalkController; // 캠프 대화 컨트롤러
    private UI_CampController campController; // 캠프 컨트롤러 참조

    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI dialogueText; // 대화 텍스트 표시
    [SerializeField] private GameObject dialogueBox; // 대화 박스 (선택사항)

    // 캐릭터 상호작용 완료
    public System.Action OnCharacterInteractionComplete;
    
    // ? 버튼 클릭 이벤트
    public System.Action OnStoryButtonClickedEvent;

    // 캠프 선택 상태 추적
    private bool isRested = false; // 휴식 선택 여부
    private List<CardModel> selectedCardsToRemove = new(); // 정비에서 제거할 카드 목록
    private Coroutine typeWriterCoroutine; // TypeWriter 코루틴 참조
    private bool isTypeWriterActive = false; // TypeWriter 실행 중 플래그
    private string currentFullText = ""; // 현재 표시 중인 전체 텍스트
    
    // 호버 효과 제어
    private bool hoverEnabled = true; // 호버 효과 활성화 여부
    private Vector3 restButtonInitialScale = Vector3.one; // 휴식 버튼 초기 스케일
    private Vector3 maintenanceButtonInitialScale = Vector3.one; // 정비 버튼 초기 스케일

    private void Start()
    {
        // 캠프 컨트롤러 참조 가져오기
        if (campController == null)
        {
            campController = GetComponentInParent<UI_CampController>();
        }

        // 휴식 버튼에 호버 이벤트 추가 및 초기 스케일 저장
        if (restButton != null)
        {
            restButtonInitialScale = restButton.transform.localScale;
            AddHoverEffectToButton(restButton, true);
        }

        // 정비 버튼에 호버 이벤트 추가 및 초기 스케일 저장
        if (maintenanceButton != null)
        {
            maintenanceButtonInitialScale = maintenanceButton.transform.localScale;
            AddHoverEffectToButton(maintenanceButton, false);
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

        // 2. 회복 텍스트 애니메이션 표시
        if (campController != null)
        {
            campController.ShowHealTextAnimation(transform);
        }

        // 3. 휴식 선택 상태 표시 (체력 회복은 씬 종료 시에 일괄 처리)
        isRested = true;
        selectedCardsToRemove.Clear();

        // 4. 상호작용 완료 표시
        MarkCharacterInteractionComplete();
    }

    /// <summary>
    /// 정비 버튼 클릭
    /// </summary>
    public void OnMaintenanceButtonClicked()
    {
        if (character == null) return;

        // PopupUI_Maintenance 팝업 열기 (CampCharSelection만 전달)
        PopupUI_Maintenance maintenancePopup = UIManager.Instance.ShowPopup<PopupUI_Maintenance>();
        if (maintenancePopup != null)
        {
            maintenancePopup.ShowMaintenance(this);
        }
    }

    /// <summary>
    /// 정비 완료 콜백 (카드 제거 UI에서 호출)
    /// </summary>
    public void OnMaintenanceComplete()
    {
        isRested = false; // 정비 선택
        MarkCharacterInteractionComplete();
    }

    /// <summary>
    /// 제거할 카드 추가 (PopupUI_Maintenance에서 호출)
    /// </summary>
    public void AddCardToRemoval(CardModel card)
    {
        if (card != null && !selectedCardsToRemove.Contains(card))
        {
            selectedCardsToRemove.Add(card);
        }
    }

    /// <summary>
    /// 캠프 씬에서 호출 - 휴식 여부 반환
    /// </summary>
    public bool GetIsRested() => isRested;

    /// <summary>
    /// 캠프 씬에서 호출 - 제거할 카드 목록 반환
    /// </summary>
    public List<CardModel> GetCardsToRemove() => new List<CardModel>(selectedCardsToRemove);

    /// <summary>
    /// 스토리 대화 버튼 클릭
    /// </summary>
    public void OnStoryButtonClicked()
    {
        // ? 버튼 비활성화
        SetStoryButtonActive(false);

        // UI_CampController에 알림
        OnStoryButtonClickedEvent?.Invoke();
    }

    /// <summary>
    /// 대화 텍스트 표시 (CampTalkController에서 호출)
    /// TypeWriter 효과로 순차적으로 텍스트 표시
    /// </summary>
    public void ShowDialogue(string text)
    {
        // 기존 TypeWriter 코루틴 중지
        if (typeWriterCoroutine != null)
        {
            StopCoroutine(typeWriterCoroutine);
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        // 현재 텍스트 저장
        currentFullText = text;
        isTypeWriterActive = true;

        // 새로운 TypeWriter 코루틴 시작
        typeWriterCoroutine = StartCoroutine(TypeWriterEffect(text));
    }

    /// <summary>
    /// TypeWriter 효과 - 텍스트를 문자 하나씩 표시
    /// </summary>
    private IEnumerator TypeWriterEffect(string text, float charDelay = 0.1f) // 기본 딜레이 0.1초 >> 변경 가능
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        typeWriterCoroutine = null;
        isTypeWriterActive = false;
    }

    /// <summary>
    /// TypeWriter 효과 스킵 - 타이핑 애니메이션을 건너뛰고 전체 텍스트 표시
    /// 실제로 스킵했는지 여부를 반환
    /// 타이핑 중이었으면 true, 이미 완료되었으면 false
    /// </summary>
    public bool SkipTypeWriter()
    {
        // 타이핑 중인 경우만 스킵 (타이핑이 완료되면 다음 진행)
        if (typeWriterCoroutine != null)
        {
            StopCoroutine(typeWriterCoroutine);
            typeWriterCoroutine = null;

            if (dialogueText != null)
            {
                dialogueText.text = currentFullText;
            }

            isTypeWriterActive = false;
            return true; // 실제로 스킵함
        }

        // 이미 완료됨
        return false;
    }

    /// <summary>
    /// 대화 텍스트 숨기기 (CampTalkController에서 호출)
    /// </summary>
    public void HideDialogue()
    {
        // TypeWriter 코루틴 중지
        if (typeWriterCoroutine != null)
        {
            StopCoroutine(typeWriterCoroutine);
            typeWriterCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
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
    /// 상호작용 버튼 비활성화 (휴식, 정비) - DOTween 스케일 애니메이션
    /// </summary>
    private void DisableInteractionButtons()
    {
        // 호버 효과 비활성화 (비활성화 애니메이션에 영향 받지 않도록)
        hoverEnabled = false;

        if (restButton != null)
        {
            restButton.transform.DOScale(0f, 0.3f).OnComplete(() => restButton.SetActive(false));
        }
        if (maintenanceButton != null)
        {
            maintenanceButton.transform.DOScale(0f, 0.3f).OnComplete(() => maintenanceButton.SetActive(false));
        }
    }

    /// <summary>
    /// 스토리 버튼 활성화/비활성화 - DOTween 스케일 애니메이션
    /// </summary>
    public void SetStoryButtonActive(bool active)
    {
        if (storyButton != null)
        {
            if (active)
            {
                storyButton.SetActive(true);
                storyButton.transform.DOScale(0.5f, 0.3f);
            }
            else
            {
                storyButton.transform.DOScale(0f, 0.3f).OnComplete(() => storyButton.SetActive(false));
            }
        }
    }

    /// <summary>
    /// 캠프 선택 상태 초기화 (새 팝업 열 때)
    /// </summary>
    public void ResetCampState()
    {
        isRested = false;
        selectedCardsToRemove.Clear();
    }

    /// <summary>
    /// 버튼에 호버 효과 추가
    /// </summary>
    private void AddHoverEffectToButton(GameObject button, bool isRestButton)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.AddComponent<EventTrigger>();
        }

        // PointerEnter 이벤트 추가
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => OnButtonHoverEnter(button, isRestButton));
        trigger.triggers.Add(entryEnter);

        // PointerExit 이벤트 추가
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => OnButtonHoverExit(button, isRestButton));
        trigger.triggers.Add(entryExit);
    }

    /// <summary>
    /// 버튼 호버 Enter - 스케일 1.1배로 확대
    /// </summary>
    private void OnButtonHoverEnter(GameObject button, bool isRestButton)
    {
        if (!hoverEnabled) return;

        Vector3 initialScale = isRestButton ? restButtonInitialScale : maintenanceButtonInitialScale;
        button.transform.DOScale(initialScale * 1.1f, 0.2f);
    }

    /// <summary>
    /// 버튼 호버 Exit - 초기 스케일로 복구
    /// </summary>
    private void OnButtonHoverExit(GameObject button, bool isRestButton)
    {
        if (!hoverEnabled) return;

        Vector3 initialScale = isRestButton ? restButtonInitialScale : maintenanceButtonInitialScale;
        button.transform.DOScale(initialScale, 0.2f);
    }
}