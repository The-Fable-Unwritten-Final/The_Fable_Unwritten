using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatCameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera mainCam;
    [SerializeField] CinemachineVirtualCamera zoomCam;
    private void Awake()
    {
        GameManager.Instance.RegisterCombatCamera(this);
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
        GameManager.Instance.UnregisterCombatCamera();
    }

    public void CameraZoomInAction(Transform subject)
    {
        // 이후 플레이어의 동작에 맞춰 줌인/아웃 분리 호출.
        // 지금은 코루틴으로 임시 구현.
        StartCoroutine(ActionStart(subject));
    }

    IEnumerator ActionStart(Transform subject)
    {
        zoomCam.Follow = subject;
        mainCam.enabled = false;
        yield return new WaitForSeconds(2f);
        mainCam.enabled = true;
        zoomCam.Follow = null;
    }
}
