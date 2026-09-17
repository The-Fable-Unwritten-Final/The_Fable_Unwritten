using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;


/// 1. 휴식 OR 정비 선택지가 존재한다
/// 2. 각 선택지는 선택이 완료 후, 캠프 씬 종료 시점에서 효과가 적용된다.
public class UI_CampController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] Image background;

    [Header("Character Interactions")]
    [SerializeField] private CampCharSelection[] campCharSelections; // 4명의 캐릭터 (0=Kyla, 1=Sophia, 2=Leon, 3=Dorothy)
    [SerializeField] private CampTalkController campTalkController; // 캠프 대화 컨트롤러
    [SerializeField] private GameObject healText; // 체력 회복 텍스트

    private int completedCharacterCount = 0;
    private const int REQUIRED_CHARACTERS = 3; // 3명의 플레이어블 캐릭터만 필요
    private const float EXIT_DELAY = 2f; // 모두 완료 후 종료 대기 시간

    private int storyCharacterIndex = -1; // ? 버튼을 표시할 캐릭터 인덱스
    private CampTalkData currentSelectedTalkData; // 현재 선택된 대화 (? 버튼 대상)
    private Sequence healTextSequence; // 회복 텍스트 애니메이션 Sequence

    private void Start()
    {
        ProgressDataManager.Instance.IsNewStage = false;

        completedCharacterCount = 0;

        // 디버그: 캠프 씬 진입 시 현재 상태 확인
        var setting = ProgressDataManager.Instance;
        Debug.Log($"[CampController] 진입 시 - StageCleared: {setting.StageCleared}, CurrentNode: {setting.CurrentNode?.type}, VisitedNodes.Count: {setting.VisitedNodes.Count}");

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
        // 회복 텍스트 애니메이션 Kill
        if (healTextSequence != null)
        {
            healTextSequence.Kill();
        }

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
    /// 조건을 만족하는 첫 대화의 첫 번째 스피커에게 버튼 표시
    /// </summary>
    private void SelectRandomStoryCharacter()
    {
        // 조건을 만족하고 아직 사용되지 않은 대화 데이터 수집
        var validTalks = DataManager.Instance.campTalkDataList
            .Where(talk => talk != null && talk.IsValid())
            .Where(talk => !ProgressDataManager.Instance.usedCampTalks.Contains(talk.GetInstanceID()))
            .ToList();

        if (validTalks.Count == 0)
        {
            Debug.LogWarning("[CampController] 조건을 만족하는 미사용 대화가 없습니다.");
            return;
        }

        // 랜덤으로 하나 선택
        currentSelectedTalkData = validTalks[Random.Range(0, validTalks.Count)];

        // 선택된 대화의 첫 번째 라인에서 스피커 파싱
        if (currentSelectedTalkData.keyTextSerial.Count > 0)
        {
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ 아래의 디버그문 절대 지우지 말것, 지우면 버그 생김.. @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // >> 일단 고치긴 했는데, 혹시 ? 와는 다른 캐릭터가 대화를 시작하는 버그가 생길 경우, 아래의 디버그 문을 다시 사용할것.
            string firstKey = currentSelectedTalkData.keyTextSerial[0];
            //Debug.Log($"[SelectRandomStoryCharacter] 첫 번째 키: '{firstKey}'");
            
            char lastChar = firstKey[firstKey.Length - 1];
            //Debug.Log($"[SelectRandomStoryCharacter] 마지막 문자: '{lastChar}'");
            
            int speakerIndex = GetSpeakerIndexFromKey(firstKey);
            //Debug.Log($"[SelectRandomStoryCharacter] 파싱된 인덱스: {speakerIndex} (K=0, S=1, L=2, D=3)");
            //Debug.Log($"[SelectRandomStoryCharacter] 버튼 표시 대상: {(speakerIndex < campCharSelections.Length && campCharSelections[speakerIndex] != null ? campCharSelections[speakerIndex].character?.CharacterName : "ERROR")}");

            // 해당 캐릭터에만 스토리 버튼 표시
            storyCharacterIndex = speakerIndex;
            if (campCharSelections[storyCharacterIndex] != null)
            {
                campCharSelections[storyCharacterIndex].SetStoryButtonActive(true);
                
                // ? 버튼 클릭 이벤트 등록 - 선택된 대화 시작
                campCharSelections[storyCharacterIndex].OnStoryButtonClickedEvent += OnStoryButtonClicked;
            }
        }
    }

    /// <summary>
    /// ? 버튼 클릭 콜백 - UI_CampController에서 선택한 대화 시작
    /// </summary>
    private void OnStoryButtonClicked()
    {
        if (campTalkController != null && currentSelectedTalkData != null)
        {
            campTalkController.StartSpecificDialogue(currentSelectedTalkData);
        }
    }

    /// <summary>
    /// CSV key에서 캐릭터 코드 파싱
    /// 예: "Camp_OT_01_01_K" → K (Kayla=0)
    /// </summary>
    private int GetSpeakerIndexFromKey(string csvKey)
    {
        if (string.IsNullOrEmpty(csvKey) || csvKey.Length < 1)
            return 0;

        char lastChar = csvKey[csvKey.Length - 1];

        return lastChar switch
        {
            'K' => 0, // Kayla
            'S' => 1, // Sophia
            'L' => 2, // Leon
            'D' => 3, // Dorothy
            _ => 0    // 기본값
        };
    }

    /// <summary>
    /// 캐릭터 상호작용 완료 콜백
    /// </summary>
    private void OnCharacterComplete()
    {
        completedCharacterCount++;
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
        yield return new WaitForSeconds(EXIT_DELAY);
        ExitCampScene();
    }

    /// <summary>
    /// 회복 텍스트 애니메이션 표시
    /// 1.5초간 위로 올라가면서 투명해지는 효과
    /// </summary>
    public void ShowHealTextAnimation(Transform b,Transform characterTransform)
    {
        if (healText == null) return;

        RectTransform healTextRect = healText.GetComponent<RectTransform>();
        RectTransform characterRect = characterTransform.GetComponent<RectTransform>();
        
        if (healTextRect != null && characterRect != null)
        {
            // 1. healText의 부모를 characterTransform(CampCharSelection)으로 설정
            healTextRect.SetParent(b);
            
            // 2. healFX의 위치에 배치 (characterRect의 localPosition 사용)
            healTextRect.localPosition = characterRect.localPosition;
        }

        // 3. 회복 텍스트 활성화 및 초기 색상 설정
        healText.SetActive(true);
        TextMeshProUGUI tmpText = healText.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            Color color = tmpText.color;
            color.a = 1f;
            tmpText.color = color;
        }

        // 4. 기존 애니메이션 Sequence Kill
        if (healTextSequence != null)
        {
            healTextSequence.Kill();
        }

        // 5. DOTween 애니메이션: 1.5초간 위로 올라가면서 투명해지는 효과
        healTextSequence = DOTween.Sequence();
        
        // 위치 애니메이션: Y축으로 100만큼 위로 이동
        healTextSequence.Join(healTextRect.transform.DOLocalMove(new Vector3(healTextRect.transform.localPosition.x, healTextRect.transform.localPosition.y + 100f, 0), 1.5f).SetEase(Ease.OutQuad));
        
        // 투명도 애니메이션: 0으로 변경
        if (tmpText != null)
        {
            healTextSequence.Join(tmpText.DOFade(0f, 1.5f).SetEase(Ease.InQuad));
        }
        
        // 애니메이션 완료 후 비활성화
        healTextSequence.OnComplete(() =>
        {
            healText.SetActive(false);
        });
    }

    /// <summary>
    /// 캠프 씬 종료
    /// </summary>
    private void ExitCampScene()
    {
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
                }

                // 정비 선택 시: 제거 카드 일괄 처리
                List<CardModel> cardsToRemove = campChar.GetCardsToRemove();
                if (cardsToRemove.Count > 0)
                {
                    foreach (var card in cardsToRemove)
                    {
                        character.currentDeck.Remove(card);
                        GameManager.Instance.analyticsLogger.LogCardRemoveInfo(card.index); // 애널리틱스 기록
                    }
                    character.UpdateCurrentDeckIndexes();
                }
            }
        }
        
        // 현재 노드 클리어 상태 업데이트
        var setting = ProgressDataManager.Instance;
        // 캠프는 "스테이지 클리어"가 아니므로 StageCleared는 false로 유지
        // (첫 번째 노드는 이미 VisitedNodes에 저장되어 있음)
        setting.StageCleared = false;
        setting.RetryFromStart = false;  // ← 중요: 캠프에서 돌아올 때 복구 가능하도록 설정
        
        // IsNewStage 판정 (보스 또는 특정 조건)
        if (setting.CurrentNode.type == NodeType.Boss || 
            (setting.StageIndex == 1 && setting.CurrentNode.columnIndex == 3))
        {
            setting.IsNewStage = true;
        }
        else
        {
            setting.IsNewStage = false;
        }
        
        // 캠프 상호작용 완료 후 CurrentNode를 null로 설정 (다음 노드 선택 가능하도록)
        setting.CurrentNode = null;
        // 스테이지 상태 저장
        if (setting.SavedStageData != null)
        {
            setting.SaveStageState(setting.SavedStageData, setting.VisitedNodes);
        }
        
        // 변경된 데이터 저장
        ProgressDataManager.Instance.SaveProgress(true);
       
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

