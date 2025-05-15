using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DiaryBook : MonoBehaviour, IBookControl
{
    [Header("UI Selection / Info")]
    [SerializeField] List<Transform> diaryClip;
    [SerializeField] TextMeshProUGUI diaryTitle; // 다이어리 제목
    [SerializeField] TextMeshProUGUI diaryContents; // 다이어리 내용
    [SerializeField] RectTransform leftArrow; // 왼쪽 화살표
    [SerializeField] RectTransform rightArrow; // 오른쪽 화살표

    public int diarySelection = 0; // 다이어리 선택 0 ~ 4
    public int maxPageCount { get; set; } // 페이지 수
    public int currentPage { get; set; } = 0; // 현재 페이지
 
    void OnEnable()
    {
        // 처음 다이어리 페이지를 열었을때 최초 페이지 == 0번 인덱스의 카드페이지
        //OnClickClip(diaryClip[0],0);
        TempBookClip(diaryClip[0],0);
    }

    public void OnClick0()
    {
        //OnClickClip(diaryClip[0],0);
        TempBookClip(diaryClip[0],0);
    }
    public void OnClick1()
    {
        //OnClickClip(diaryClip[1],1);
        TempBookClip(diaryClip[1],1);
    }
    public void OnClick2()
    {
        //OnClickClip(diaryClip[2],2);
        TempBookClip(diaryClip[2],2);
    }
    public void OnClick3()
    {
        //OnClickClip(diaryClip[3],3);
        TempBookClip(diaryClip[3],3);
    }
    public void OnClick4()
    {
        //OnClickClip(diaryClip[4],4);
        TempBookClip(diaryClip[4],4);
    }

    public void OnclickPageBefore()
    {
        if (currentPage > 0)
        {
            currentPage--;
            Debug.Log("이전 페이지로 이동: " + currentPage);
        }
        else
        {
            Debug.Log("첫 페이지입니다.");
        }
        UpdatePage(currentPage, diarySelection); // 다이어리 내용 업데이트
        UpdateArrow(); // 화살표 업데이트
    }
    public void OnclickPageAfter()
    {
        if (currentPage < maxPageCount - 1)
        {
            currentPage++;
            Debug.Log("다음 페이지로 이동: " + currentPage);
        }
        else
        {
            Debug.Log("마지막 페이지입니다.");
        }
        UpdatePage(currentPage, diarySelection); // 다이어리 내용 업데이트
        UpdateArrow(); // 화살표 업데이트
    }

    private void OnClickClip(Transform t,int selection)// 클릭한 책갈피 최초 오픈
    {
        SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        currentPage = 0; // 페이지 초기화
        diarySelection = selection; // 다이어리 선택
        UpdatePage(0,selection); // 다이어리 내용 업데이트
        UpdateArrow(); // 화살표 업데이트
        SetLastSibling(t); // 클릭한 책갈피를 가장 위로 올림
    }
    // 배포까지만 사용할 첫번째 스토리만 표시할 임시 메서드
    private void TempBookClip(Transform t, int selection)
    {
        if(selection == 0)
        {
            SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
            currentPage = 0; // 페이지 초기화
            diarySelection = selection;
            UpdatePage(0, selection); // 다이어리 내용 업데이트
            leftArrow.gameObject.SetActive(false); // 왼쪽 화살표 비활성화
            rightArrow.gameObject.SetActive(false); // 오른쪽 화살표 비활성화
            SetLastSibling(t); // 클릭한 책갈피를 가장 위로 올림
        }
        else
        {
            SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
            diaryTitle.text = ""; // 다이어리 제목 초기화
            diaryContents.text = ""; // 다이어리 내용 초기화
            leftArrow.gameObject.SetActive(false); // 왼쪽 화살표 비활성화
            rightArrow.gameObject.SetActive(false); // 오른쪽 화살표 비활성화
            SetLastSibling(t); // 클릭한 책갈피를 가장 위로 올림
        }
    }
    void UpdatePage(int pagenum,int selection)// 다이어리 내용 업데이트
    {
        if(selection < 0 || selection >= diaryClip.Count)
        {
            Debug.LogError("Invalid diary selection: " + selection);
            return;
        }

        var list = DataManager.Instance.DiaryGroups[selection];
        maxPageCount = list.Count;

        if (pagenum >= 0 && pagenum < list.Count)
        {
            diaryTitle.text = list[pagenum].title;
            diaryContents.text = list[pagenum].contents;
        }
    }
    void SetLastSibling(Transform t)
    {
        t.SetAsLastSibling();
    }
    void SetAllToFirst()
    {
        for (int i = 0; i < diaryClip.Count; i++)
        {
            diaryClip[i].SetAsFirstSibling();
        }
    }
    void UpdateArrow()
    {
        if (currentPage == 0) // 첫 페이지일 경우 왼쪽 화살표 비활성화
        {
            leftArrow.gameObject.SetActive(false);
        }
        else
        {
            leftArrow.gameObject.SetActive(true);
        }

        if (currentPage == maxPageCount - 1) // 마지막 페이지일 경우 오른쪽 화살표 비활성화
        {
            rightArrow.gameObject.SetActive(false);
        }
        else
        {
            rightArrow.gameObject.SetActive(true);
        }
    }
}

[System.Serializable]
public class DiaryListWrapper
{
    public List<DiaryData> diaries;
}
