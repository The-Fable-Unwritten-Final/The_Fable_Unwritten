using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 캠프 대화 진행 관리
/// CampTalkData의 조건을 체크하고 대화 시퀀스를 재생
/// Space 키 입력으로 다음 라인 진행
/// </summary>
public class CampTalkController : MonoBehaviour
{
    [SerializeField] private CampCharSelection[] campCharSelections; // 4명의 캐릭터 (0=Kyla, 1=Sophia, 2=Leon, 3=Dorothy)

    private CampTalkData currentTalkData;
    private int currentTextIndex = 0;
    private bool isDialogueActive = false;
    private int currentSpeakerIndex = -1; // 현재 말하고 있는 캐릭터 인덱스

    private void Start()
    {
    }

    private void Update()
    {
        // Space 키로 다음 대화 진행
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            // TypeWriter 실행 중이면 스킵
            if (currentSpeakerIndex >= 0 && campCharSelections[currentSpeakerIndex] != null)
            {
                campCharSelections[currentSpeakerIndex].SkipTypeWriter();
            }
            
            ShowNextDialogueLine();
        }
    }

    /// <summary>
    /// 랜덤 대화 시작
    /// 조건을 만족하는 CampTalkData 중에서 랜덤으로 선택하여 재생
    /// </summary>
    public void StartRandomDialogue()
    {
        // 조건을 만족하는 대화 데이터 모두 수집
        var validTalks = DataManager.Instance.campTalkDataList
            .Where(talk => talk != null && talk.IsValid())
            .ToList();

        if (validTalks.Count == 0)
        {
            Debug.LogWarning("[CampTalkController] 조건을 만족하는 대화가 없습니다.");
            return;
        }

        // 랜덤 선택
        currentTalkData = validTalks[Random.Range(0, validTalks.Count)];
        currentTextIndex = 0;
        isDialogueActive = true;

        ShowNextDialogueLine();
    }

    /// <summary>
    /// CSV key에서 캐릭터 코드 파싱
    /// 예: "Camp_OT_01_01_K" → K (Kayla=0)
    /// </summary>
    private int GetSpeakerIndexFromKey(string csvKey)
    {
        if (string.IsNullOrEmpty(csvKey) || csvKey.Length < 1)
            return 0; // 기본값

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
    /// 다음 대화 라인 표시
    /// CSV key의 마지막 캐릭터 코드에서 话者 결정
    /// 예: "Camp_OT_01_01_K" → Kayla가 말함
    /// </summary>
    public void ShowNextDialogueLine()
    {
        if (currentTalkData == null || currentTextIndex >= currentTalkData.keyTextSerial.Count)
        {
            EndDialogue();
            return;
        }

        // 현재 텍스트 키 가져오기
        string textKey = currentTalkData.keyTextSerial[currentTextIndex];
        
        // CSV key에서 캐릭터 코드 파싱 (마지막 문자: K, S, L, D)
        int speakerIndex = GetSpeakerIndexFromKey(textKey);
        
        // 현재 스피커 저장
        currentSpeakerIndex = speakerIndex;
 
        // 현재 말하는 캐릭터가 null이면 스킵
        if (campCharSelections[speakerIndex] == null)
        {
            currentTextIndex++;
            ShowNextDialogueLine();
            return;
        }

        // 다른 캐릭터의 대화 박스 모두 숨기기
        for (int i = 0; i < campCharSelections.Length; i++)
        {
            if (i != speakerIndex && campCharSelections[i] != null)
            {
                campCharSelections[i].HideDialogue();
            }
        }

        // 텍스트 출력
        string localizedText = LocaleDataManager.GetLocalizedCampTalk(textKey);
        campCharSelections[speakerIndex].ShowDialogue(localizedText);
        currentTextIndex++;
    }

    /// <summary>
    /// 특정 대화 시작 (UI_CampController에서 선택한 대화 재생)
    /// </summary>
    public void StartSpecificDialogue(CampTalkData talkData)
    {
        if (talkData == null)
        {
            Debug.LogWarning("[CampTalkController] 전달받은 CampTalkData가 null입니다.");
            return;
        }

        currentTalkData = talkData;
        currentTextIndex = 0;
        isDialogueActive = true;

        Debug.Log($"[CampTalkController] 특정 대화 시작: {talkData.name}");
        ShowNextDialogueLine();
    }

    /// <summary>
    /// 대화 종료
    /// </summary>
    private void EndDialogue()
    {
        isDialogueActive = false;
        currentTalkData = null;
        currentTextIndex = 0;

        // 모든 캐릭터의 대화 UI 숨기기
        foreach (var campChar in campCharSelections)
        {
            if (campChar != null)
            {
                campChar.HideDialogue();
            }
        }
    }

    /// <summary>
    /// 현재 대화가 진행 중인지 여부
    /// </summary>
    public bool IsDialogueActive => isDialogueActive;
}
