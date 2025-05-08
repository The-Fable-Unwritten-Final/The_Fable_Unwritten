using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCanvas_Camera : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private static MainCanvas_Camera instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("[MainCanvas_Camera] 중복 객체 제거됨");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)// 메인 카메라 변경
    {
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
    }

    /*
    public void TrySetCamera(Camera cam)
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[MainCanvas_Camera] Canvas 컴포넌트를 찾지 못했습니다.");
                return;
            }
        }

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
    }*/
}
