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

    private void Update()
    {
        // Space 키로 다음 대화 진행
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            bool wasSkipped = false;
            
            // TypeWriter 실행 중이면 스킵
            if (currentSpeakerIndex >= 0 && campCharSelections[currentSpeakerIndex] != null)
            {
                wasSkipped = campCharSelections[currentSpeakerIndex].SkipTypeWriter();
            }
            
            // 텍스트가 출력중 스킵을 누를 시, 전체 텍스트 표기. 텍스트가 이미 다 출력되었을때 스킵을 누를 시, 다음 대화로 진행
            if (!wasSkipped)
            {
                ShowNextDialogueLine();
            }
        }
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

        // 특정 대화도 사용 완료 목록에 추가
        ProgressDataManager.Instance.usedCampTalks.Add(currentTalkData.GetInstanceID());
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
