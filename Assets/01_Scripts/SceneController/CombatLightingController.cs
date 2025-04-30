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
        Color targetColor = mainLight.color; // 현재 조명 색상
        switch (state)
        {
            case LightingState.bg_1:// 기본 값 "#FFC877"
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
}
