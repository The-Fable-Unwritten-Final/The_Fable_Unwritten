using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupUI_Book : BasePopupUI
{
    [SerializeField] List<Transform> bookClip;
    [SerializeField] List<RectTransform> pages; // 0: 카드 페이지, 1 : 이상 실현 페이지, 2 : 일기 페이지 3: 카드 팝업 페이지


    private void OnEnable()
    {
        // 처음 책을 열었을때 최초 페이지 == 0번 인덱스의 카드페이지
        OnClick0();
    }
    // 클릭한 책갈피가 펼쳐줄 캐릭터 카드 종류(해당 데이터를 cardPage 에서 받아서 보여줄 카드 업데이트) 1: 소피아, 2: 카일라, 3: 레온
    public void OnClick0()
    {
        OnClickClip(bookClip[0], 0);
    }
    public void OnClick1()
    {
        OnClickClip(bookClip[1], 1);
    }
    public void OnClick2()
    {
        OnClickClip(bookClip[2], 2);
    }
    void OnClickClip(Transform t, int i)
    {
        SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        SetAllPageClose(); // 모든 페이지 비활성화
        pages[0].gameObject.SetActive(true); // 카드 페이지 활성화
        // i번 캐릭터에 맞는 카드 페이지 init.
        SetLastSibling(t); // 클릭한 책갈피를 가장 위로 올림
    }// i번 캐릭터에 맞는 카드 페이지 펼치기 래핑. (i == 0 : 소피아, 1 : 카일라, 2: 레온)

    public void OnClickIdle()// 이상 실현 버튼
    {
        SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        SetAllPageClose(); // 모든 페이지 비활성화
        pages[1].gameObject.SetActive(true); // 이상 실현 페이지 활성화
        // 이상 실현 페이지 초기화.
    }
    public void OnClickDiary()// 일기장 버튼
    {
        SetAllToFirst(); // 모든 책갈피를 가장 아래로 내림
        SetAllPageClose(); // 모든 페이지 비활성화
        pages[2].gameObject.SetActive(true); // 일기장 페이지 활성화
        // 일기장 페이지 초기화.
    }
    public void OnClickCard()// 카드 페이지의 카드 클릭시 해당 카드의 정보를 담은 팝업 띄움
    {
        pages[3].gameObject.SetActive(true);
        // 카드 팝업 페이지에 해당 카드의 정보 표시.
    }
    void SetLastSibling(Transform t)
    {
        t.SetAsLastSibling();
    }// 클릭한 책갈피를 가장 위로 올림
    void SetAllToFirst()
    {
        foreach (var item in bookClip)
        {
            item.SetAsFirstSibling();
        }
    }// 모든 책갈피를 가장 아래로 내림
    void SetAllPageClose()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].gameObject.SetActive(false); // 모든 페이지 비활성화
        }
    }// 책의 모든 페이지 비활성화
}
