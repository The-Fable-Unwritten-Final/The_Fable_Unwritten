using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class ScenePopupController : MonoBehaviour
{

    private void Awake()
    {
        UIManager.Instance.RegisterPopup(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    private void OnSceneUnloaded(Scene scene)
    {
        UIManager.Instance.UnregisterPopup();// 씬 나갈 때 UI 팝업 컨트롤러 해제
    }

    // 씬에서 특정 팝업을 오픈시 버튼에서 호출할 메서드 (팝업할 UI 프리팹의 이름과 동일하게 작성)
    public void OpenPopup(string popupName)
    {
        UIManager.Instance.ShowPopupByName(popupName);
    }
}
