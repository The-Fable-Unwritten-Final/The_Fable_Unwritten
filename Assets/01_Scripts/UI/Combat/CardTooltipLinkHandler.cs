using TMPro;
using UnityEngine;

public class CardTooltipLinkHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private CardModel cardModel;
    private int currentLinkIndex = -1;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    // 카드 UI에서 카드 데이터 설정 시 호출
    public void SetCard(CardModel card)
    {
        HideTooltip();
        cardModel = card;
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
        if (currentLinkIndex != linkIndex)
        {
            currentLinkIndex = linkIndex;

            if (cardModel != null &&
                cardModel.TryGetStatusTooltipValue(keywordType, out string value))
            {
                StatusTooltipUI.Instance.Show(
                    keywordType,
                    mousePosition,
                    value
                );
            }
            else
            {
                StatusTooltipUI.Instance.Show(
                    keywordType,
                    mousePosition
                );
            }
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