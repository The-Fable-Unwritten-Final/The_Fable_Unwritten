using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiaryBook : MonoBehaviour, IBookControl
{
    // 각 스테이지 별 다이어리 데이터 (1 스테이지 ~ 5 스테이지)
    [Header("Diary Data")]
    [SerializeField] string csvPath = "ExternalFiles/DiaryData.csv"; // 다이어리 데이터 csv 경로
    Dictionary <int,DiaryData> Diary0 = new (); // <스토리 진행정도 , 스토리 내용> 구조로 기록
    Dictionary <int,DiaryData> Diary1 = new ();
    Dictionary <int,DiaryData> Diary2 = new ();
    Dictionary <int,DiaryData> Diary3 = new ();
    Dictionary <int,DiaryData> Diary4 = new ();

    [Header("UI Selection / Info")]
    [SerializeField] List<Transform> diaryClip;
    [SerializeField] TextMeshProUGUI diaryTitle; // 다이어리 제목
    [SerializeField] TextMeshProUGUI diaryContents; // 다이어리 내용
    public int diarySelection = 0; // 다이어리 선택 0 ~ 4
    public int maxPageCount { get; set; } // 페이지 수
    public int currentPage { get; set; } = 0; // 현재 페이지
 
    void OnEnable()
    {
        // 처음 다이어리 페이지를 열었을때 최초 페이지 == 0번 인덱스의 카드페이지
        OnClickClip(diaryClip[0],0);
    }

    public void OnClick0()
    {
        OnClickClip(diaryClip[0],0);
    }
    public void OnClick1()
    {
        OnClickClip(diaryClip[1],1);
    }
    public void OnClick2()
    {
        OnClickClip(diaryClip[2],2);
    }
    public void OnClick3()
    {
        OnClickClip(diaryClip[3],3);
    }
    public void OnClick4()
    {
        OnClickClip(diaryClip[4],4);
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
    }

    public void InitDictionary()
    {
        var list = DiaryCSVParser.Parse(csvPath);

        // 추후 스테이지의 진행도 데이터를 받아와서, diaryData의 isOpen를 설정.
        // EX) int storyComplete == 11 이면, list의 0~10까지의 isOpen을 true로 설정.

        // Parse 한 데이터를 각 딕셔너리에 저장.
        for (int i = 0; i < list.Count; i++)
        {
            switch (list[i].stageNum)
            {
                case 0:
                    Diary0.Add(list[i].index, list[i]);
                    break;
                case 1:
                    Diary1.Add(list[i].index, list[i]);
                    break;
                case 2:
                    Diary2.Add(list[i].index, list[i]);
                    break;
                case 3:
                    Diary3.Add(list[i].index, list[i]);
                    break;
                case 4:
                    Diary4.Add(list[i].index, list[i]);
                    break;
            }
        }
    }

    private void OnClickClip(Transform t,int selection)// 클릭한 책갈피 최초 오픈
    {
        SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        currentPage = 0; // 페이지 초기화
        diarySelection = selection; // 다이어리 선택
        UpdatePage(0,selection); // 다이어리 내용 업데이트
        SetLastSibling(t); // 클릭한 책갈피를 가장 위로 올림
    }
    void UpdatePage(int i,int selection)// 다이어리 내용 업데이트
    {
        switch (selection)
        {
            case 0:
                maxPageCount = Diary0.Count;
                diaryTitle.text = Diary0[i].title; // 다이어리 제목
                diaryContents.text = Diary0[i].contents; // 다이어리 내용
                break;
            case 1:
                maxPageCount = Diary1.Count;
                diaryTitle.text = Diary1[i].title; // 다이어리 제목
                diaryContents.text = Diary1[i].contents; // 다이어리 내용
                break;
            case 2:
                maxPageCount = Diary2.Count;
                diaryTitle.text = Diary2[i].title; // 다이어리 제목
                diaryContents.text = Diary2[i].contents; // 다이어리 내용
                break;
            case 3:
                maxPageCount = Diary3.Count;
                diaryTitle.text = Diary3[i].title; // 다이어리 제목
                diaryContents.text = Diary3[i].contents; // 다이어리 내용
                break;
            case 4:
                maxPageCount = Diary4.Count;
                diaryTitle.text = Diary4[i].title; // 다이어리 제목
                diaryContents.text = Diary4[i].contents; // 다이어리 내용
                break;
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
}
