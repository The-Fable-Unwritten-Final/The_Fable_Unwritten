using UnityEngine;


public class CameraProvider : MonoBehaviour
{
    public static CameraProvider Instance { get; private set; }
    public Camera UICamera { get; private set; }

    private void Awake()
    {
        Instance = this;
        UICamera = Camera.main;

        if (UICamera == null)
        {
            Debug.LogWarning("[CameraProvider] Camera.main is null.");
            return;
        }

        Debug.Log("[CameraProvider] 카메라 설정 완료: " + UICamera.name);

        // 씬마다 있는 CameraProvider가 MainCanvas를 찾아서 카메라 연결
        var canvasController = FindObjectOfType<MainCanvas_Camera>();
        if (canvasController != null)
        {
            canvasController.TrySetCamera(UICamera);
        }
        else
        {
            Debug.LogWarning("[CameraProvider] MainCanvas_Camera 찾을 수 없음.");
        }
    }
}