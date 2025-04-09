using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoSingleton<UIManager>
{
    // Resources 에서 로드한 프리팹 캐싱
    private Dictionary<string, GameObject> popupPrefabs = new Dictionary<string, GameObject>();

    // 현재 씬 내 인스턴스 캐싱
    private Dictionary<string, BasePopupUI> popupInstances = new Dictionary<string, BasePopupUI>();

    private const string popupPath = "UI/Popup/";
    private Transform popupRoot;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        popupInstances.Clear();

        GameObject popupRootObj = GameObject.Find("Canvas/PopupUI");
        if (popupRootObj == null)
        {
            Debug.Log("PopupUI 의 캔버스가 없습니다.");
            popupRoot = null;
        }
        else
        {
            popupRoot = popupRootObj.transform;         
        }
    }


    /// <summary>
    /// 팝업 Open 매서드(컴포넌트 기준)
    /// </summary>
    public T ShowPopup<T>() where T : BasePopupUI
    {
        string popupName = typeof(T).Name;

        // 이미 인스턴스가 있다면 재사용
        if (popupInstances.TryGetValue(popupName, out BasePopupUI existingPoupup))
        {
            existingPoupup.Open();
            return existingPoupup as T;
        }

        GameObject prefab = null;

        // 프리팹이 캐싱 되어 있는지 확인 후 없으면 캐싱
        if (!popupPrefabs.TryGetValue(popupName, out prefab))
        {
            prefab = Resources.Load<GameObject>($"{popupPath}{popupName}");

            if (prefab == null)
            {
                Debug.LogError($"Resources에 '{popupName}'을 찾을 수 없습니다.");
                return null;
            }
   
            popupPrefabs[popupName] = prefab;
        }

        // 인스턴스 생성 후 캐싱
        GameObject popupObject = Instantiate(prefab, popupRoot);
        T popup = popupObject.GetComponent<T>();

        popupInstances[popupName] = popup;
        popup.Open();
        return popup;
    }

    /// <summary>
    /// 팝업 Close 매서드
    /// </summary>
    public void ClosePopup<T>() where T : BasePopupUI
    {
        string popupName = typeof(T).Name;

        if (popupInstances.TryGetValue(popupName, out BasePopupUI popup))
        {
            popup.Close();
        }
    }
}
