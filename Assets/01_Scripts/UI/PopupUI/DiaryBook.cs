using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryBook : MonoBehaviour, IBookControl
{

    [SerializeField] List<Transform> diaryClip;
    public int maxPageCount { get; set; } // 페이지 수
    public int currentPage { get; set; } = 0; // 현재 페이지
 
    private void OnEnable()
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
    }

    void OnClickClip(Transform t,int i)
    {
        SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        // 다이어리의 내용 초기화. (첫번째 책갈피의 첫페이지 내용으로)
        SetLastSibling(t); // 클릭한 책갈피를 가장 위로 올림
    }

    private void SetLastSibling(Transform t)
    {
        t.SetAsLastSibling();
    }
    private void SetAllToFirst()
    {
        for (int i = 0; i < diaryClip.Count; i++)
        {
            diaryClip[i].SetAsFirstSibling();
        }
    }
}
