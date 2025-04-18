using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBookControl
{
    int maxPageCount { get; set; } // 페이지 수
    int currentPage { get; set; } // 현재 페이지

    void OnclickPageBefore();
    void OnclickPageAfter();
}
