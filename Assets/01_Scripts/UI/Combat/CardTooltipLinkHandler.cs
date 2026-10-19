using TMPro;
using UnityEngine;

public class CardTooltipLinkHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private int currentLinkIndex = -1;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (text == null || !text.gameObject.activeInHierarchy)
            return;

        Vector2 mousePosition = Input.mousePosition;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            text,
            mousePosition,
            null
        );

        // 링크 위가 아님
        if (linkIndex == -1)
        {
            HideTooltip();
            return;
        }

        if (linkIndex < 0 || linkIndex >= text.textInfo.linkCount)
        {
            HideTooltip();
            return;
        }

        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
        string keywordType = linkInfo.GetLinkID();

        if (string.IsNullOrEmpty(keywordType))
        {
            HideTooltip();
            return;
        }

        // 새 링크에 들어왔을 때만 Show
        if (currentLinkIndex != linkIndex)
        {
            currentLinkIndex = linkIndex;

            StatusTooltipUI.Instance.Show(
                keywordType,
                mousePosition
            );
        }
    }

    private void HideTooltip()
    {
        if (currentLinkIndex == -1)
            return;

        currentLinkIndex = -1;

        if (StatusTooltipUI.Instance != null)
            StatusTooltipUI.Instance.Hide();
    }

    private void OnDisable()
    {
        HideTooltip();
    }
}