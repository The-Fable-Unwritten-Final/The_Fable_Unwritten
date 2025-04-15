using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatCameraController : MonoBehaviour
{
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

    }
}
