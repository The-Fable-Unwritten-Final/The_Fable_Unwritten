using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdealBook : MonoBehaviour ,IBookControl
{
    // 페이지 인터페이스
    public int maxPageCount { get; set; } = 3;// 페이지 수
    public int currentPage { get; set; } = 0; // 현재 페이지
   

    // 상호작용 UI
    [SerializeField] RectTransform leftArrow; // 왼쪽 화살표
    [SerializeField] RectTransform rightArrow; // 오른쪽 화살표

    private void OnEnable()
    {
        currentPage = 0; // 페이지 초기화
        UpdateArrow(); // 화살표 업데이트
    }

    
    public void OnclickPageBefore()
    {
        if(currentPage > 0)
        {
            currentPage--;
        }
        else
        {
        }
        UpdateArrow(); // 화살표 업데이트
    }
    public void OnclickPageAfter()
    {
        if(currentPage < maxPageCount - 1)
        {
            currentPage++;
        }
        else
        {
        }
        UpdateArrow(); // 화살표 업데이트
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
