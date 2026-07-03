using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// 1. 휴식 OR 정비 선택지가 존재한다
/// 2. 각 선택지는 선택이 완료 후, 캠프 씬 종료 시점에서 효과가 적용된다.
public class UI_CampController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] Sprite[] campBackgrounds;

    [Header("Character Interactions")]
    [SerializeField] private CampCharSelection[] campCharSelections; // 4명의 캐릭터 (0=Kyla, 1=Sophia, 2=Leon, 3=Dorothy)

    private int completedCharacterCount = 0;
    private const int REQUIRED_CHARACTERS = 3; // 3명의 플레이어블 캐릭터만 필요
    private const float EXIT_DELAY = 2f; // 모두 완료 후 종료 대기 시간

    private int storyCharacterIndex = -1; // ? 버튼을 표시할 캐릭터 인덱스

    private void Start()
    {
        SetBackground();
        ProgressDataManager.Instance.IsNewStage = false;

        completedCharacterCount = 0;

        // 각 CampCharSelection에 완료 이벤트 등록
        if (campCharSelections != null)
        {
            for (int i = 0; i < campCharSelections.Length; i++)
            {
                if (campCharSelections[i] != null)
                {
                    campCharSelections[i].OnCharacterInteractionComplete += OnCharacterComplete;
                }
            }
        }

        // 랜덤하게 스토리 버튼을 표시할 캐릭터 선택 (4명 중 1명)
        SelectRandomStoryCharacter();
    }

    private void OnDestroy()
    {
        if (campCharSelections != null)
        {
            for (int i = 0; i < campCharSelections.Length; i++)
            {
                if (campCharSelections[i] != null)
                {
                    campCharSelections[i].OnCharacterInteractionComplete -= OnCharacterComplete;
                }
            }
        }
    }

    /// <summary>
    /// 랜덤하게 스토리 버튼을 표시할 캐릭터 선택
    /// </summary>
    private void SelectRandomStoryCharacter()
    {
        storyCharacterIndex = Random.Range(0, campCharSelections.Length);

        if (campCharSelections[storyCharacterIndex] != null)
        {
            campCharSelections[storyCharacterIndex].SetStoryButtonActive(true);
            Debug.Log($"[CampController] 스토리 버튼 표시: 캐릭터 {storyCharacterIndex}");
        }
    }

    // 스테이지에 따른 백그라운드 설정
    private void SetBackground()
    {
        int stageIndex = ProgressDataManager.Instance.StageIndex;

        if (stageIndex >= 2 && stageIndex <= 4)
        {
            int index = stageIndex - 2; // 2스테이지 = 0번 인덱스
            if (index >= 0 && index < campBackgrounds.Length)
            {
                background.sprite = campBackgrounds[index];
            }
        }
    }

    /// <summary>
    /// 캐릭터 상호작용 완료 콜백
    /// </summary>
    private void OnCharacterComplete()
    {
        completedCharacterCount++;
        Debug.Log($"[CampController] 캐릭터 상호작용 완료 ({completedCharacterCount}/{REQUIRED_CHARACTERS})");

        // 모든 플레이어블 캐릭터(3명)가 상호작용을 완료했을 경우
        if (completedCharacterCount >= REQUIRED_CHARACTERS)
        {
            StartCoroutine(ExitCampSceneAfterDelay());
        }
    }

    /// <summary>
    /// 지정된 시간 후 캠프 씬 종료
    /// </summary>
    private IEnumerator ExitCampSceneAfterDelay()
    {
        Debug.Log($"[CampController] 모든 상호작용 완료. {EXIT_DELAY}초 후 캠프 씬 종료");
        yield return new WaitForSeconds(EXIT_DELAY);
        ExitCampScene();
    }

    /// <summary>
    /// 캠프 씬 종료
    /// </summary>
    private void ExitCampScene()
    {
        Debug.Log("[CampController] 캠프 씬 종료 - 선택 사항 처리 중");

        // 모든 캐릭터의 선택 사항 처리
        if (campCharSelections != null)
        {
            for (int i = 0; i < campCharSelections.Length; i++)
            {
                if (campCharSelections[i] == null || campCharSelections[i].character == null)
                    continue;

                CampCharSelection campChar = campCharSelections[i];
                PlayerData character = campChar.character;

                // 휴식 선택 시: 체력 회복 처리
                if (campChar.GetIsRested())
                {
                    character.currentHP += 10;
                    Debug.Log($"[CampController] {character.CharacterName}: 10 체력 회복 (현재 HP: {character.currentHP})");
                }

                // 정비 선택 시: 제거 카드 일괄 처리
                List<CardModel> cardsToRemove = campChar.GetCardsToRemove();
                if (cardsToRemove.Count > 0)
                {
                    foreach (var card in cardsToRemove)
                    {
                        character.currentDeck.Remove(card);
                    }
                    character.UpdateCurrentDeckIndexes();
                    Debug.Log($"[CampController] {character.CharacterName}: {cardsToRemove.Count}개 카드 제거 완료");
                }
            }
        }

        Debug.Log("[CampController] 캠프 선택 사항 처리 완료");
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
    }


    /// <summary>
    /// UI전반의 구조가 바뀌며 각 씬에 존재하는 도감 UI를 개별로 UI매니저와 연결 해 주는 메서드
    /// </summary>
    public void BookUIManagerOpen()
    {
        UIManager.Instance.ShowPopupByName("PopupUI_Book");
    }
    /// <summary>
    /// UI전반의 구조가 바뀌며 각 씬에 존재하는 세팅 UI를 개별로 UI매니저와 연결 해 주는 메서드
    /// </summary>
    public void SettingUIManagerOpen()
    {
        UIManager.Instance.ShowPopupByName("PopupUI_Setting");
    }
}

