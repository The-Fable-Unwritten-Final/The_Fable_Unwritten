using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private static ToolTip instance;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public static void Show(string message, Vector2 screenPos)
    {
        if (instance == null) return;

        instance.gameObject.SetActive(true);
        instance.tooltipText.text = message;
        instance.UpdatePosition(screenPos);
    }

    public static void Hide()
    {
        if (instance != null)
            instance.gameObject.SetActive(false);
    }

    private void UpdatePosition(Vector2 screenPos)
    {
        Vector2 offset = new Vector2(230f, -50f);
        backgroundRect.position = screenPos + offset;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            UpdatePosition(Input.mousePosition);
        }
    }
}
