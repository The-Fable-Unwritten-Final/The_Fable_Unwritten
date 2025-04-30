using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatLightingController : MonoBehaviour
{
    [SerializeField] Light mainLight; // 전투 조명
    public enum LightingState // 뒷배경 이미지 파일의 이름.
    {
        bg_1,
        bg_2,
        bg_3,
        bg_4,
        bg_5,
    }

    private void Awake()
    {
        GameManager.Instance.RegisterCombatLightingController(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    void OnSceneUnloaded(Scene scene)
    {
        GameManager.Instance.UnregisterCombatLightingController();
    }

    public void SetLighting(LightingState state)
    {
        // LightingState enum 값이 주어진 int 값으로 변환될 수 있는지 확인
        if (Enum.IsDefined(typeof(LightingState), state))
        {
            LightingState lightingState = (LightingState)state;

            // Enum 값을 기반으로 조명 색상 설정
            Color targetColor = mainLight.color; // 현재 조명 색상

            switch (lightingState)
            {
                case LightingState.bg_1:
                    ColorUtility.TryParseHtmlString("#FFC877", out targetColor);
                    break;
                case LightingState.bg_2:
                    ColorUtility.TryParseHtmlString("#80CFC8", out targetColor);
                    break;
                case LightingState.bg_3:
                    ColorUtility.TryParseHtmlString("#6FA2AB", out targetColor);
                    break;
                case LightingState.bg_4:
                    ColorUtility.TryParseHtmlString("#FFEFAF", out targetColor);
                    break;
                case LightingState.bg_5:
                    ColorUtility.TryParseHtmlString("#D3FFCE", out targetColor);
                    break;
            }
            mainLight.color = targetColor;
        }

        else
        {
            // 범위 밖의 값이 들어왔을 때 예외 처리
            Debug.LogError("Invalid LightingState value: " + state);
            // 예를 들어, 기본값을 설정하거나 예외를 던질 수 있습니다.
            mainLight.color = Color.white;  // 기본값을 흰색으로 설정
        }
    }
}
