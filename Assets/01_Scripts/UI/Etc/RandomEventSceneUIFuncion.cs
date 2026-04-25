using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEventSceneUIFuncion : MonoBehaviour
{
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
    /// <summary>
    /// UI전반의 구조가 바뀌며 각 씬에 존재하는 미니맵 UI를 개별로 UI매니저와 연결 해 주는 메서드
    /// </summary>
    public void MiniMapUIManagerOpen()
    {
        UIManager.Instance.ShowPopupByName("PopupUI_MiniMap");
    }
}
