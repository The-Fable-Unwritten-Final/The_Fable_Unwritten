using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoSingleton<UIManager>
{
    // 확장성을 고려한 string을 통한 제네릭 메서드 호출 + Awake 에서 런타임에 Resources 파일을 통해 자동으로 등록.
    // ShowPopupByName() 에서 string 값을 입력하면 Type을 가져올 수 있게함.
    private Dictionary<string, System.Type> popupTypeMap = new();

    // 현재 씬 내 팝업 인스턴스 저장
    private Dictionary<string, BasePopupUI> popupInstances = new Dictionary<string, BasePopupUI>();
    private const string popupPath = "UI/Popup/";
    [SerializeField] private Transform popupRoot;

    // PopUp 형식의 UI Stack
    public Stack<BasePopupUI> popupStack = new Stack<BasePopupUI>();

    [Header("Controller")]
    public ScenePopupController scenePopupController; // 씬 팝업 컨트롤러
    public NextSceneFade nextSceneFade; // 씬 전환 페이드
    public CustomMouseCursor customMouseCursor; // 커스텀 마우스 커서


    protected override void Awake()
    {
        base.Awake();
        RegisterAllPopupsInResources();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && popupStack.Count > 0)
        {
            BasePopupUI popup = popupStack.Peek();// Close() 메서드 내부에 Pop() 존재.
            popup.Close();
        }
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
        if (popupRoot == null)
        {
            Debug.Log("PopupUI 의 캔버스가 없습니다.");
            GameObject popupRootObj = GameObject.Find("Canvas/PopupUI");
            popupRoot = popupRootObj.transform;
        }

        while (popupStack.Count > 0)
        {
            var popup = popupStack.Pop();
            if (popup != null)
            {
                popup.gameObject.SetActive(false);
            }
        }
    }

    // 버튼 등의 외부에서 파라미터 값을 받아서 팝업 등장시키는 메서드
    public void ShowPopupByName(string popupName)
    {
        if (!popupTypeMap.TryGetValue(popupName, out var popupType))
        {
            Debug.LogError($"[UIManager] '{popupName}'에 해당하는 팝업 타입이 없습니다.");
            return;
        }

        MethodInfo showPopupGeneric = typeof(UIManager)
            .GetMethod(nameof(ShowPopup), BindingFlags.Public | BindingFlags.Instance);

        MethodInfo showPopupTyped = showPopupGeneric.MakeGenericMethod(popupType);
        showPopupTyped.Invoke(this, null);
    }
    /// <summary>
    /// 전투 보상을 띄워주는 팝업 UI 호출
    /// </summary>
    public void PopupRewardUI()
    {
        ShowPopupByName("PopupUI_CombatReward");
    }

    /// <summary>
    /// 전투 보상을 띄워주는 팝업 UI 호출
    /// </summary>
    public void PopupUnlockUI()
    {
        ShowPopupByName("UI_UnlockController");
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
            existingPoupup.gameObject.transform.SetAsLastSibling(); // 팝업이 열릴 때 가장 위로 오도록 설정
            return existingPoupup as T;
        }

        // Resources에서 프리팹 캐싱
        GameObject prefab = Resources.Load<GameObject>($"{popupPath}{popupName}");
        if (prefab == null)
        {
            Debug.LogError($"Resources에 '{popupName}'을 찾을 수 없습니다.");
            return null;
        }

        // 캐싱된 프리팹 생성
        GameObject popupObject = Instantiate(prefab, popupRoot);
        T popup = popupObject.GetComponent<T>();
        if (popup == null)
        {
            Debug.LogError($"프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");
            Destroy(popupObject);
            return null;
        }


        popupInstances[popupName] = popup;
        popup.Open();
        popup.gameObject.transform.SetAsLastSibling(); // 팝업이 열릴 때 가장 위로 오도록 설정
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

    public void RegisterPopup(ScenePopupController popcon)
    {
        scenePopupController = popcon;
    }
    public void UnregisterPopup()
    {
        scenePopupController = null;
    }

    /// <summary>
    /// Resources 폴더에서 모든 팝업 프리팹의 Type을 등록.
    /// </summary>
    private void RegisterAllPopupsInResources()
    {
        GameObject[] popupPrefabs = Resources.LoadAll<GameObject>(popupPath);

        foreach (GameObject prefab in popupPrefabs)
        {
            if (prefab.TryGetComponent<BasePopupUI>(out var popup))
            {
                string popupName = prefab.name;
                System.Type popupType = popup.GetType();

                if (!popupTypeMap.ContainsKey(popupName))
                {
                    popupTypeMap.Add(popupName, popupType);
                    // Debug.Log($"[UIManager] 등록됨: {popupName} → {popupType}");
                }
            }
            else
            {
                Debug.LogWarning($"[UIManager] {prefab.name} 프리팹에 BasePopupUI가 없음 → 등록되지 않음");
            }
        }
    }
}
