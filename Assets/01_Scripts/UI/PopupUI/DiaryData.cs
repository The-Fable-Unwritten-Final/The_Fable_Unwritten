using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DiaryData 
{
    public int index; // 다이어리 인덱스
    public int tag_num; // 스테이지 번호
    public string title; // 다이어리 제목
    public string contents; // 다이어리 내용
    public bool isOpen; // 다이어리 열림 여부 (스테이지 진행에 따라 진행)
}
