using UnityEngine;

public class MainCanvas_Camera : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void TrySetCamera(Camera cam)
    {
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        Debug.Log("[MainCanvas] 카메라 연결 완료: " + cam.name);
    }
}