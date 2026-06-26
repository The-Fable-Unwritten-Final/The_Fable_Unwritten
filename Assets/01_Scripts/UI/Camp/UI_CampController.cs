using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
    }
}

